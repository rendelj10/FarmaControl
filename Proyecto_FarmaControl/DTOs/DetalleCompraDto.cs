
namespace Proyecto_FarmaControl.DTOs
{
    public class DetalleCompraDto
    {
        // ========================================
        // IDENTIFICADORES
        // ========================================

        public int Id { get; set; }

        public int CompraId { get; set; }

        // ========================================
        // MEDICAMENTO
        // ========================================

        public int MedicamentoId { get; set; }

        public string MedicamentoNombre { get; set; }
            = string.Empty;

        // ========================================
        // CANTIDAD Y COSTO
        // ========================================

        public int Cantidad { get; set; }

        public decimal CostoUnitario { get; set; }

        public decimal Subtotal { get; set; }

        // ========================================
        // LOTE
        // ========================================

        public string? NumeroLote { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        public int? LoteId { get; set; }
    }
}
