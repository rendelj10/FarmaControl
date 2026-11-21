
using System.Text.RegularExpressions;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Service
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPermissionRepository _permissionRepository;

        public RolePermissionService(
            IRoleRepository roleRepository,
            IUsuarioRepository usuarioRepository,
            IPermissionRepository permissionRepository)
        {
            _roleRepository = roleRepository;
            _usuarioRepository = usuarioRepository;
            _permissionRepository = permissionRepository;
        }

        // =====================================
        // PERMISOS
        // =====================================

        public async Task<IReadOnlyList<PermissionDto>>
            GetAllPermissionsAsync()
        {
            var permissions =
                await _permissionRepository.GetAllAsync();

            return permissions.Select(MapPermission).ToList();
        }

        public Task<DataTablesResponseDto<PermissionDto>>
            GetPermissionsPagedAsync(DataTablesRequestDto request)
        {
            return _permissionRepository.GetPagedAsync(request);
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(int id)
        {
            var permission =
                await _permissionRepository.GetByIdAsync(id);

            return permission == null
                ? null
                : MapPermission(permission);
        }

        public async Task<(
            bool Success,
            PermissionDto? Result,
            string? ErrorMessage)>
            CreatePermissionAsync(SavePermissionDto dto)
        {
            var name = NormalizePermissionName(dto.Name);

            if (!IsValidPermissionName(name))
            {
                return (false, null,
                    "El nombre del permiso no tiene un formato válido.");
            }

            if (await _permissionRepository.GetByNameAsync(name) != null)
            {
                return (false, null,
                    "Ya existe un permiso con ese nombre.");
            }

            try
            {
                var permission =
                    await _permissionRepository.CreateAsync(
                        name,
                        NormalizeDescription(dto.Descripcion));

                return (true, MapPermission(permission), null);
            }
            catch (ArgumentException ex)
            {
                return (false, null, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)>
            UpdatePermissionAsync(SavePermissionDto dto)
        {
            if (!dto.Id.HasValue || dto.Id.Value <= 0)
            {
                return (false,
                    "El identificador del permiso no es válido.");
            }

            var permission =
                await _permissionRepository.GetByIdAsync(dto.Id.Value);

            if (permission == null)
                return (false, "El permiso no existe.");

            var name = NormalizePermissionName(dto.Name);

            if (!IsValidPermissionName(name))
            {
                return (false,
                    "El nombre del permiso no tiene un formato válido.");
            }

            var existing =
                await _permissionRepository.GetByNameAsync(name);

            if (existing != null && existing.Id != permission.Id)
            {
                return (false,
                    "Ya existe otro permiso con ese nombre.");
            }

            permission.Name = name;
            permission.Description =
                NormalizeDescription(dto.Descripcion);

            await _permissionRepository.UpdateAsync(permission);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)>
            DeletePermissionAsync(int id)
        {
            var permission =
                await _permissionRepository.GetByIdAsync(id);

            if (permission == null)
                return (false, "El permiso no existe.");

            if (await _permissionRepository.IsAssignedAsync(id))
            {
                return (false,
                    "No se puede eliminar el permiso porque está asignado a un rol o usuario.");
            }

            var deleted =
                await _permissionRepository.DeleteAsync(id);

            return deleted
                ? (true, null)
                : (false, "No se pudo eliminar el permiso.");
        }

        // =====================================
        // ROLES
        // =====================================

        public async Task<IReadOnlyList<RoleWithPermissionDto>>
            GetAllRolesAsync()
        {
            var roles =
                await _roleRepository.GetAllWithPermissionsAsync();

            return roles.Select(MapRole).ToList();
        }

        public async Task<RoleWithPermissionDto?>
            GetRoleByIdAsync(int roleId)
        {
            var role =
                await _roleRepository.GetByIdWithPermissionAsync(roleId);

            return role == null
                ? null
                : MapRole(role);
        }

        public async Task<(
            bool Success,
            RoleWithPermissionDto? Result,
            string? ErrorMessage)>
            CreateRoleAsync(CreateRoleDto dto)
        {
            var name = dto.Name?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, null,
                    "El nombre del rol es obligatorio.");
            }

            if (await _roleRepository.GetByNameAsync(name) != null)
            {
                return (false, null,
                    "Ya existe un rol con ese nombre.");
            }

            try
            {
                var role =
                    await _roleRepository.CreateAsync(name);

                var createdRole =
                    await _roleRepository.GetByIdWithPermissionAsync(
                        role.Id);

                return createdRole == null
                    ? (false, null,
                        "El rol fue creado, pero no se pudo recuperar.")
                    : (true, MapRole(createdRole), null);
            }
            catch (ArgumentException ex)
            {
                return (false, null, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)>
            AssignPermissionToRoleAsync(
                int roleId,
                int permissionId)
        {
            var role =
                await _roleRepository.GetByIdWithPermissionAsync(roleId);

            if (role == null)
                return (false, "El rol no existe.");

            var permission =
                await _permissionRepository.GetByIdAsync(permissionId);

            if (permission == null)
                return (false, "El permiso no existe.");

            var assigned =
                await _roleRepository.AssignPermissionToRoleAsync(
                    roleId,
                    permissionId);

            return assigned
                ? (true, null)
                : (false, "El permiso ya está asignado a este rol.");
        }

        public async Task<(bool Success, string? ErrorMessage)>
            RemovePermissionFromRoleAsync(
                int roleId,
                int permissionId)
        {
            var role =
                await _roleRepository.GetByIdWithPermissionAsync(roleId);

            if (role == null)
                return (false, "El rol no existe.");

            var permission =
                await _permissionRepository.GetByIdAsync(permissionId);

            if (permission == null)
                return (false, "El permiso no existe.");

            var removed =
                await _roleRepository.RemovePermissionFromRoleAsync(
                    roleId,
                    permissionId);

            return removed
                ? (true, null)
                : (false, "El permiso no estaba asignado a este rol.");
        }

        // =====================================
        // USUARIOS
        // =====================================

        public async Task<IReadOnlyList<UserListItemDto>>
            GetAllUsersAsync()
        {
            var users =
                await _usuarioRepository.GetAllUsuariosAsync();

            return users.Select(u => new UserListItemDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                RoleName = u.Role.Name
            }).ToList();
        }

        public async Task<UserPermissionDto?>
            GetUserPermissionsAsync(int userId)
        {
            var user =
                await _usuarioRepository
                    .GetUsuarioByIdWithPermissionAsync(userId);

            if (user == null)
                return null;

            // Permisos heredados del rol

            var effectivePermissions = new HashSet<string>(
                user.Role.RolePermissions
                    .Select(rp => rp.Permission.Name),
                StringComparer.OrdinalIgnoreCase);

            // Aplicar excepciones individuales

            foreach (var overridePermission in user.UsuarioPermisos)
            {
                var permissionName =
                    overridePermission.Permiso.Name;

                if (overridePermission.IsGranted)
                {
                    effectivePermissions.Add(permissionName);
                }
                else
                {
                    effectivePermissions.Remove(permissionName);
                }
            }

            return new UserPermissionDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role.Name,

                EffectivePermission = effectivePermissions
                    .OrderBy(p => p)
                    .ToList(),

                DirectPermission = user.UsuarioPermisos
                    .Select(up => new UserPermissionOverrideDto
                    {
                        PermissionId = up.PermissionId,
                        PermissionName = up.Permiso.Name,
                        IsGranted = up.IsGranted
                    })
                    .OrderBy(p => p.PermissionName)
                    .ToList()
            };
        }

        public async Task<(bool Success, string? ErrorMessage)>
            AssignPermissionToUserAsync(
                int userId,
                AssignUserPermissionDto dto)
        {
            var user =
                await _usuarioRepository
                    .GetUsuarioByIdWithPermissionAsync(userId);

            if (user == null)
                return (false, "El usuario no existe.");

            var permission =
                await _permissionRepository.GetByIdAsync(
                    dto.PermissionId);

            if (permission == null)
                return (false, "El permiso no existe.");

            var assigned =
                await _usuarioRepository.AssignPermissionToUserAsync(
                    userId,
                    dto.PermissionId,
                    dto.IsGranted);

            if (!assigned)
                return (false,
                    "No se pudo asignar el permiso al usuario.");

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)>
            RemoveUserPermissionAsync(
                int userId,
                int permissionId)
        {
            var user =
                await _usuarioRepository
                    .GetUsuarioByIdWithPermissionAsync(userId);

            if (user == null)
                return (false, "El usuario no existe.");

            var permission =
                await _permissionRepository.GetByIdAsync(permissionId);

            if (permission == null)
                return (false, "El permiso no existe.");

            var removed =
                await _usuarioRepository.RemovePermissionFromUserAsync(
                    userId,
                    permissionId);

            return removed
                ? (true, null)
                : (false,
                    "El usuario no tiene una excepción individual para este permiso.");
        }

        public async Task<(bool Success, string? ErrorMessage)>
            UpdateUserRoleAsync(int userId, int roleId)
        {
            var user =
                await _usuarioRepository
                    .GetUsuarioByIdWithPermissionAsync(userId);

            if (user == null)
                return (false, "El usuario no existe.");

            var role =
                await _roleRepository.GetByIdWithPermissionAsync(roleId);

            if (role == null)
                return (false, "El rol seleccionado no existe.");

            var updated =
                await _usuarioRepository.UpdateUsuarioRolAsync(
                    userId,
                    roleId);

            return updated
                ? (true, null)
                : (false, "No se pudo actualizar el rol del usuario.");
        }

        // =====================================
        // METODOS AUXILIARES
        // =====================================

        private static PermissionDto MapPermission(Permiso permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description
            };
        }

        private static RoleWithPermissionDto MapRole(Rol role)
        {
            return new RoleWithPermissionDto
            {
                Id = role.Id,
                Name = role.Name,

                Permissions = role.RolePermissions
                    .Select(rp => MapPermission(rp.Permission))
                    .OrderBy(p => p.Name)
                    .ToList()
            };
        }

        private static string NormalizePermissionName(string? name)
        {
            return name?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        private static bool IsValidPermissionName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) &&
                   name.Length <= 100 &&
                   Regex.IsMatch(name, @"^[a-z][a-z0-9._]*$");
        }

        private static string? NormalizeDescription(string? description)
        {
            return string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim();
        }
    }
}
