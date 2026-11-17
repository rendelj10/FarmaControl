using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Permiso
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public ICollection<UsuarioPermiso> UsuarioPermisos
            { get; set; } = new List<UsuarioPermiso>();

        public ICollection<RolePermission> RolePermissions
            { get; set; } = new List<RolePermission>();
    }
}