
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class UpdateUserRoleDto
    {
        [Range(1, int.MaxValue)]
        public int RolId { get; set; }
    }
}
