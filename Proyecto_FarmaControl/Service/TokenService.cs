
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Proyecto_FarmaControl.Helpers;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Models.settings;

namespace Proyecto_FarmaControl.Service
{
    public class TokenService : ITokenService
    {
        public const string SessionClaimType = "fc_session";
        private readonly JwtSettings _jwt;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwt = jwtSettings.Value;

            if (string.IsNullOrWhiteSpace(_jwt.Key) ||
                Encoding.UTF8.GetByteCount(_jwt.Key) < 32)
            {
                throw new InvalidOperationException(
                    "La clave JWT debe tener al menos 32 bytes.");
            }
        }

        public string GenerateAccessToken(Usuario usuario)
        {
            string? sessionId = GetSessionId(usuario.RefreshToken);
            if (sessionId == null)
                throw new InvalidOperationException(
                    "No hay una sesión activa válida para generar el JWT.");

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    usuario.Id.ToString()),

                new(
                    ClaimTypes.Name,
                    usuario.UserName),

                new(
                    ClaimTypes.Email,
                    usuario.Email),

                new(
                    ClaimTypes.Role,
                    usuario.Role?.Name ?? "Usuario"),

                new(SessionClaimType, sessionId)
            };

            // Incorporar los permisos efectivos del usuario
            foreach (var permission in EffectivePermission.FromUser(usuario))
            {
                claims.Add(new Claim("Permission", permission));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwt.Key));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(
                _jwt.TokenValidateInMinutos);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiration,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            // La primera parte identifica la sesión (128 bits),
            // la segunda es el secreto rotativo (512 bits).
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(16))
                + "." + Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        public static string RotateRefreshToken(string currentRefreshToken)
        {
            string? sessionId = GetSessionId(currentRefreshToken);
            if (sessionId == null)
                throw new InvalidOperationException(
                    "El refresh token tiene un formato inválido.");

            return sessionId + "." +
                Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        public static bool IsCurrentSession(
            Usuario usuario,
            string? jwtSessionId)
        {
            if (string.IsNullOrWhiteSpace(jwtSessionId) ||
                !usuario.EmailConfirmed ||
                !usuario.RefreshTokenExpiryTime.HasValue ||
                usuario.RefreshTokenExpiryTime.Value <= DateTime.UtcNow)
                return false;

            string? storedId = GetSessionId(usuario.RefreshToken);
            return storedId != null &&
                string.Equals(storedId, jwtSessionId,
                    StringComparison.Ordinal);
        }

        private static string? GetSessionId(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            // Formato: 32 hex de sesión + '.' + 128 hex de secreto.
            if (refreshToken.Length != 161 || refreshToken[32] != '.')
                return null;

            if (!refreshToken[..32].All(Uri.IsHexDigit) ||
                !refreshToken[33..].All(Uri.IsHexDigit))
                return null;

            return refreshToken.Substring(0, 32);
        }

        public string GeneratePasswordResetToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32));
        }
    }
}
