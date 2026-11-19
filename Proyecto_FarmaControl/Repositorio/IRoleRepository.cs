using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IRoleRepository
    {
        Task<IReadOnlyList<Rol>> GetAllWithPermissionsAsync();

        Task<Rol?> GetByIdWithPermissionAsync(int id);

        Task<Rol?> GetByNameAsync(string name);

        Task<Rol> CreateAsync(string name);

        Task<bool> AssignPermissionToRoleAsync(
            int roleId,
            int permissionId);

        Task<bool> RemovePermissionFromRoleAsync(
            int roleId,
            int permissionId);
    }
}
