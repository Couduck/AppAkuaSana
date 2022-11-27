using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppPrueba
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell2 : Xamarin.Forms.Shell
    {
        public AppShell2()
        {
            InitializeComponent();
            
        }

        private void CerrarSesion_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new AppShell();
        }

        private void MenuPrincipal_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new AppShell2();
        }
    }
}