
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IPermissionRepository
    {
        Task<DataTablesResponseDto<PermissionDto>>
            GetPagedAsync(DataTablesRequestDto request);

        Task<IReadOnlyList<Permiso>> GetAllAsync();

        Task<Permiso?> GetByIdAsync(int id);

        Task<Permiso?> GetByNameAsync(string name);

        Task<Permiso> CreateAsync(
            string name,
            string? description);

        Task<bool> UpdateAsync(Permiso permission);

        Task<bool> DeleteAsync(int id);

        Task<bool> IsAssignedAsync(int permissionId);
    }
}
