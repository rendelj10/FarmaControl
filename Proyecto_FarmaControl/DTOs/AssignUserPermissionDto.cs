
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class AssignUserPermissionDto
    {
        [Range(1, int.MaxValue)]
        public int PermissionId { get; set; }

        public bool IsGranted { get; set; }
    }
}
