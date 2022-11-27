using AppPrueba;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AkuaApp_v3.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogInPage : ContentPage
    {
        public LogInPage()
        {
            InitializeComponent();
            Usuario.Text = "AKUA001";
            Clave.Text = "123456";
        }

        //Solucion provicional al LogIn, solamente contando con un usuario válido
        private async void Ingresar_Clicked(object sender, EventArgs e)
        {
            if (Usuario.Text == "AKUA001" & Clave.Text == "123456")
            {
                Application.Current.MainPage = new AppShell2(); 
            }

            else
            {
                await DisplayAlert("ERROR", "El usuario o la contraseña son incorrectos, intentelo de nuevo", "ACEPTAR");
            }
        }
    }
}