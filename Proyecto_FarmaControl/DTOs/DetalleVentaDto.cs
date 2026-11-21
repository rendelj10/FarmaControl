
namespace Proyecto_FarmaControl.DTOs
{
    public class DetalleVentaDto
    {
        public int Id { get; set; }

        public int VentaId { get; set; }

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
        // IMPORTES
        // ========================================

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
