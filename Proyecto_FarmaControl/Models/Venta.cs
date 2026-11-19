
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Venta
    {
        public int Id { get; set; }

        // CLIENTE

        [Range(1, int.MaxValue)]
        public int ClienteId { get; set; }

        public Cliente Cliente { get; set; } = null!;

        // ALMACEN DE SALIDA

        [Range(1, int.MaxValue)]
        public int AlmacenId { get; set; }

        public Almacen Almacen { get; set; } = null!;

        // DOCUMENTO

        [Required]
        [StringLength(50)]
        public string NumeroDocumento { get; set; }
            = string.Empty;

        // FECHAS

        public DateTime FechaVenta { get; set; }

        public DateTime? FechaConfirmacion { get; set; }

        // ESTADOS:
        // Borrador, Confirmada, Anulada

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Borrador";

        // IMPORTES CALCULADOS POR EL SERVICIO

        public decimal Subtotal { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }

        // OBSERVACIONES

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // DETALLES

        public ICollection<DetalleVenta> Detalles
        { get; set; } = new List<DetalleVenta>();
    }
}
