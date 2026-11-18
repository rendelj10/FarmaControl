namespace Proyecto_FarmaControl.Models
{
    public class UsuarioPermiso
    {
        public int UsuarioId { get; set; }

        public virtual Usuario Usuario { get; set; } = null!;

        public int PermissionId { get; set; }

        public virtual Permiso Permiso { get; set; } = null!;

        public bool IsGranted { get; set; }
    }
}