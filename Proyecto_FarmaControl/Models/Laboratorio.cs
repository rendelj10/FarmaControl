using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Laboratorio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del laboratorio es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string? Pais { get; set; }

        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string? Contacto { get; set; }

        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string? Email { get; set; }

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
