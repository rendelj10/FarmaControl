
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class DetalleVentaGuardarDto
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
        // CANTIDAD
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }

        // ========================================
        // PRECIO UNITARIO
        // ========================================

        [Range(
            typeof(decimal),
            "0.01",
            "999999999.99",
            ErrorMessage = "El precio unitario debe ser mayor que cero.")]
        public decimal PrecioUnitario { get; set; }
    }
}
