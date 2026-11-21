using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class MetodoPago
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del método de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string? Descripcion { get; set; }

        public bool RequiereReferencia { get; set; } = false;

        public bool Activo { get; set; } = true;
    }
}
