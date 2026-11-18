
    using AutoMapper;
    using Microsoft.Extensions.Options;
    using System.Net;
    using System.Security.Cryptography;
    using Proyecto_FarmaControl.DTOs;
    using Proyecto_FarmaControl.Interfaces;
    using Proyecto_FarmaControl.Models;
    using Proyecto_FarmaControl.Models.settings;

    namespace Proyecto_FarmaControl.Services
    {
        public class AuthService : IAuthService
        {
            private readonly IUsuarioRepository _userRepo;
            private readonly IRoleRepository _roleRepo;
            private readonly ITokenService _tokenService;
            private readonly IEmailService _emailService;
            private readonly IMapper _mapper;
            private readonly IConfiguration _configuration;
            private readonly JwtSettings _jwtSettings;

            public AuthService(
                IUsuarioRepository userRepo,
                IRoleRepository roleRepo,
                ITokenService tokenService,
                IEmailService emailService,
                IMapper mapper,
                IConfiguration configuration,
                IOptions<JwtSettings> jwtSettings)
            {
                _userRepo = userRepo;
                _roleRepo = roleRepo;
                _tokenService = tokenService;
                _emailService = emailService;
                _mapper = mapper;
                _configuration = configuration;
                _jwtSettings = jwtSettings.Value;
            }

            // REGISTRO
            public async Task<(bool Success, string? ErrorMessage)>
                RegisterAsync(RegisterRequestDto dto)
            {
                var email = dto.Email.Trim().ToLowerInvariant();
                var username = dto.Username.Trim();

                if (await _userRepo.GetUsuarioByEmailAsync(email) != null)
                    return (false, "El correo ya está registrado.");

                if (await _userRepo.GetUsuarioByUserNameAsync(username) != null)
                    return (false, "El nombre de usuario ya existe.");

                var role = await _roleRepo.GetByNameAsync("Usuario");

                if (role == null)
                    return (false, "El rol Usuario no está configurado.");

                var user = _mapper.Map<Usuario>(dto);

                user.Email = email;
                user.UserName = username;
                user.Password =
                    BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);

                user.RoleId = role.Id;
                user.EmailConfirmed = false;

                var rawToken = Convert.ToBase64String(
                    RandomNumberGenerator.GetBytes(64));

                user.EmailConfirmToken = rawToken;

                await _userRepo.CreateAsync(user);
                await _userRepo.SaveAsync();

                var link = CreateAuthLink(
                    "ConfirmEmail",
                    user.Email,
                    rawToken);

                var body = CreateEmailBody(
                    "Confirma tu cuenta de FarmaControl.",
                    "Confirmar cuenta",
                    link);

                _emailService.SendPasswordResetEmail(
                    user.Email,
                    body);

                return (true, null);
            }

            // INICIAR SESIÓN
            public async Task<(
                bool Success,
                LoginResponseDto? result,
                string? ErrorMessage)> LoginAsync(LoginRequestDto dto)
            {
                var email = dto.Email.Trim().ToLowerInvariant();

                var user = await _userRepo.GetUsuarioByEmailAsync(email);

                if (user == null ||
                    !_userRepo.ValidatePassword(user, dto.Password))
                {
                    return (false, null, "Credenciales incorrectas.");
                }

                if (!user.EmailConfirmed)
                {
                    return (
                        false,
                        null,
                        "Debe confirmar su correo electrónico.");
                }

                var accessToken =
                    _tokenService.GenerateAccessToken(user);

                var refreshToken =
                    _tokenService.GenerateRefreshToken();

                var expiration = DateTime.UtcNow.AddMinutes(
                    _jwtSettings.TokenValidateInMinutos);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime =
                    DateTime.UtcNow.AddDays(7);

                await _userRepo.SaveAsync();

                return (
                    true,
                    new LoginResponseDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = expiration,
                        RefreshTokenExpiresAt =
                            user.RefreshTokenExpiryTime
                    },
                    null);
            }

            // CONFIRMAR CORREO
            public async Task<bool> ConfirmEmailAsync(
                string email,
                string token)
            {
                var user = await _userRepo.GetUsuarioByEmailAsync(
                    email.Trim().ToLowerInvariant());

                if (user == null ||
                    string.IsNullOrWhiteSpace(user.EmailConfirmToken))
                    return false;

                if (!string.Equals(
                    user.EmailConfirmToken,
                    token,
                    StringComparison.Ordinal))
                    return false;

                user.EmailConfirmed = true;
                user.EmailConfirmToken = null;

                await _userRepo.SaveAsync();

                return true;
            }

            // ENVIAR ENLACE DE RECUPERACIÓN
            public async Task<bool> SendResetPasswordLinkAsync(
                string email)
            {
                var user = await _userRepo.GetUsuarioByEmailAsync(
                    email.Trim().ToLowerInvariant());

                if (user == null)
                    return false;

                var token =
                    _tokenService.GeneratePasswordResetToken();

                user.PasswordResedToken = token;

                user.PasswordResetTokenExpirityTime =
                    DateTime.UtcNow.AddHours(1);

                await _userRepo.SaveAsync();

                var link = CreateAuthLink(
                    "ResetPassword",
                    user.Email,
                    token);

                var body = CreateEmailBody(
                    "Solicitaste restablecer tu contraseña.",
                    "Restablecer contraseña",
                    link);

                _emailService.SendPasswordResetEmail(
                    user.Email,
                    body);

                return true;
            }

            // RESTABLECER CONTRASEÑA

            public async Task<bool> ResetPasswordAsync(
                ResetPasswordRequestDto dto)
            {
                var user = await _userRepo.GetUsuarioByEmailAsync(
                    dto.Email.Trim().ToLowerInvariant());

                if (user == null)
                    return false;

                if (!string.Equals(
                    user.PasswordResedToken,
                    dto.Token,
                    StringComparison.Ordinal))
                    return false;

                if (!user.PasswordResetTokenExpirityTime.HasValue ||
                    user.PasswordResetTokenExpirityTime.Value <=
                        DateTime.UtcNow)
                    return false;

                user.Password =
                    BCrypt.Net.BCrypt.EnhancedHashPassword(
                        dto.NewPassword);

                user.PasswordResedToken = null;
                user.PasswordResetTokenExpirityTime = null;

                // Revocar las sesiones anteriores
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                await _userRepo.SaveAsync();

                return true;
            }


            // REENVIAR CONFIRMACIÓN
            public async Task<bool> ResendConfirmationEmailAsync(
                string email)
            {
                var user = await _userRepo.GetUsuarioByEmailAsync(
                    email.Trim().ToLowerInvariant());

                if (user == null || user.EmailConfirmed)
                    return false;

                var token = Convert.ToBase64String(
                    RandomNumberGenerator.GetBytes(64));

                user.EmailConfirmToken = token;

                await _userRepo.SaveAsync();

                var link = CreateAuthLink(
                    "ConfirmEmail",
                    user.Email,
                    token);

                var body = CreateEmailBody(
                    "Confirma tu cuenta de FarmaControl.",
                    "Confirmar cuenta",
                    link);

                _emailService.SendPasswordResetEmail(
                    user.Email,
                    body);

                return true;
            }

            // CAMBIAR CONTRASEÑA
            public async Task<bool> ChangePasswordAsync(
                string userName,
                ChangePasswordDto dto)
            {
                var user = await _userRepo
                    .GetUsuarioByUserNameAsync(userName);

                if (user == null ||
                    !_userRepo.ValidatePassword(
                        user,
                        dto.CurrentPassword))
                    return false;

                user.Password =
                    BCrypt.Net.BCrypt.EnhancedHashPassword(
                        dto.NewPassword);

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                await _userRepo.SaveAsync();

                return true;
            }

            // RENOVAR TOKENS
            public async Task<LoginResponseDto?>
                RefreshAccessTokenAsync(string refreshToken)
            {
                var user = await _userRepo
                    .GetUsuarioByRefreshTokenAsync(refreshToken);

                if (user == null ||
                    !user.RefreshTokenExpiryTime.HasValue ||
                    user.RefreshTokenExpiryTime.Value <= DateTime.UtcNow)
                    return null;

                var accessToken =
                    _tokenService.GenerateAccessToken(user);

                var newRefreshToken =
                    _tokenService.GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;

                user.RefreshTokenExpiryTime =
                    DateTime.UtcNow.AddDays(7);

                await _userRepo.SaveAsync();

                return new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,

                    ExpiresAt = DateTime.UtcNow.AddMinutes(
                        _jwtSettings.TokenValidateInMinutos),

                    RefreshTokenExpiresAt =
                        user.RefreshTokenExpiryTime
                };
            }

            // CONSTRUIR ENLACES
            private string CreateAuthLink(
                string action,
                string email,
                string token)
            {
                var baseUrl = _configuration["App:BaseUrl"];

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    throw new InvalidOperationException(
                        "No está configurada App:BaseUrl.");
                }

                return $"{baseUrl.TrimEnd('/')}/Auth/{action}" +
                    $"?email={Uri.EscapeDataString(email)}" +
                    $"&token={Uri.EscapeDataString(token)}";
            }

            // CONTENIDO DEL CORREO
            private static string CreateEmailBody(
                string message,
                string buttonText,
                string link)
            {
                var safeMessage = WebUtility.HtmlEncode(message);
                var safeLink = WebUtility.HtmlEncode(link);
                var safeButton = WebUtility.HtmlEncode(buttonText);

                return $"""
                    <!DOCTYPE html>
                    <html>
                    <body>
                        <h2>FarmaControl</h2>
                        <p>{safeMessage}</p>
                        <a href="{safeLink}"
                           style="background:#007BBF;color:white;
                           padding:10px 20px;border-radius:5px;
                           text-decoration:none;">
                           {safeButton}
                        </a>
                    </body>
                    </html>
                    """;
            }
        }
    }
