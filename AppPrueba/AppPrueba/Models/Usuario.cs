using SQLite;

namespace AppPrueba.Models
{
    [Table("Usuarios")]

    class Usuario
    {
        string Nombre { get; set; }

        string Password { get; set; }

        string TipoUsuario { get; set; }

        bool Status { get; set; }
    }
}