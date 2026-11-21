
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Proyecto_FarmaControl.Service;
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
            // =====================================
            // 1. OMITIR RUTAS DE AUTENTICACION
            // =====================================

            if (context.Request.Path.StartsWithSegments("/Auth"))
            {
                await _next(context);
                return;
            }

            // =====================================
            // 2. LEER COOKIES
            // =====================================

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

            // =====================================
            // 3. CONFIGURAR VALIDACION JWT
            // =====================================

            var handler = new JwtSecurityTokenHandler();

            var validationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                _jwtSettings.Key)),

                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };

            // =====================================
            // 4. VALIDAR ACCESS TOKEN
            // =====================================

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                ClaimsPrincipal? principal = null;

                try
                {
                    principal = handler.ValidateToken(
                        accessToken,
                        validationParameters,
                        out SecurityToken validatedToken);

                    ValidarAlgoritmo(validatedToken);
                }
                catch (SecurityTokenExpiredException)
                {
                    // El token expiró.
                    // Continuar con la renovación.
                }
                catch (SecurityTokenException)
                {
                    await HandleInvalidToken(context);
                    return;
                }
                catch
                {
                    await HandleInvalidToken(context);
                    return;
                }

                if (principal != null)
                {
                    context.User = principal;

                    // Ejecutar la aplicación fuera
                    // del bloque de validación.
                    await _next(context);
                    return;
                }
            }

            // =====================================
            // 5. COMPROBAR REFRESH TOKEN
            // =====================================

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                await HandleInvalidToken(context);
                return;
            }

            // =====================================
            // 6. RENOVAR TOKENS
            // =====================================

            var newTokens =
                await authService.RefreshAccessTokenAsync(
                    refreshToken);

            if (newTokens == null)
            {
                await HandleInvalidToken(context);
                return;
            }

            // =====================================
            // 7. VALIDAR NUEVO ACCESS TOKEN
            // =====================================

            ClaimsPrincipal newPrincipal;

            try
            {
                newPrincipal = handler.ValidateToken(
                    newTokens.AccessToken,
                    validationParameters,
                    out SecurityToken validatedToken);

                ValidarAlgoritmo(validatedToken);
            }
            catch
            {
                await HandleInvalidToken(context);
                return;
            }

            // =====================================
            // 8. GUARDAR NUEVAS COOKIES
            // =====================================

            var accessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = newTokens.ExpiresAt,
                IsEssential = true
            };

            var refreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = newTokens.RefreshTokenExpiresAt
                    ?? DateTime.UtcNow.AddDays(7),
                IsEssential = true
            };

            context.Response.Cookies.Append(
                "access_token",
                newTokens.AccessToken,
                accessOptions);

            context.Response.Cookies.Append(
                "refresh_token",
                newTokens.RefreshToken,
                refreshOptions);

            // =====================================
            // 9. ACTUALIZAR USUARIO AUTENTICADO
            // =====================================

            context.User = newPrincipal;

            // Compartir el token con JwtBearer.
            context.Items[RefreshedTokenKey] =
                newTokens.AccessToken;

            // =====================================
            // 10. CONTINUAR PETICION
            // =====================================

            await _next(context);
        }

        // =====================================
        // VALIDAR ALGORITMO DE SEGURIDAD
        // =====================================

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

        // =====================================
        // CERRAR SESION SI EL TOKEN ES INVALIDO
        // =====================================

        private static Task HandleInvalidToken(
            HttpContext context)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
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
