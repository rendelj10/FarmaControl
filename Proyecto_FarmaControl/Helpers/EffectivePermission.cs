
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Helpers
{
    public static class EffectivePermission
    {
        public static IReadOnlyList<string> FromUser(Usuario usuario)
        {
            var permisos = new HashSet<string>(
                usuario.Role?.RolePermissions
                    .Where(rp => rp.Permission != null)
                    .Select(rp => rp.Permission.Name)
                    ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            foreach (var permiso in usuario.UsuarioPermisos)
            {
                if (permiso.Permiso == null)
                    continue;

                if (permiso.IsGranted)
                    permisos.Add(permiso.Permiso.Name);
                else
                    permisos.Remove(permiso.Permiso.Name);
            }

            return permisos.OrderBy(p => p).ToList();
        }
    }
}
