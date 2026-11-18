using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Rol
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Usuario> Usuarios { get; set; }
            = new List<Usuario>();

        public ICollection<RolePermission> RolePermissions
            { get; set; } = new List<RolePermission>();
    }
}