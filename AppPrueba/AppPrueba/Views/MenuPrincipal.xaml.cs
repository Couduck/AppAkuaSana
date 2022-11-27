using AppPrueba.Models;
using AppPrueba;
using AppPrueba.Models;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AkuaApp_v3.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPrincipal : ContentPage
    {
        public MenuPrincipal()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()     //Cada vez que se llega nuevamente a esta página se actualiza
        { 

            try
            {
                base.OnAppearing();
                List<Cliente> listaClientes = await App.BaseApp.RecuperarTodosLosClientesAsync();   //Se recuperan todos los prospectos de la BD
                List<Venta> listaVentas = await App.BaseApp.RecuperarTodasLasVentasAsync();   //Se recuperan todos los prospectos de la BD
                List<Imagen> listaImagen = await App.BaseApp.RecuperarTodasLasImagenesAsync();   //Se recuperan todos los prospectos de la BD
            }

            catch
            {

            }
        }
        private async void Transacciones_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ListaPage());
        }
    }
}