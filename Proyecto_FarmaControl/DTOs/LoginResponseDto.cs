
namespace Proyecto_FarmaControl.DTOs
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime? RefreshTokenExpiresAt { get; set; }
    }
}
