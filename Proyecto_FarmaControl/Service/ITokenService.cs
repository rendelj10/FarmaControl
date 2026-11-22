
namespace Proyecto_FarmaControl.Service
{
    public interface ITokenService
    {
        string GenerateAccessToken(
            Proyecto_FarmaControl.Models.Usuario usuario);

        string GenerateRefreshToken();

        string GeneratePasswordResetToken();
    }
}
