
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(30)]
        public string? DocumentoIdentidad { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
