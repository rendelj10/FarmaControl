
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
