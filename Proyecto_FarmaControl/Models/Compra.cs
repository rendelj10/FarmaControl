
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Compra
    {
        public int Id { get; set; }

        // ========================================
        // PROVEEDOR
        // ========================================

        [Range(1, int.MaxValue)]
        public int ProveedorId { get; set; }

        public Proveedor Proveedor { get; set; } = null!;

        // ========================================
        // ALMACEN DESTINO
        // ========================================

        [Range(1, int.MaxValue)]
        public int AlmacenId { get; set; }

        public Almacen Almacen { get; set; } = null!;

        // ========================================
        // DOCUMENTO
        // ========================================

        [Required]
        [StringLength(50)]
        public string NumeroDocumento { get; set; }
            = string.Empty;

        // ========================================
        // FECHAS
        // ========================================

        public DateTime FechaCompra { get; set; }

        // Se registra al confirmar la recepcion.
        // Se guarda en UTC.

        public DateTime? FechaRecepcion { get; set; }

        // ========================================
        // ESTADO
        // ========================================

        // Borrador
        // Recibida
        // Anulada

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Borrador";

        // ========================================
        // TOTALES
        // ========================================

        public decimal Subtotal { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }

        // ========================================
        // OBSERVACIONES
        // ========================================

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // ========================================
        // DETALLES
        // ========================================

        public ICollection<DetalleCompra> Detalles
        { get; set; } = new List<DetalleCompra>();
    }
}
