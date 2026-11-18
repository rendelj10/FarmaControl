
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        // Consulta de usuarios incluyendo roles y permisos
        private IQueryable<Usuario> UsuariosConPermisos =>
            _context.Usuarios
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .Include(u => u.UsuarioPermisos)
                    .ThenInclude(up => up.Permiso);

        public async Task<Usuario?> GetUsuarioByUserNameAsync(
            string userName)
        {
            return await UsuariosConPermisos
                .FirstOrDefaultAsync(
                    u => u.UserName == userName);
        }

        public async Task<Usuario?> GetUsuarioByEmailAsync(
            string email)
        {
            return await UsuariosConPermisos
                .FirstOrDefaultAsync(
                    u => u.Email == email);
        }

        public async Task<Usuario?> GetUsuarioByIdWithPermissionAsync(
            int id)
        {
            return await UsuariosConPermisos
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> GetUsuarioByRefreshTokenAsync(
            string refreshToken)
        {
            return await UsuariosConPermisos
                .FirstOrDefaultAsync(
                    u => u.RefreshToken == refreshToken);
        }

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            var resultado = await _context.Usuarios.AddAsync(usuario);

            return resultado.Entity;
        }

        public bool ValidatePassword(
            Usuario usuario,
            string password)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(
                password,
                usuario.Password);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
