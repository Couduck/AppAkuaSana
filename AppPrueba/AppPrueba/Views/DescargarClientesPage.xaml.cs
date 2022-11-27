using AppPrueba;
using AppPrueba.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AkuaApp_v3.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DescargarClientesPage : ContentPage
    {
        private string AppFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public DescargarClientesPage()
        {
            InitializeComponent();
        }

        private async void DescargarClientesCSV_Clicked(object sender, EventArgs e)
        {
            List<Cliente> ClientesASalir = await App.BaseApp.RecuperarTodosLosClientesAsync();
            IEnumerable<Cliente> clientesOrdenados= ClientesASalir.OrderBy(cliente => cliente.Clave);

            var src = DateTime.Now;

            string nombreArchivo = $"Clientes_AkuaSana_{src.Day}-{src.Month}-{src.Year}__{src.Hour}:{src.Minute}:{src.Second}.csv";
            string FilePath = Path.Combine(AppFolder, nombreArchivo);

            using (var writer = new StreamWriter(FilePath))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(clientesOrdenados);
                }
            }
            
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Exportar clientes",
                File = new ShareFile(FilePath)
            });

        }
    }
}