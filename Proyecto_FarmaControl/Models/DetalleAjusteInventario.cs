
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class DetalleAjusteInventario
    {
        public int Id { get; set; }

        // ========================================
        // AJUSTE PRINCIPAL
        // ========================================

        [Range(1, int.MaxValue)]
        public int AjusteInventarioId { get; set; }

        public AjusteInventario AjusteInventario
        { get; set; } = null!;

        // ========================================
        // MEDICAMENTO
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un medicamento.")]
        public int MedicamentoId { get; set; }

        public Medicamento Medicamento
        { get; set; } = null!;

        // ========================================
        // LOTE
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un lote válido.")]
        public int LoteId { get; set; }

        public Lote Lote { get; set; } = null!;

        // ========================================
        // TIPO DE MOVIMIENTO
        // Entrada o Salida
        // ========================================

        [Required(ErrorMessage =
            "El tipo de movimiento es obligatorio.")]
        [StringLength(10)]
        public string TipoMovimiento { get; set; }
            = string.Empty;

        // ========================================
        // CANTIDAD A AJUSTAR
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }

        // ========================================
        // HISTORIAL DE EXISTENCIAS
        // ========================================

        // Estos valores se registran al aplicar
        // el ajuste, nunca desde el navegador.

        public int? ExistenciasAntes { get; set; }

        public int? ExistenciasDespues { get; set; }

        // ========================================
        // OBSERVACIONES DEL DETALLE
        // ========================================

        [StringLength(300)]
        public string? Observaciones { get; set; }
    }
}
