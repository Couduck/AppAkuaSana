using SQLite;

namespace AppPrueba.Models
{
    [Table("Imagenes")]

    public class Imagen
    {
        [PrimaryKey, MaxLength(10)]
        public string Clave { get; set; }

        public string RutaTelefonoImagen { get; set; }

        public byte[] CadenaImagen { get; set; }

        public string Fotografo { get; set; }
    }
}