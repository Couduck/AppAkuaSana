using AppPrueba;
using AppPrueba.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AkuaApp_v3.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CargarClientes : ContentPage
    {
        public CargarClientes()
        {
            InitializeComponent();
        }

        //Click al botón de cargar un archivo CSV, se definen los parametros para solo aceptar arhivos de tipo CSV, si se ingresa el archivo, 
        private async void CargarClientesCSV_Clicked(object sender, EventArgs e)
        {
            //Parametros para cada SO
            FilePickerFileType tipoCSV =
                new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { ".csv" } },
                    { DevicePlatform.Android, new[] { "text/comma-separated-values" } },
                    { DevicePlatform.UWP, new[] { ".csv" } },
                });

            //Opciones del Pick del archivo
            PickOptions opciones = new PickOptions
            {
                PickerTitle = "Seleccione al archivo CSV que contenga a los clientes",
                FileTypes = tipoCSV,
            };

            //Accion de elegir el archivo
            FileResult ArchivoClientesCSV = await FilePicker.PickAsync(opciones);

            //Si se eligió uno, se procede
            if (ArchivoClientesCSV != null)
            {
                SacarValoresdeCSV(ArchivoClientesCSV.FullPath);
            }
        }

        //Saca los valores del CSV y los coloca en la Base de Datos
        async void SacarValoresdeCSV(string pathArchivo)
        {
            string[] contenido = File.ReadAllLines(pathArchivo);        //Se parte el archivo en renglones
            bool sinErrores = true;         //El archivo CSV no tiene errores

            //Se genera la lista de clientes y la de errores posibles
            List<Cliente> lista = new List<Cliente>();
            List<string> errores = new List<string>();

            //Se guarda cada registro en una lista temporal mientras se evalua, con que haya uno erroneo los datos no se cargan
            foreach (var registro in contenido)
            {
                string[] campos = registro.Split(',');

                if (campos[0] != "clave")
                {
                    try
                    {
                        Cliente cliente = new Cliente
                        {
                            Clave = campos[0],
                            Status = campos[1],
                            Nombre = campos[2],
                            Telefono = campos[3],
                            Giro = campos[4],
                            TipoGarrafon = campos[5],
                            TipoFactura = campos[6],
                            Calle = campos[7],
                            NumExt = campos[8],
                            NumInt = campos[9],
                            Colonia = campos[10],
                            Municipio = campos[11],
                            Estado = campos[12],
                            CodPost = campos[13],
                            Tipo = campos[14],
                            Precio = decimal.Parse(campos[15]),
                            FechaUltimaCompra = campos[16],
                            FormaPago = campos[17],
                            Latitud = campos[18],
                            Longitud = campos[19],
                            SwFoto = BitsToBools(campos[20])
                        };

                        lista.Add(cliente);

                    }

                    catch (Exception)
                    {
                        sinErrores = false;
                        errores.Add(campos[0]);
                    }
                }
            }

            //Si no hay errores, se actualiza la BD, de lo contrario, se muestran cuales fueron los registros que se guardaron mal
            if (sinErrores)
            {
                await App.BaseApp.BorrarTablaClientes();
                await App.BaseApp.CrearTablaClientes();

                foreach (Cliente cliente in lista)
                {
                    await App.BaseApp.GuardarClienteAsync(cliente, false);
                }
            }

            else
            {
                await DisplayAlert("ERROR DE CARGA", "Los datos no se actualizaron ya que se detectó un error en los registros con los registros con clave:\n" + RegistrosConErrores(errores) + "Corrija los errores y vuelva a intentarlo", "ACEPTAR");
            }
        }

        /*string EmptyToNotAvailable(string campo)
        {
            if(String.IsNullOrWhiteSpace(campo))
            {
                return "FALTA INFO.";
            }

            else
            {
                return campo;
            }
        }*/

        bool BitsToBools(string valor)
        {
            if(valor == "1" | valor == "true")
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        string RegistrosConErrores(List<string> registrosErrados)
        {
            string totalErrores = "\"" + registrosErrados[0] + "\"\n";

            for (int i = 1; i < registrosErrados.Count; i++)
            {
                totalErrores += "\"" + registrosErrados[i] + "\"\n";
            }

            totalErrores += "\n";

            return totalErrores;
        }
    }
}