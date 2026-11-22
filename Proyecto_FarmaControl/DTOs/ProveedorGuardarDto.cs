
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class ProveedorGuardarDto
    {
        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string RazonSocial { get; set; } = string.Empty;

        [StringLength(150)]
        public string? NombreComercial { get; set; }

        [StringLength(20)]
        public string? Ruc { get; set; }

        [StringLength(100)]
        public string? Contacto { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }
    }
}
