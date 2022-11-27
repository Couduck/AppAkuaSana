using AppPrueba;
using AppPrueba.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AkuaApp_v3.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DescargarVentasPage : ContentPage
    {
        private string AppFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public DescargarVentasPage()
        {
            InitializeComponent();
        }

        private async void DescargarVentasCSV_Clicked(object sender, EventArgs e)
        {
            List<Venta> VentasASacar = await App.BaseApp.RecuperarTodasLasVentasAsync();

            var src = DateTime.Now;

            string nombreArchivo = $"Ventas_AkuaSana_{src.Day}-{src.Month}-{src.Year}__{src.Hour}:{src.Minute}:{src.Second}.csv";
            string FilePath = Path.Combine(AppFolder, nombreArchivo);
            // string FilePath = Path.Combine(Environment.CurrentDirectory, nombreArchivo);

            using (var writer = new StreamWriter(FilePath))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(VentasASacar);
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