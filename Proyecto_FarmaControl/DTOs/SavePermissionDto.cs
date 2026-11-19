
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class SavePermissionDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El nombre del permiso es obligatorio.")]
        [StringLength(100)]
        [RegularExpression(
            @"^[a-z][a-z0-9._]*$",
            ErrorMessage = "Utilice el formato recurso.accion (ejemplo: categoria.crear).")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descripcion { get; set; }
    }
}
