using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class MetodoPagoGuardarDto
    {
        [Required(ErrorMessage = "El nombre del método de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string? Descripcion { get; set; }

        public bool RequiereReferencia { get; set; }
    }
}
