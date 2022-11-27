using SQLite;

namespace AppPrueba.Models
{
    [Table("Clientes")]

    public class Cliente
    {
        [PrimaryKey, MaxLength(10)]
        public string Clave { get; set; }

        [MaxLength(1)]
        public string Status { get; set; }

        [MaxLength(35)]
        public string Nombre { get; set; }

        [MaxLength(10)]
        public string Telefono { get; set; }
        
        [MaxLength(15)]
        public string Giro { get; set; }

        [MaxLength(10)]
        public string TipoGarrafon { get; set; }

        [MaxLength(4)]
        public string TipoFactura { get; set; }

        [MaxLength(30)]
        public string Calle { get; set; }

        [MaxLength(10)]
        public string NumExt { get; set; }

        [MaxLength(10)]
        public string NumInt { get; set; }

        [MaxLength(30)]
        public string Colonia { get; set; }

        [MaxLength(30)]
        public string Municipio { get; set; }

        [MaxLength(20)]
        public string Estado { get; set; }

        [MaxLength(5)]
        public string CodPost { get; set; }

        [MaxLength(4)]
        public string Tipo { get; set; }

        [MaxLength(5)]
        public decimal Precio { get; set; }

        [MaxLength(10)]
        public string FechaUltimaCompra { get; set; }

        [MaxLength(1)]
        public string FormaPago { get; set; }

        public string Latitud { get; set; }

        public string Longitud { get; set; }

        public bool SwFoto { get; set; }
    }
}