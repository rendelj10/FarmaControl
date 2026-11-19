
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IUsuarioRepository
    {
        // CONSULTAS DE AUTENTICACION

        Task<Usuario?> GetUsuarioByUserNameAsync(string userName);

        Task<Usuario?> GetUsuarioByEmailAsync(string email);

        Task<Usuario?> GetUsuarioByIdWithPermissionAsync(int id);

        Task<Usuario?> GetUsuarioByRefreshTokenAsync(string refreshToken);

        // REGISTRO Y AUTENTICACION

        Task<Usuario> CreateAsync(Usuario usuario);

        bool ValidatePassword(Usuario usuario, string password);

        Task SaveAsync();

        // ADMINISTRACION DE USUARIOS

        Task<IReadOnlyList<Usuario>> GetAllUsuariosAsync();

        Task<bool> AssignPermissionToUserAsync(
            int usuarioId,
            int permissionId,
            bool isGranted);

        Task<bool> RemovePermissionFromUserAsync(
            int usuarioId,
            int permissionId);

        Task<bool> UpdateUsuarioRolAsync(
            int usuarioId,
            int roleId);
    }
}
