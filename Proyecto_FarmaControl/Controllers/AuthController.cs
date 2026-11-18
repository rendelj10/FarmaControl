
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;

namespace Proyecto_FarmaControl.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        // =====================================
        // INICIAR SESION
        // =====================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginRequestDto());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var (success, result, errorMessage) =
                await _service.LoginAsync(dto);

            if (!success || result == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    errorMessage ?? "No se pudo iniciar sesión.");

                return View(dto);
            }

            Response.Cookies.Append(
                "access_token",
                result.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Path = "/",
                    Expires = result.ExpiresAt,
                    IsEssential = true
                });

            Response.Cookies.Append(
                "refresh_token",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Path = "/",
                    Expires = result.RefreshTokenExpiresAt,
                    IsEssential = true
                });

            return RedirectToAction("Index", "Home");
        }

        // =====================================
        // REGISTRAR USUARIO
        // =====================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterRequestDto());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var (success, errorMessage) =
                await _service.RegisterAsync(dto);

            if (!success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    errorMessage ?? "No se pudo registrar el usuario.");

                return View(dto);
            }

            TempData["Mensaje"] =
                "Registro realizado. Revisa tu correo para confirmar la cuenta.";

            return RedirectToAction(nameof(Login));
        }

        // =====================================
        // CONFIRMAR CORREO ELECTRONICO
        // =====================================

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(
            string email,
            string token)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(token))
            {
                TempData["Error"] = "El enlace de confirmación no es válido.";
                return RedirectToAction(nameof(Login));
            }

            var confirmado =
                await _service.ConfirmEmailAsync(email, token);

            if (confirmado)
            {
                TempData["Mensaje"] =
                    "Tu correo ha sido confirmado correctamente. Ya puedes iniciar sesión.";
            }
            else
            {
                TempData["Error"] =
                    "El enlace de confirmación no es válido o ya fue utilizado.";
            }

            return RedirectToAction(nameof(Login));
        }

        // =====================================
        // SOLICITAR RECUPERACION DE CONTRASEÑA
        // =====================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDto());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _service.SendResetPasswordLinkAsync(dto.Email);

            TempData["Mensaje"] =
                "Si existe una cuenta con ese correo, recibirás instrucciones para restablecer tu contraseña.";

            return RedirectToAction(nameof(Login));
        }

        // =====================================
        // MOSTRAR FORMULARIO DE NUEVA CONTRASEÑA
        // =====================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(
            string email,
            string token)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(token))
            {
                TempData["Error"] =
                    "El enlace de recuperación no es válido.";

                return RedirectToAction(nameof(ForgotPassword));
            }

            var dto = new ResetPasswordRequestDto
            {
                Email = email,
                Token = token
            };

            return View(dto);
        }

        // =====================================
        // GUARDAR NUEVA CONTRASEÑA
        // =====================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var resultado =
                await _service.ResetPasswordAsync(dto);

            if (!resultado)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El enlace es inválido o ha expirado. Solicita uno nuevo.");

                return View(dto);
            }

            TempData["Mensaje"] =
                "Tu contraseña fue actualizada correctamente. Ya puedes iniciar sesión.";

            return RedirectToAction(nameof(Login));
        }

        // =====================================
        // REENVIAR CONFIRMACION DE CORREO
        // =====================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationEmail(
            ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Introduce un correo electrónico válido.";

                return RedirectToAction(nameof(Login));
            }

            await _service.ResendConfirmationEmailAsync(dto.Email);

            TempData["Mensaje"] =
                "Si la cuenta existe y aún no está confirmada, recibirás un nuevo correo de confirmación.";

            return RedirectToAction(nameof(Login));
        }

        // =====================================
        // CERRAR SESION
        // =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };

            Response.Cookies.Delete("access_token", options);
            Response.Cookies.Delete("refresh_token", options);

            return RedirectToAction(nameof(Login));
        }
    }
}
