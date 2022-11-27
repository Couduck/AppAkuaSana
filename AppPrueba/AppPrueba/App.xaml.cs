using AppPrueba.Data;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppPrueba
{
    public partial class App : Application
    {

        private static SQLiteHelper DB;

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        public static SQLiteHelper BaseApp  //Es la conexión que se establece entre la aplicación y la Base de Datos, es estática y por ello, accesible a través de cada pantalla de la aplicación
        {
            get
            {
                if (DB == null)
                {
                    DB = new SQLiteHelper(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DB_AkuaApp"));
                }

                return DB;
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}