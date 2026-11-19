
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class DetalleAjusteInventarioGuardarDto
    {
        // ========================================
        // MEDICAMENTO
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un medicamento válido.")]
        public int MedicamentoId { get; set; }

        // ========================================
        // LOTE
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un lote válido.")]
        public int LoteId { get; set; }

        // ========================================
        // TIPO DE MOVIMIENTO
        // ========================================

        // Valores permitidos:
        // Entrada o Salida.
        //
        // El servicio validará que el valor
        // corresponda exactamente a uno
        // de estos movimientos.

        [Required(ErrorMessage =
            "El tipo de movimiento es obligatorio.")]
        [StringLength(10)]
        public string TipoMovimiento { get; set; }
            = string.Empty;

        // ========================================
        // CANTIDAD
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }

        // ========================================
        // OBSERVACIONES
        // ========================================

        [StringLength(300)]
        public string? Observaciones { get; set; }
    }
}
