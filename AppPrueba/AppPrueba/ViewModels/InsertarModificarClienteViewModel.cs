using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace AppPrueba.ViewModels
{
    public class InsertarModificarClienteViewModel : BindableObject
    {
        public InsertarModificarClienteViewModel()
        {

        }

        public InsertarModificarClienteViewModel(int funcion)
        {
            IntercalarInsertUpdate(funcion);
        }

        Color colorBarraNav = Color.White;
        string textoBarra = "Texto";

        //Metodos para notificar el cambio de valores
        public Color ColorBarraNav
        {
            get => colorBarraNav;
            set
            {
                if (value == colorBarraNav)
                {
                    return;
                }

                colorBarraNav = value;
                OnPropertyChanged();
            }
        }
        public string TextoBarra
        {
            get => textoBarra;
            set
            {
                if (value == textoBarra)
                {
                    return;
                }

                textoBarra = value;
                OnPropertyChanged();
            }
        }
        
        void IntercalarInsertUpdate(int funcion)
        {
            switch(funcion)
            {
                case 1:
                    TextoBarra = "Agregar Cliente";
                    break;

                case 2:
                    TextoBarra = "Actualizar Cliente";
                    break;
            }
        }
    }
}