
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
                    usuario.Role?.Name ?? "Usuario")
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
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));
        }

        public string GeneratePasswordResetToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32));
        }
    }
}
