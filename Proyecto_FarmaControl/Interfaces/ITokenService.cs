
namespace Proyecto_FarmaControl.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(
            Proyecto_FarmaControl.Models.Usuario usuario);

        string GenerateRefreshToken();

        string GeneratePasswordResetToken();
    }
}
