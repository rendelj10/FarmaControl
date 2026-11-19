
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Introduce un correo válido.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
    }
}
