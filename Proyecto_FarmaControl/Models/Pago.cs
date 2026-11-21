using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Pago
    {
        public int Id { get; set; }

        public int? CompraId { get; set; }
        public Compra? Compra { get; set; }

        public int? VentaId { get; set; }
        public Venta? Venta { get; set; }

        [Range(1, int.MaxValue)]
        public int MetodoPagoId { get; set; }
        public MetodoPago MetodoPago { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int? TurnoCajaId { get; set; }
        public TurnoCaja? TurnoCaja { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        [Range(typeof(decimal), "0.01", "9999999999999999.99")]
        public decimal Monto { get; set; }

        [StringLength(100)]
        public string? Referencia { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Registrado";

        [StringLength(250)]
        public string? Observaciones { get; set; }
    }
}
