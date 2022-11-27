using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppPrueba;
using AppPrueba.Models;
using Plugin.Geolocator;
using Plugin.Media;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AkuaApp_v3.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistrarNuevaVenta : ContentPage
    {
        //Variables de la página para registrar ventas
        string RutaFoto, longitud, latitud, FormaPago;
        bool coordenadasGuardadas = false, fotoGuardada = false;
        Plugin.Geolocator.Abstractions.IGeolocator localizar = CrossGeolocator.Current;
        byte[] ImagenEnBytes;

        public RegistrarNuevaVenta()
        {
            InitializeComponent();
        }

        //Constructor, requiere del cliente al cual se le realizará la nueva venta
        public RegistrarNuevaVenta(Cliente clienteSeleccionado)
        {
            InitializeComponent();

            ClaveCliente.Text = clienteSeleccionado.Clave;
            PrecioGarrafon.Text = clienteSeleccionado.Precio.ToString();
            TotalVenta.Text = "0";
        }


        private async void BotonFoto_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            try
            {
                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Name = $"Foto{ClaveCliente.Text}.jpg",     //Se le pone nombre a la foto
                    Directory = "Sample",           //Se le pone directorio
                    SaveToAlbum = true,             //Se indica que si se quiere guardar la foto en el album del celular

                });

                if (file == null)   //Si no se tomó la foto, el proceso se cancela
                {
                    return;
                }


                //Se guarda la dirección de la foto en el celular
                RutaFoto = file.Path;

                fotoGuardada = true;

                using (MemoryStream memory = new MemoryStream())
                {

                    Stream stream = file.GetStream();
                    stream.CopyTo(memory);
                    ImagenEnBytes = memory.ToArray();
                }

            }

            catch (Plugin.Media.Abstractions.MediaPermissionException) //Esto ocurre si el usuario niega los permisos 
            {
                await DisplayAlert("ERROR", "Se requiere activar todos los permisos de la camara para usar esta función, vuelva a intentarlo y acepte la solicitud", "ACEPTAR");
                return;
            }
        }

        private async void BotonGPS_Clicked(object sender, EventArgs e)
        {
            localizar.DesiredAccuracy = 50;

            //Evalua si los permisos de Geolocalizacion estan disponibles para la aplicación, si no, se pregunta si es que se desean activar los mismos, en caso de que no esten activados y se haya negado el permiso, la foto tomada se elimina
            if (!localizar.IsGeolocationAvailable | !localizar.IsGeolocationEnabled)
            {
                bool activarPermisosGPS = await DisplayAlert("ERROR", "La configuracion de GPS del dispositivo no está habilitada, ¿Desea activarla?", "SI", "NO");

                if (activarPermisosGPS)
                {
                    PermissionStatus locationPermission1 = await Permissions.RequestAsync<Permissions.LocationAlways>();

                    if (locationPermission1 == PermissionStatus.Denied)
                    {
                        await DisplayAlert("ERROR", "Se requiere de activar los permisos del GPS para usar esta función, vuelva a intentarlo y acepte la solicitud", "ACEPTAR");
                        return;
                    }

                    PermissionStatus locationPermission2 = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                    if (locationPermission2 == PermissionStatus.Denied)
                    {
                        await DisplayAlert("ERROR", "Se requiere de activar los permisos del GPS para usar esta función, vuelva a intentarlo y acepte la solicitud", "ACEPTAR");
                        locationPermission1 = PermissionStatus.Denied;
                        return;
                    }
                }

                else
                {
                    await DisplayAlert("ERROR", "Se requiere de activar los permisos del GPS para usar esta función, vuelva a intentarlo y acepte la solicitud", "ACEPTAR");
                    return;
                }

            }

            //Si la aplicación actualmente NO se encuentra "escuchando" la ubicación del dispositivo
            if (localizar.IsListening == false)
            {
                //Se empieza a rastrear la localización, esta ubicacion la actualiza cada 10 segundos
                await CrossGeolocator.Current.StartListeningAsync(new TimeSpan(0, 0, 10), 0.5);
            }

            //Se adquiere la ubicación actual
            var ubicacion = await CrossGeolocator.Current.GetPositionAsync();

            //Los valores se guardan en variables de tipo String
            longitud = ubicacion.Longitude.ToString();
            latitud = ubicacion.Latitude.ToString();

            coordenadasGuardadas = true;
        }

        private async void RegistrarVenta_Clicked(object sender, EventArgs e)
        {
            //Si no se ha seleccionado forma de pago, no se puede seguir
            if(FormaPago == null)
            {
                await DisplayAlert("ERROR", "Seleccione un metodo de pago", "OK");
                return;
            }

            //Se tiene que haber ingresado una cantidad de garrafones vendidos para tomarse en cuenta
            if(!string.IsNullOrWhiteSpace(CantidadVenta.Text))
            {
                decimal cantidad = decimal.Parse(CantidadVenta.Text);

                //La cantidad no puede ser menor a 1
                if(cantidad > 0)
                {
                    string formaPagoChar = "";
                    formaPagoChar += FormaPago[0];

                    Cliente clienteUpdate = await App.BaseApp.RecuperarClientePorClave(ClaveCliente.Text);

                    //Se tiene que generar la nueva venta y guardarse

                    Venta nuevaVenta = new Venta
                    {
                        Cliente = ClaveCliente.Text,
                        Precio = decimal.Parse(PrecioGarrafon.Text),
                        Cantidad = int.Parse(CantidadVenta.Text),
                        Total = decimal.Parse(TotalVenta.Text),
                        TipoPago = formaPagoChar
                    };

                    //Se tienen que guardar la info de la foto y del GPS si es que se tomaron
                    Imagen imagenUpdate = await App.BaseApp.RecuperarImagenPorClave(clienteUpdate.Clave);

                    if (imagenUpdate == null)
                    {
                        imagenUpdate = new Imagen
                        {
                            Clave = clienteUpdate.Clave
                        };

                        await App.BaseApp.GuardarImagenAsync(imagenUpdate, false);
                    }

                    if (fotoGuardada)
                    {
                        imagenUpdate.RutaTelefonoImagen = RutaFoto;
                        imagenUpdate.CadenaImagen = ImagenEnBytes;
                        clienteUpdate.SwFoto = true;
                        await App.BaseApp.GuardarImagenAsync(imagenUpdate, true);
                    }

                    if (coordenadasGuardadas)
                    {
                        clienteUpdate.Latitud = latitud;
                        clienteUpdate.Longitud = longitud;
                    }

                    //Se debe actualizar la información del cliente (fecha de ultima compra)

                    string date = DateTime.UtcNow.ToString("dd/MM/yyyy");
                    clienteUpdate.FechaUltimaCompra = date;

                    await App.BaseApp.GuardarClienteAsync(clienteUpdate, true);
                    await App.BaseApp.GuardarVentaAsync(nuevaVenta, false);

                }

                else
                {
                    await DisplayAlert("ERROR", "La cantidad de garrafones minima permitida es 1", "OK");
                }
            }

            else
            {
                await DisplayAlert("ERROR", "No se ha ingresado ninguna cantidad de garrafones.", "OK");
            }
        }

        private void CantidadVenta_TextChanged(object sender, TextChangedEventArgs e)
        {
            Int32.TryParse(CantidadVenta.Text, out int cantidad);
            decimal totalActual = cantidad * decimal.Parse(PrecioGarrafon.Text);
            TotalVenta.Text = totalActual.ToString();
        }

        private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            FormaPago = ((RadioButton)sender).Content.ToString();
        }
    }
}