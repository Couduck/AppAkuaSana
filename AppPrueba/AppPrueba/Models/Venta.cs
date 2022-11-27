using SQLite;

namespace AppPrueba.Models
{
    [Table("Ventas")]

    public class Venta
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [MaxLength(10)]
        public string Cliente { get; set; }

        public decimal Precio { get; set; }

        public int Cantidad { get; set; }

        public decimal Total { get; set; }

        [MaxLength(1)]
        public string TipoPago { get; set; }
    }
}