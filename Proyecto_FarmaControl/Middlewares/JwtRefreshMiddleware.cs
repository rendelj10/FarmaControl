
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models.settings;

namespace Proyecto_FarmaControl.Middlewares
{
    public class JwtRefreshMiddleware
    {
        public const string RefreshedTokenKey =
            "FarmaControl.RefreshedAccessToken";

        private readonly RequestDelegate _next;
        private readonly JwtSettings _jwtSettings;

        public JwtRefreshMiddleware(
            RequestDelegate next,
            IOptions<JwtSettings> jwtSettings)
        {
            _next = next;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IAuthService authService)
        {
            // Omitir las rutas de autenticación
            if (context.Request.Path.StartsWithSegments("/Auth"))
            {
                await _next(context);
                return;
            }

            var accessToken =
                context.Request.Cookies["access_token"];

            var refreshToken =
                context.Request.Cookies["refresh_token"];

            if (string.IsNullOrWhiteSpace(accessToken) &&
                string.IsNullOrWhiteSpace(refreshToken))
            {
                await _next(context);
                return;
            }

            var handler = new JwtSecurityTokenHandler();

            var validationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_jwtSettings.Key)),

                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

            // Validar el Access Token
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                try
                {
                    var principal = handler.ValidateToken(
                        accessToken,
                        validationParameters,
                        out SecurityToken validatedToken);

                    ValidarAlgoritmo(validatedToken);

                    context.User = principal;

                    await _next(context);
                    return;
                }
                catch (SecurityTokenExpiredException)
                {
                    // Continuar con la renovación
                }
                catch
                {
                    await HandleInvalidToken(context);
                    return;
                }
            }

            // Comprobar Refresh Token
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                await HandleInvalidToken(context);
                return;
            }

            var newTokens =
                await authService.RefreshAccessTokenAsync(refreshToken);

            if (newTokens == null)
            {
                await HandleInvalidToken(context);
                return;
            }

            // Guardar las nuevas cookies
            var accessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = newTokens.ExpiresAt
            };

            var refreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = newTokens.RefreshTokenExpiresAt
                    ?? DateTime.UtcNow.AddDays(7)
            };

            context.Response.Cookies.Append(
                "access_token",
                newTokens.AccessToken,
                accessOptions);

            context.Response.Cookies.Append(
                "refresh_token",
                newTokens.RefreshToken,
                refreshOptions);

            try
            {
                // Validar el nuevo Access Token
                var principal = handler.ValidateToken(
                    newTokens.AccessToken,
                    validationParameters,
                    out SecurityToken validatedToken);

                ValidarAlgoritmo(validatedToken);

                // Asignar usuario a la petición actual
                context.User = principal;

                // Compartir el token renovado con JwtBearer
                context.Items[RefreshedTokenKey] =
                    newTokens.AccessToken;
            }
            catch
            {
                await HandleInvalidToken(context);
                return;
            }

            await _next(context);
        }

        private static void ValidarAlgoritmo(
            SecurityToken validatedToken)
        {
            if (validatedToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityTokenException(
                    "Algoritmo JWT inválido.");
            }
        }

        private static Task HandleInvalidToken(
            HttpContext context)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };

            context.Response.Cookies.Delete(
                "access_token", options);

            context.Response.Cookies.Delete(
                "refresh_token", options);

            context.Response.Redirect("/Auth/Login");

            return Task.CompletedTask;
        }
    }
}
