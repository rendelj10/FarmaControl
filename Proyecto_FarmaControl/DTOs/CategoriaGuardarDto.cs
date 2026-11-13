
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class CategoriaGuardarDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100,
            ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250,
            ErrorMessage = "Máximo 250 caracteres.")]
        public string? Descripcion { get; set; }
    }
}
