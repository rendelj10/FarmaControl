namespace Proyecto_FarmaControl.Models
{
    public class RolePermission
    {
        public int RoleId { get; set; }

        public virtual Rol Role { get; set; } = null!;

        public int PermissionId { get; set; }

        public virtual Permiso Permission { get; set; } = null!;
    }
}