
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Service
{
    public interface IRolePermissionService
    {
        // PERMISOS

        Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync();

        Task<DataTablesResponseDto<PermissionDto>>
            GetPermissionsPagedAsync(DataTablesRequestDto request);

        Task<PermissionDto?> GetPermissionByIdAsync(int id);

        Task<(bool Success, PermissionDto? Result, string? ErrorMessage)>
            CreatePermissionAsync(SavePermissionDto dto);

        Task<(bool Success, string? ErrorMessage)>
            UpdatePermissionAsync(SavePermissionDto dto);

        Task<(bool Success, string? ErrorMessage)>
            DeletePermissionAsync(int id);

        // ROLES

        Task<IReadOnlyList<RoleWithPermissionDto>> GetAllRolesAsync();

        Task<RoleWithPermissionDto?> GetRoleByIdAsync(int roleId);

        Task<(bool Success, RoleWithPermissionDto? Result, string? ErrorMessage)>
            CreateRoleAsync(CreateRoleDto dto);

        Task<(bool Success, string? ErrorMessage)>
            AssignPermissionToRoleAsync(int roleId, int permissionId);

        Task<(bool Success, string? ErrorMessage)>
            RemovePermissionFromRoleAsync(int roleId, int permissionId);

        // USUARIOS

        Task<IReadOnlyList<UserListItemDto>> GetAllUsersAsync();

        Task<UserPermissionDto?> GetUserPermissionsAsync(int userId);

        Task<(bool Success, string? ErrorMessage)>
            AssignPermissionToUserAsync(
                int userId,
                AssignUserPermissionDto dto);

        Task<(bool Success, string? ErrorMessage)>
            RemoveUserPermissionAsync(int userId, int permissionId);

        Task<(bool Success, string? ErrorMessage)>
            UpdateUserRoleAsync(int userId, int roleId);
    }
}
