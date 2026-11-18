
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

        // GET: Auth/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginRequestDto());
        }

        // POST: Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginRequestDto dto)
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

        // GET: Auth/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterRequestDto());
        }

        // POST: Auth/Register
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

        // POST: Auth/Logout
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
