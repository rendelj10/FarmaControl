
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetUsuarioByUserNameAsync(string userName);

        Task<Usuario?> GetUsuarioByEmailAsync(string email);

        Task<Usuario?> GetUsuarioByIdWithPermissionAsync(int id);

        Task<Usuario> CreateAsync(Usuario usuario);

        bool ValidatePassword(Usuario usuario, string password);

        Task SaveAsync();

        Task<Usuario?> GetUsuarioByRefreshTokenAsync(string refreshToken);
    }
}
