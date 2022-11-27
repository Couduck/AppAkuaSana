using AppPrueba;
using AppPrueba.Models;
using AppPrueba.ViewModels;
using Plugin.Geolocator;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AkuaApp_v3.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InsertarModificarClientePage : ContentPage
    {
        
        int Funcion;
        Cliente ClienteActualizar;
        bool coordenadasGuardadas = false, fotoGuardada = false;
        string RutaFoto,longitud, latitud;     //Almacenan la ruta de la foto tomada, la longitud y la latitud
        Plugin.Geolocator.Abstractions.IGeolocator localizar = CrossGeolocator.Current;
        byte[] ImagenEnBytes;

        public InsertarModificarClientePage()
        {
            InitializeComponent();
        }

        //Inicia cuando se pregunta si la clave se quiere de tal forma
        public InsertarModificarClientePage(string nuevaClave)
        {
            InitializeComponent();
            BindingContext = new InsertarModificarClienteViewModel(1);
            Funcion = 1;
            ClienteActualizar = null;
            ClaveCliente.Text = nuevaClave;
        }

        //Inicia cuando se quiere modificar
        public InsertarModificarClientePage(int funcionActual, Cliente clienteActualizar)
        {
            InitializeComponent();
            BindingContext = new InsertarModificarClienteViewModel(funcionActual);
            Funcion = funcionActual;
            ClienteActualizar = clienteActualizar;

            if(clienteActualizar != null)
            {
                CargarDatosCliente(clienteActualizar);
            }
        }

        //Se quiere tomar una foto
        private async void BotonFoto_Clicked(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(ClaveCliente.Text))
            {
                await DisplayAlert("AVISO", "Antes de tomar la foto se requiere ingresar una clave valida (CAMPO DE CLAVE VACIO)", "OK");
                return;
            }

            else
            {
                Regex sinEspacios = new Regex(@"[ ]+");

                if (sinEspacios.IsMatch(ClaveCliente.Text))
                {
                    await DisplayAlert("AVISO", "Antes de tomar la foto se requiere ingresar una clave valida (CLAVE POSEE ESPACIOS)", "OK");
                    return;
                }
            }

            if(Funcion == 1)
            {
                Cliente claveExistente = await App.BaseApp.RecuperarClientePorClave(ClaveCliente.Text);

                if (claveExistente != null)
                {
                    await DisplayAlert("AVISO", "Antes de tomar la foto se requiere ingresar una clave valida (CLAVE INGRESADA YA EXISTE EN BASE DE DATOS)", "OK");
                    return;
                }
            }
            

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

        //Se quiere tomar la ubicación GPS del teléfono
        private async void BotonGPS_Clicked(object sender, EventArgs e)
        {
            //Se define la exactitud del rastreo de GPS
            localizar.DesiredAccuracy = 50;

            //Evalua si los permisos de Geolocalizacion estan disponibles para la aplicación, si no, se pregunta si es que se desean activar los mismos, en caso de que no esten activados y se haya negado el permiso, la foto tomada se elimina
            if (!localizar.IsGeolocationAvailable | !localizar.IsGeolocationEnabled)
            {
                bool activarPermisosGPS = await DisplayAlert("ERROR", "La configuracion de GPS del dispositivo no está habilitada, ¿Desea activarla?", "SI", "NO");

                if (activarPermisosGPS)
                {
                    
                    PermissionStatus locationPermission1 = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                    if (locationPermission1 == PermissionStatus.Denied)
                    {
                        await DisplayAlert("ERROR", "Se requiere de activar los permisos del GPS para usar esta función, vuelva a intentarlo y acepte la solicitud", "ACEPTAR");
                        
                        return;
                    }

                    PermissionStatus locationPermission2 = await Permissions.RequestAsync<Permissions.LocationAlways>();

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

        private async void RegistrarCliente_Clicked(object sender, EventArgs e)
        {
            bool listoGuardar = await VerificarCampos();  //Se llama al método que verifica si hay algun campo vacío

            if (listoGuardar == false)       //En caso de faltar un campo se cancela el proceso
            {
                return;
            }

            RemoverEnter();     //En algunos casos puede haber un caracter de salto de linea al final de algunos campos, para evitar guardar los datos de manera incorrecta se elimina este caracter en caso de haberlo

            string clienteLista = ClienteEnLista();     //La información del prospecto se guarda como un texto separado por saltos de linea

            //Se muestra al usuario la información ingresada y se le pregunta si así desea guardar el prospecto
            bool guardarEnBD = await DisplayAlert("CONFIRMAR", "El nuevo prospecto tendra los siguientes atributos:" + clienteLista + "¿Es correcto?", "GUARDAR", "CANCELAR");

            if (guardarEnBD)
            {
                if(Funcion == 1)
                {
                    Cliente clienteNuevo = new Cliente
                    {
                        Clave = ClaveCliente.Text,
                        Status = "V",
                        Nombre = NombreCliente.Text,
                        Telefono = TelefonoCliente.Text,
                        Giro = GiroCliente.Text,
                        TipoGarrafon = GarrafonCliente.Text,
                        TipoFactura = "",
                        Calle = CalleCliente.Text,
                        NumExt = NumExtCliente.Text,
                        NumInt = NumIntCliente.Text,
                        Colonia = ColoniaCliente.Text,
                        Municipio = DelMunCliente.Text,
                        Estado = EstadoCliente.Text,
                        CodPost = CodPostCliente.Text,
                        Tipo = $"E{PrecioCliente.Text}",
                        Precio = decimal.Parse(PrecioCliente.Text),
                        FormaPago = FormaPagoCliente.Text
                    };

                    Imagen imagenNueva = new Imagen
                    {
                        Clave = ClaveCliente.Text
                    };

                    if(fotoGuardada)
                    {
                        //PENSAR EN COMO CONVERTIR EL STREAM A CADENA DE BYTES
                        imagenNueva.RutaTelefonoImagen = RutaFoto;
                        imagenNueva.CadenaImagen = ImagenEnBytes;
                        clienteNuevo.SwFoto = true;
                    }

                    if(coordenadasGuardadas)
                    {
                        clienteNuevo.Latitud = latitud;
                        clienteNuevo.Longitud = longitud;
                    }

                    await App.BaseApp.GuardarClienteAsync(clienteNuevo, false);
                    await App.BaseApp.GuardarImagenAsync(imagenNueva, false);
                }

                else
                {
                    Cliente clienteUpdate = await App.BaseApp.RecuperarClientePorClave(ClienteActualizar.Clave);

                    clienteUpdate.Nombre = NombreCliente.Text;
                    clienteUpdate.Telefono = TelefonoCliente.Text;
                    clienteUpdate.Giro = GiroCliente.Text;
                    clienteUpdate.TipoGarrafon = GarrafonCliente.Text;
                    clienteUpdate.Calle = CalleCliente.Text;
                    clienteUpdate.NumExt = NumExtCliente.Text;
                    clienteUpdate.NumInt = NumIntCliente.Text;
                    clienteUpdate.Colonia = ColoniaCliente.Text;
                    clienteUpdate.Municipio = DelMunCliente.Text;
                    clienteUpdate.Estado = EstadoCliente.Text;
                    clienteUpdate.CodPost = CodPostCliente.Text;
                    clienteUpdate.FormaPago = FormaPagoCliente.Text;

                    Imagen imagenUpdate = await App.BaseApp.RecuperarImagenPorClave(ClienteActualizar.Clave);

                    if(imagenUpdate == null)
                    {
                        imagenUpdate = new Imagen
                        {
                            Clave = ClienteActualizar.Clave
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

                    await App.BaseApp.GuardarClienteAsync(clienteUpdate, true);

                }
            }
        }

        


        async Task<bool> VerificarCampos()    //Verifica que ninguno de los campos obligatorios para ingresar o modificar el registro estén vacíos y cumplan sus reglas
        {
            bool todoCampoCorrecto = true;     //TRUE si todos los campos cumplen con las especificaciones dadas
            List<string> camposVacios = new List<string>();     //Lista que guarda el nombre de los campos que están vacíos
            List<string> errores = new List<string>();          //Lista que guarda el nombre de los campos que tienen errores

            //Si cualquiera de las variables esta vacía, la bool de todoCampoLleno se pone en FALSE y se añade el nombre del campo faltante a la lista
            if (string.IsNullOrWhiteSpace(ClaveCliente.Text))
            {
                todoCampoCorrecto = false;
                camposVacios.Add("\"Clave\"");
            }

            else
            {
                ClaveCliente.Text = ClaveCliente.Text.ToUpper();

                Regex sinEspacios = new Regex(@"[ ]+");

                if(sinEspacios.IsMatch(ClaveCliente.Text))
                {
                    todoCampoCorrecto = false;
                    errores.Add("\"La Clave no puede tener espacios blancos\"");
                }

                Cliente claveYaUsada = await App.BaseApp.RecuperarClientePorClave(ClaveCliente.Text);
                
                if (claveYaUsada != null & Funcion == 1)
                {
                    todoCampoCorrecto = false;
                    errores.Add("\"La Clave ingresada ya esta utilizada por otro cliente\"");
                }
            }

            if (string.IsNullOrWhiteSpace(NombreCliente.Text))
            {
                todoCampoCorrecto = false;
                camposVacios.Add("\"Nombre Negocio\"");
            }

            else
            {
                if (NombreCliente.Text.Length < 5)
                {
                    todoCampoCorrecto = false;
                    errores.Add("\"El Nombre debe ser mayor a 5 caracteres\"");
                }
            }

            if (string.IsNullOrWhiteSpace(TelefonoCliente.Text))
            {

                bool telefonoProporcionado = await DisplayAlert("AVISO", "Telefono no proporcionado, ¿Desea omitirlo?", "SI", "NO");

                //Si se omitió la calle por error, se niega la entrada
                if (!telefonoProporcionado)
                {
                    todoCampoCorrecto = false;
                    camposVacios.Add("\"Telefono\"");
                }

            }

            else
            {
                if (TelefonoCliente.Text.Length < 10)
                {
                    todoCampoCorrecto = false;
                    errores.Add("\"El Telefono del cliente debe ser de 10 digitos\"");
                }
            }

            if (string.IsNullOrWhiteSpace(GiroCliente.Text))
            {
                todoCampoCorrecto = false;
                camposVacios.Add("\"Giro\"");
            }

            if (string.IsNullOrWhiteSpace(CalleCliente.Text))
            {

                bool calleProporcionada = await DisplayAlert("AVISO", "Calle no proporcionada, ¿Desea omitirla?", "SI", "NO");

                //Si se omitió la calle por error, se niega la entrada
                if (!calleProporcionada)
                {
                    todoCampoCorrecto = false;
                    camposVacios.Add("\"Calle\"");
                }

            }

            else
            {
                if (string.IsNullOrWhiteSpace(NumExtCliente.Text))
                {
                    todoCampoCorrecto = false;
                    camposVacios.Add("\"Numero exterior\"");
                }

                if (string.IsNullOrWhiteSpace(NumIntCliente.Text))
                {
                    bool hayNumeroInt = await DisplayAlert("AVISO", "No se ha ingresado un numero interior, ¿el cliente posee uno?", "SI", "NO");

                    //Si hay un numero interno, significa que no se escribió y automáticamente se toma como un campo con información faltante
                    if (hayNumeroInt)
                    {
                        todoCampoCorrecto = false;
                        camposVacios.Add("\"Numero interior\"");
                    }

                    /*En caso de que no exista el numero, se coloca N/A en el campo de texto
                    else
                    {
                        NumIntCliente.Text = "N/A";
                    }*/
                }

                if (string.IsNullOrWhiteSpace(ColoniaCliente.Text))
                {
                    todoCampoCorrecto = false;
                    camposVacios.Add("\"Colonia\"");
                }

                if (string.IsNullOrWhiteSpace(DelMunCliente.Text))
                {
                    todoCampoCorrecto = false;
                    camposVacios.Add("\"Delegacion/Municipio\"");
                }

                if (string.IsNullOrWhiteSpace(EstadoCliente.Text))
                {
                    todoCampoCorrecto = false;
                    camposVacios.Add("\"Estado\"");
                }

                if (string.IsNullOrWhiteSpace(CodPostCliente.Text))
                {
                    todoCampoCorrecto = false;
                    camposVacios.Add("\"Codigo Postal\"");
                }
            }

            if (string.IsNullOrWhiteSpace(GarrafonCliente.Text))
            {
                todoCampoCorrecto = false;
                camposVacios.Add("\"Tipo de Garrafon\"");
            }

            if (string.IsNullOrWhiteSpace(PrecioCliente.Text))
            {
                todoCampoCorrecto = false;
                camposVacios.Add("\"Precio\"");
            }

            else
            {
                decimal.TryParse(PrecioCliente.Text, out decimal precio);

                if (precio < 11 | precio > 50)
                {
                    todoCampoCorrecto = false;
                    errores.Add("\"El Precio debe estar entre los 11 y los 50 pesos\"");
                }
            }

            //Si hay algun campo vacío, se manda un mensaje al usuario especificando que no se puede continuar el proceso y se especifican cuales son los campos que están vacíos
            if (camposVacios.Count > 0)
            {
                string totalVacios = camposVacios[0];

                for (byte i = 1; i < camposVacios.Count; i++)
                {
                    totalVacios += ", " + camposVacios[i];
                }

                totalVacios += ".";

                await DisplayAlert("ERROR", "Los siguientes campos se encuentran vacios: " + totalVacios, "ACEPTAR");
            }

            if (errores.Count >0)
            {
                string totalErrores = "\n\n";
                totalErrores += errores[0];

                for (byte i = 1; i < errores.Count; i++)
                {
                    totalErrores += "\n" + errores[i];
                }

                await DisplayAlert("ERROR", "La información proporcionada no cumple con el formato solicitado: " + totalErrores, "ACEPTAR");
            }

            //Devuelve el valor del bool
            return todoCampoCorrecto;
        }

        /*private void ClaveCliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex sinComas = new Regex("[,]+");

            if(sinComas.IsMatch(e.NewTextValue))
            {
                ClaveCliente.Text = e.OldTextValue;
            }
        }*/

        void RemoverEnter()
        {
            if (ClaveCliente.Text[ClaveCliente.Text.Length - 1] == '\n')
            {
                ClaveCliente.Text = ClaveCliente.Text.Remove(ClaveCliente.Text.Length - 1);
            }

            if (NombreCliente.Text[NombreCliente.Text.Length - 1] == '\n')
            {
                NombreCliente.Text = NombreCliente.Text.Remove(NombreCliente.Text.Length - 1);
            }

            if(!string.IsNullOrWhiteSpace(TelefonoCliente.Text))
            {
                if (TelefonoCliente.Text[TelefonoCliente.Text.Length - 1] == '\n')
                {
                    TelefonoCliente.Text = TelefonoCliente.Text.Remove(TelefonoCliente.Text.Length - 1);
                }
            }

            if (GiroCliente.Text[GiroCliente.Text.Length - 1] == '\n')
            {
                GiroCliente.Text = GiroCliente.Text.Remove(GiroCliente.Text.Length - 1);
            }

            if (!string.IsNullOrWhiteSpace(CalleCliente.Text))
            {
                if (CalleCliente.Text[CalleCliente.Text.Length - 1] == '\n')
                {
                    CalleCliente.Text = CalleCliente.Text.Remove(CalleCliente.Text.Length - 1);
                }

                if (NumExtCliente.Text[NumExtCliente.Text.Length - 1] == '\n')
                {
                    NumExtCliente.Text = NumExtCliente.Text.Remove(NumExtCliente.Text.Length - 1);
                }
                
                if (!string.IsNullOrWhiteSpace(NumIntCliente.Text))
                {
                    if (NumIntCliente.Text[NumIntCliente.Text.Length - 1] == '\n')
                    {
                        NumIntCliente.Text = NumIntCliente.Text.Remove(NumIntCliente.Text.Length - 1);
                    }
                }

                if (ColoniaCliente.Text[ColoniaCliente.Text.Length - 1] == '\n')
                {
                    ColoniaCliente.Text = ColoniaCliente.Text.Remove(ColoniaCliente.Text.Length - 1);
                }

                if (DelMunCliente.Text[DelMunCliente.Text.Length - 1] == '\n')
                {
                    DelMunCliente.Text = DelMunCliente.Text.Remove(DelMunCliente.Text.Length - 1);
                }

                if (EstadoCliente.Text[EstadoCliente.Text.Length - 1] == '\n')
                {
                    EstadoCliente.Text = EstadoCliente.Text.Remove(EstadoCliente.Text.Length - 1);
                }

                if (CodPostCliente.Text[CodPostCliente.Text.Length - 1] == '\n')
                {
                    CodPostCliente.Text = CodPostCliente.Text.Remove(CodPostCliente.Text.Length - 1);
                }
            }

            if (GarrafonCliente.Text[GarrafonCliente.Text.Length - 1] == '\n')
            {
                GarrafonCliente.Text = GarrafonCliente.Text.Remove(GarrafonCliente.Text.Length - 1);
            }

            if (PrecioCliente.Text[PrecioCliente.Text.Length - 1] == '\n')
            {
                PrecioCliente.Text = PrecioCliente.Text.Remove(PrecioCliente.Text.Length - 1);
            }

        }

        string ClienteEnLista()   //El prospecto se guarda en una sola string que separa la información por lienas
        {
            string clienteLista = "\n\nClave " + ClaveCliente.Text + "\n";
            clienteLista += "Nom. Cliente: " + NombreCliente.Text + "\n";

            if(!string.IsNullOrWhiteSpace(TelefonoCliente.Text))
            {
                clienteLista += "Telefono: " + TelefonoCliente.Text + "\n";
            }
            
            clienteLista += "Giro: " + GiroCliente.Text + "\n";

            if (!string.IsNullOrWhiteSpace(CalleCliente.Text))
            {
                clienteLista += "Calle: " + CalleCliente.Text + "\n";
                clienteLista += "Num. Exterior: " + NumExtCliente.Text + "\n";
                clienteLista += "Num. Interior: " + NumIntCliente.Text + "\n";
                clienteLista += "Colonia: " + ColoniaCliente.Text + "\n";
                clienteLista += "Municipio/Delegacion: " + DelMunCliente.Text + "\n";
                clienteLista += "Estado: " + EstadoCliente.Text + "\n";
                clienteLista += "Cod. Post.: " + CodPostCliente.Text + "\n";
            }

            clienteLista += "Garrafon: " + GarrafonCliente.Text + "\n";
            clienteLista += "Precio: " + PrecioCliente.Text + "\n\n";

            return clienteLista;
        }

        void CargarDatosCliente(Cliente clienteActualizar)
        {
            ClaveCliente.Text = clienteActualizar.Clave;
            ClaveCliente.IsEnabled = false;

            NombreCliente.Text = clienteActualizar.Nombre;
            TelefonoCliente.Text = clienteActualizar.Telefono;
            GiroCliente.Text = clienteActualizar.Giro;
            CalleCliente.Text = clienteActualizar.Calle;
            NumExtCliente.Text = clienteActualizar.NumExt;
            NumIntCliente.Text = clienteActualizar.NumInt;
            ColoniaCliente.Text = clienteActualizar.Colonia;
            DelMunCliente.Text = clienteActualizar.Municipio;
            EstadoCliente.Text = clienteActualizar.Estado;
            CodPostCliente.Text = clienteActualizar.CodPost;
            GarrafonCliente.Text = clienteActualizar.TipoGarrafon;
            TipoCliente.Text = clienteActualizar.Tipo;

            PrecioCliente.Text = clienteActualizar.Precio.ToString();
            PrecioCliente.IsEnabled = false;

            UltCompCliente.Text = clienteActualizar.FechaUltimaCompra;
            FormaPagoCliente.Text = clienteActualizar.FormaPago;
        }
    }
}