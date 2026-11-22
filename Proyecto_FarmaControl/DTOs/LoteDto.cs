
namespace Proyecto_FarmaControl.DTOs
{
    public class LoteDto
    {
        public int Id { get; set; }

        public int MedicamentoId { get; set; }

        public string MedicamentoNombre { get; set; } = string.Empty;

        public string NumeroLote { get; set; } = string.Empty;

        public int AlmacenId { get; set; }

        public string AlmacenNombre { get; set; } = string.Empty;

        public int CantidadInicial { get; set; }

        public int Existencias { get; set; }

        public decimal CostoUnitario { get; set; }

        public DateTime FechaIngreso { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public bool Activo { get; set; }
    }
}
