
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // CONSULTA DE USUARIOS CON PERMISOS
        // ========================================

        private IQueryable<Usuario> UsuariosConPermisos =>
            _context.Usuarios
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .Include(u => u.UsuarioPermisos)
                    .ThenInclude(up => up.Permiso);

        // ========================================
        // OBTENER USUARIO POR NOMBRE
        // ========================================

        public async Task<Usuario?> GetUsuarioByUserNameAsync(
            string userName)
        {
            return await UsuariosConPermisos
                .FirstOrDefaultAsync(
                    u => u.UserName == userName);
        }

        // ========================================
        // OBTENER USUARIO POR CORREO
        // ========================================

        public async Task<Usuario?> GetUsuarioByEmailAsync(
            string email)
        {
            return await UsuariosConPermisos
                .FirstOrDefaultAsync(
                    u => u.Email == email);
        }

        // ========================================
        // OBTENER USUARIO POR ID
        // ========================================

        public async Task<Usuario?> GetUsuarioByIdWithPermissionAsync(
            int id)
        {
            return await UsuariosConPermisos
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        // ========================================
        // OBTENER USUARIO POR REFRESH TOKEN
        // ========================================

        public async Task<Usuario?> GetUsuarioByRefreshTokenAsync(
            string refreshToken)
        {
            return await UsuariosConPermisos
                .FirstOrDefaultAsync(
                    u => u.RefreshToken == refreshToken);
        }

        // ========================================
        // REGISTRAR USUARIO
        // ========================================

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            var resultado =
                await _context.Usuarios.AddAsync(usuario);

            return resultado.Entity;
        }

        // ========================================
        // VALIDAR CONTRASEÑA
        // ========================================

        public bool ValidatePassword(
            Usuario usuario,
            string password)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(
                password,
                usuario.Password);
        }

        // ========================================
        // GUARDAR CAMBIOS
        // ========================================

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        // ========================================
        // OBTENER TODOS LOS USUARIOS
        // ========================================

        public async Task<IReadOnlyList<Usuario>>
            GetAllUsuariosAsync()
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Role)
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }

        // ========================================
        // ASIGNAR PERMISO INDIVIDUAL
        // ========================================

        public async Task<bool> AssignPermissionToUserAsync(
            int usuarioId,
            int permissionId,
            bool isGranted)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.Id == usuarioId);

            var permisoExiste = await _context.Permissions
                .AnyAsync(p => p.Id == permissionId);

            if (!usuarioExiste || !permisoExiste)
                return false;

            var relacion = await _context.UsuarioPermisos
                .FirstOrDefaultAsync(up =>
                    up.UsuarioId == usuarioId &&
                    up.PermissionId == permissionId);

            if (relacion != null)
            {
                relacion.IsGranted = isGranted;
            }
            else
            {
                _context.UsuarioPermisos.Add(
                    new UsuarioPermiso
                    {
                        UsuarioId = usuarioId,
                        PermissionId = permissionId,
                        IsGranted = isGranted
                    });
            }

            await _context.SaveChangesAsync();

            return true;
        }

        // ========================================
        // RETIRAR PERMISO INDIVIDUAL
        // ========================================

        public async Task<bool> RemovePermissionFromUserAsync(
            int usuarioId,
            int permissionId)
        {
            var relacion = await _context.UsuarioPermisos
                .FirstOrDefaultAsync(up =>
                    up.UsuarioId == usuarioId &&
                    up.PermissionId == permissionId);

            if (relacion == null)
                return false;

            _context.UsuarioPermisos.Remove(relacion);

            await _context.SaveChangesAsync();

            return true;
        }

        // ========================================
        // ACTUALIZAR ROL DEL USUARIO
        // ========================================

        public async Task<bool> UpdateUsuarioRolAsync(
            int usuarioId,
            int roleId)
        {
            var usuario = await _context.Usuarios
                .FindAsync(usuarioId);

            if (usuario == null)
                return false;

            var rolExiste = await _context.Roles
                .AnyAsync(r => r.Id == roleId);

            if (!rolExiste)
                return false;

            usuario.RoleId = roleId;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
