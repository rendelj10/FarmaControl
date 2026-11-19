
namespace Proyecto_FarmaControl.DTOs
{
    public class CompraDto
    {
        public int Id { get; set; }

        // ========================================
        // PROVEEDOR
        // ========================================

        public int ProveedorId { get; set; }

        public string ProveedorNombre { get; set; }
            = string.Empty;

        // ========================================
        // ALMACEN
        // ========================================

        public int AlmacenId { get; set; }

        public string AlmacenNombre { get; set; }
            = string.Empty;

        // ========================================
        // DOCUMENTO Y FECHAS
        // ========================================

        public string NumeroDocumento { get; set; }
            = string.Empty;

        public DateTime FechaCompra { get; set; }

        public DateTime? FechaRecepcion { get; set; }

        // ========================================
        // ESTADO
        // ========================================

        public string Estado { get; set; }
            = string.Empty;

        // ========================================
        // TOTALES
        // ========================================

        public decimal Subtotal { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }

        // ========================================
        // OBSERVACIONES
        // ========================================

        public string? Observaciones { get; set; }

        // ========================================
        // DETALLES
        // ========================================

        public List<DetalleCompraDto> Detalles
        { get; set; } = new();
    }
}
