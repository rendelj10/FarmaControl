using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class TurnoCajaCerrarDto
    {
        [Required(ErrorMessage = "Debe indicar el monto contado en caja.")]
        [Range(typeof(decimal), "0", "9999999999999999.99",
            ErrorMessage = "El monto contado debe ser mayor o igual a cero.")]
        public decimal? MontoContado { get; set; }

        [StringLength(250,
            ErrorMessage = "Las observaciones no pueden exceder 250 caracteres.")]
        public string? Observaciones { get; set; }
    }
}
