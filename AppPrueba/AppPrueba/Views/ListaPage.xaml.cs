using AppPrueba;
using AppPrueba.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AkuaApp_v3.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListaPage : ContentPage
    {


        ObservableCollection<Cliente> lista;
        Cliente ClienteSeleccionado;

        public ListaPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()     //Cada vez que se llega nuevamente a esta página se actualiza
        {

            try
            {
                base.OnAppearing();
                List<Cliente> listaEnBase = await App.BaseApp.RecuperarTodosLosClientesAsync();
                IEnumerable<Cliente> listaEnOrden = listaEnBase.OrderBy(cliente => cliente.Clave);
                Lista_Clientes.ItemsSource = listaEnOrden.ToList();
                lista = new ObservableCollection<Cliente>(listaEnOrden.ToList());
            }

            catch
            {

            }
        }

        
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)       //Cada vez que el texto de la barra cambia
        {
            string textoNuevo = e.NewTextValue.ToUpper();
            Lista_Clientes.ItemsSource = lista.Where(s => s.Clave.StartsWith(textoNuevo));
            Lista_Clientes.SelectedItem = null; 
        }

        private async void SearchBarCliente_SearchButtonPressed(object sender, EventArgs e)         //Se presiona el boton de busqueda (ENTER) de la barra de busqueda
        {
            //Comprueba si hay algun espacio de en la barra
            Regex sinEspacios = new Regex(@"[ ]+");

            if (sinEspacios.IsMatch(SearchBarCliente.Text))
            {
                await DisplayAlert("ERROR", "No puede haber espacios en la barra de busquedas", "ENTENDIDO");
                return;
            }

            //Comprueba que lo que se busca no esté en la BD para preguntar si desea ingresarlo como algo nuevo
            Cliente claveExiste = await App.BaseApp.RecuperarClientePorClave(SearchBarCliente.Text);

            if(claveExiste == null)
            {
                bool nuevaClave = await DisplayAlert("NUEVA CLAVE", "La clave que usted ha ingresado no existe en la lista de clientes, ¿desea crear un nuevo cliente?", "SI", "NO");

                if(nuevaClave)
                {
                    await Navigation.PushAsync(new InsertarModificarClientePage(SearchBarCliente.Text));
                }
            }
        }

        private void Lista_Clientes_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (((ListView)sender).SelectedItem == null)
            {
                ModificarCliente.IsEnabled = false;
                RegistrarVenta.IsEnabled = false;
                return;
            }

            ClienteSeleccionado = Lista_Clientes.SelectedItem as Cliente;

            ModificarCliente.IsEnabled = true;
            RegistrarVenta.IsEnabled = true;
        }

        private async void AgregarCliente_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertarModificarClientePage(1,null));
            Lista_Clientes.SelectedItem = null;
        }

        private async void ModificarCliente_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertarModificarClientePage(2, ClienteSeleccionado));
            Lista_Clientes.SelectedItem = null;
        }

        private async void RegistrarVenta_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistrarNuevaVenta(ClienteSeleccionado));
            Lista_Clientes.SelectedItem = null;
        }
    }
}