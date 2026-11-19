
namespace Proyecto_FarmaControl.DTOs
{
    public class VentaDto
    {
        public int Id { get; set; }

        // ========================================
        // CLIENTE
        // ========================================

        public int ClienteId { get; set; }

        public string ClienteNombre { get; set; }
            = string.Empty;

        // ========================================
        // ALMACEN
        // ========================================

        public int AlmacenId { get; set; }

        public string AlmacenNombre { get; set; }
            = string.Empty;

        // ========================================
        // DOCUMENTO
        // ========================================

        public string NumeroDocumento { get; set; }
            = string.Empty;

        // ========================================
        // FECHAS
        // ========================================

        public DateTime FechaVenta { get; set; }

        public DateTime? FechaConfirmacion { get; set; }

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

        public List<DetalleVentaDto> Detalles
        { get; set; } = new();
    }
}
