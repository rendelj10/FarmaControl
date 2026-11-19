
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class DetalleCompraGuardarDto
    {
        // ========================================
        // MEDICAMENTO
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un medicamento válido.")]
        public int MedicamentoId { get; set; }

        // ========================================
        // CANTIDAD
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }

        // ========================================
        // COSTO UNITARIO
        // ========================================

        [Range(
            typeof(decimal),
            "0.01",
            "999999999.99",
            ErrorMessage = "El costo unitario debe ser mayor que cero.")]
        public decimal CostoUnitario { get; set; }

        // ========================================
        // INFORMACION DEL LOTE
        // ========================================

        [StringLength(50,
            ErrorMessage = "El número de lote no puede superar 50 caracteres.")]
        public string? NumeroLote { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaVencimiento { get; set; }
    }
}
