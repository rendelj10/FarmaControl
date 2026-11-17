
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositories
{
    public class RolRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RolRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Rol>>
            GetAllWithPermissionsAsync()
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Rol?> GetByIdWithPermissionAsync(int id)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Rol?> GetByNameAsync(string name)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == name.Trim());
        }

        public async Task<Rol> CreateAsync(string name)
        {
            var nombre = name.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre del rol es obligatorio.");

            var existe = await _context.Roles
                .AnyAsync(r => r.Name == nombre);

            if (existe)
                throw new InvalidOperationException(
                    "El rol ya existe.");

            var rol = new Rol
            {
                Name = nombre
            };

            await _context.Roles.AddAsync(rol);
            await _context.SaveChangesAsync();

            return rol;
        }

        public async Task<bool> AssignPermissionToRoleAsync(
            int roleId,
            int permissionId)
        {
            var existe = await _context.RolePermissions
                .AnyAsync(rp =>
                    rp.RoleId == roleId &&
                    rp.PermissionId == permissionId);

            if (existe)
                return false;

            var rolExiste = await _context.Roles
                .AnyAsync(r => r.Id == roleId);

            var permisoExiste = await _context.Permissions
                .AnyAsync(p => p.Id == permissionId);

            if (!rolExiste || !permisoExiste)
                return false;

            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(
            int roleId,
            int permissionId)
        {
            var relacion = await _context.RolePermissions
                .FirstOrDefaultAsync(rp =>
                    rp.RoleId == roleId &&
                    rp.PermissionId == permissionId);

            if (relacion == null)
                return false;

            _context.RolePermissions.Remove(relacion);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
