using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class PagoGuardarDto
    {
        // Debe especificarse solamente CompraId o VentaId.
        [Range(1, int.MaxValue, ErrorMessage = "La compra debe ser válida.")]
        public int? CompraId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La venta debe ser válida.")]
        public int? VentaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un método de pago válido.")]
        public int MetodoPagoId { get; set; }

        public int? TurnoCajaId { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999.99",
            ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Monto { get; set; }

        [StringLength(100, ErrorMessage = "La referencia no puede superar 100 caracteres.")]
        public string? Referencia { get; set; }

        [StringLength(250, ErrorMessage = "Las observaciones no pueden superar 250 caracteres.")]
        public string? Observaciones { get; set; }
    }
}
