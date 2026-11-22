
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }

        // VENTA

        [Range(1, int.MaxValue)]
        public int VentaId { get; set; }

        public Venta Venta { get; set; } = null!;

        // MEDICAMENTO

        [Range(1, int.MaxValue)]
        public int MedicamentoId { get; set; }

        public Medicamento Medicamento { get; set; } = null!;

        // LOTE DEL CUAL SE DESCONTARA INVENTARIO

        [Range(1, int.MaxValue)]
        public int LoteId { get; set; }

        public Lote Lote { get; set; } = null!;

        // CANTIDAD VENDIDA

        [Range(1, int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }

        // PRECIO DE VENTA

        [Range(typeof(decimal), "0.01", "999999999.99")]
        public decimal PrecioUnitario { get; set; }

        // CALCULADO POR EL SERVICIO

        public decimal Subtotal { get; set; }
    }
}
