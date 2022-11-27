using AppPrueba.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace AppPrueba.Data
{
    public class SQLiteHelper
    {
        public static SQLiteAsyncConnection DB; //Permite la conexión entre la aplicación y la base de datos

        //Se crea la tabla de datos de la clase Autor al inicializarse en caso de que esta tabla no exista
        public SQLiteHelper(string DBPath)
        {
            DB = new SQLiteAsyncConnection(DBPath);
            DB.CreateTableAsync<Cliente>();   //Se crea la tabla si es que esta no existe en la base de datos
            DB.CreateTableAsync<Imagen>();   //Se crea la tabla si es que esta no existe en la base de datos
            DB.CreateTableAsync<Venta>();   //Se crea la tabla si es que esta no existe en la base de datos
        }

        //DB.DropTableAsync<Autor>();  //Este comando permite eliminar la tabla, si desea eliminarla, quite las lineas de inicio del comentario y coloquelas en CreateTable, cuando lo inicie, todos los datos de la base de datos habrán sido eliminados

        //Permite insertar o modificar un registro dentro de la BD, todo dependiendo de si el ID del autor que recibe el método como parametro es 0 o no lo es
        public Task<int> GuardarClienteAsync(Cliente cliente, bool clienteYaExiste)
        {
            if (!clienteYaExiste)
            {
                return DB.InsertAsync(cliente);
            }

            else
            {
                return DB.UpdateAsync(cliente);
            }
        }

        //Recupera todos los registros dentro de la BD
        public Task<List<Cliente>> RecuperarTodosLosClientesAsync()
        {
            return DB.Table<Cliente>().ToListAsync();
        }

        //Recupera un registro de la BD buscandolo por medio de su ID
        public Task<Cliente> RecuperarClientePorClave(string clave)
        {
            return DB.Table<Cliente>().Where(a => a.Clave == clave).FirstOrDefaultAsync();
        }

        //Borra un registro en base a su ID
        public Task<int> BorrarCliente(string clave)
        {
            return DB.DeleteAsync<Cliente>(clave);
        }

        public Task<int> BorrarTablaClientes()
        {
            return DB.DropTableAsync<Cliente>();
        }

        public Task<CreateTableResult> CrearTablaClientes()
        {
            return DB.CreateTableAsync<Cliente>();
        }

        /**
         * 
         *  SECCIÓN DE IMAGENES
         *  
         */

        public Task<int> GuardarImagenAsync(Imagen imagen, bool imagenYaExiste)
        {
            if (!imagenYaExiste)
            {
                return DB.InsertAsync(imagen);
            }

            else
            {
                return DB.UpdateAsync(imagen);
            }
        }

        //Recupera todos los registros dentro de la BD
        public Task<List<Imagen>> RecuperarTodasLasImagenesAsync()
        {
            return DB.Table<Imagen>().ToListAsync();
        }

        //Recupera un registro de la BD buscandolo por medio de su ID
        public Task<Imagen> RecuperarImagenPorClave(string clave)
        {
            return DB.Table<Imagen>().Where(a => a.Clave == clave).FirstOrDefaultAsync();
        }

        //Borra un registro en base a su ID
        public Task<int> BorrarImagen(string clave)
        {
            return DB.DeleteAsync<Imagen>(clave);
        }

        public Task<int> BorrarTablaImagenes()
        {
            return DB.DropTableAsync<Imagen>();
        }

        public Task<CreateTableResult> CrearTablaImagenes()
        {
            return DB.CreateTableAsync<Imagen>();
        }


        /**
         * 
         *  SECCIÓN DE VENTAS
         *  
         */

        public Task<int> GuardarVentaAsync(Venta venta, bool VentaYaExiste)
        {
            if (!VentaYaExiste)
            {
                return DB.InsertAsync(venta);
            }

            else
            {
                return DB.UpdateAsync(venta);
            }
        }

        //Recupera todos los registros dentro de la BD
        public Task<List<Venta>> RecuperarTodasLasVentasAsync()
        {
            return DB.Table<Venta>().ToListAsync();
        }

        //Recupera un registro de la BD buscandolo por medio de su ID
        public Task<Venta> RecuperarVentaPorClave(int ID)
        {
            return DB.Table<Venta>().Where(a => a.ID == ID).FirstOrDefaultAsync();
        }

        //Borra un registro en base a su ID
        public Task<int> BorrarVenta(string clave)
        {
            return DB.DeleteAsync<Venta>(clave);
        }

        public Task<int> BorrarTablaVentas()
        {
            return DB.DropTableAsync<Venta>();
        }

        public Task<CreateTableResult> CrearTablaVentas()
        {
            return DB.CreateTableAsync<Venta>();
        }
    }
}