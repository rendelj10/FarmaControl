
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class SucursalGuardarDto
    {
        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Direccion { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        [StringLength(150)]
        public string? Email { get; set; }
    }
}
