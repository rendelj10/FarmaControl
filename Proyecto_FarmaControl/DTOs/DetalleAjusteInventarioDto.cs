
namespace Proyecto_FarmaControl.DTOs
{
    public class DetalleAjusteInventarioDto
    {
        public int Id { get; set; }

        // ========================================
        // AJUSTE PRINCIPAL
        // ========================================

        public int AjusteInventarioId { get; set; }

        // ========================================
        // MEDICAMENTO
        // ========================================

        public int MedicamentoId { get; set; }

        public string MedicamentoNombre { get; set; }
            = string.Empty;

        // ========================================
        // LOTE
        // ========================================

        public int LoteId { get; set; }

        public string NumeroLote { get; set; }
            = string.Empty;

        // ========================================
        // MOVIMIENTO
        // ========================================

        public string TipoMovimiento { get; set; }
            = string.Empty;

        public int Cantidad { get; set; }

        // ========================================
        // HISTORIAL DE EXISTENCIAS
        // ========================================

        // Serán null mientras el ajuste
        // permanezca en Borrador.

        public int? ExistenciasAntes { get; set; }

        public int? ExistenciasDespues { get; set; }

        // ========================================
        // OBSERVACIONES
        // ========================================

        public string? Observaciones { get; set; }
    }
}
