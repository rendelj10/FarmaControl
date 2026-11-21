
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class DetalleCompra
    {
        public int Id { get; set; }

        // ========================================
        // COMPRA
        // ========================================

        [Range(1, int.MaxValue)]
        public int CompraId { get; set; }

        public Compra Compra { get; set; } = null!;

        // ========================================
        // MEDICAMENTO
        // ========================================

        [Range(1, int.MaxValue)]
        public int MedicamentoId { get; set; }

        public Medicamento Medicamento { get; set; } = null!;

        // ========================================
        // CANTIDAD Y COSTO
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }

        [Range(typeof(decimal), "0.01", "999999999.99",
            ErrorMessage = "El costo debe ser mayor que cero.")]
        public decimal CostoUnitario { get; set; }

        // Calculado por el servicio:
        // Cantidad * CostoUnitario.

        public decimal Subtotal { get; set; }

        // ========================================
        // INFORMACION DEL LOTE
        // ========================================

        // Puede completarse durante la recepción.
        // No se ingresará inventario hasta confirmar.

        [StringLength(50)]
        public string? NumeroLote { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        // Relación opcional con el lote
        // identificado al recibir la compra.

        public int? LoteId { get; set; }

        public Lote? Lote { get; set; }
    }
}
