using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class LaboratorioGuardarDto
    {
        [Required(ErrorMessage = "El nombre del laboratorio es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Pais { get; set; }

        [StringLength(100)]
        public string? Contacto { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }
    }
}
