
namespace Proyecto_FarmaControl.DTOs
{
    public class AjusteInventarioDto
    {
        public int Id { get; set; }

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

        public DateTime FechaAjuste { get; set; }

        public DateTime? FechaAplicacion { get; set; }

        // ========================================
        // ESTADO
        // ========================================

        // Borrador, Aplicado o Anulado.

        public string Estado { get; set; }
            = string.Empty;

        // ========================================
        // MOTIVO
        // ========================================

        public string Motivo { get; set; }
            = string.Empty;

        // ========================================
        // OBSERVACIONES
        // ========================================

        public string? Observaciones { get; set; }

        // ========================================
        // DETALLES
        // ========================================

        public List<DetalleAjusteInventarioDto>
            Detalles
        { get; set; } = new();
    }
}
