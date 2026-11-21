
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Entensions;
using Proyecto_FarmaControl.Service;

namespace Proyecto_FarmaControl.Controllers
{
    public class AdminController : Controller
    {
        private readonly IRolePermissionService _rolePermissionService;

        public AdminController(
            IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }

        // =====================================
        // OBTENER PERMISOS EFECTIVOS DEL USUARIO
        // =====================================

        private async Task<IReadOnlyList<string>>
            GetEffectivePermissionAsync()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
            {
                return User.GetPermission();
            }

            var dbUser =
                await _rolePermissionService
                    .GetUserPermissionsAsync(userId);

            return dbUser != null
                ? dbUser.EffectivePermission
                : User.GetPermission();
        }

        // =====================================
        // TOKEN ANTIFORGERY PARA SOLICITUDES FETCH
        // GET: /Admin/ObtenerToken
        // =====================================

        [Authorize]
        [HttpGet]
        public IActionResult ObtenerToken(
            [FromServices] IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { requestToken = tokens.RequestToken });
        }

        // =====================================
        // PANEL PRINCIPAL
        // =====================================

        [Authorize(Policy = "admin.panel")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Permissions =
                await GetEffectivePermissionAsync();

            return View();
        }

        // =====================================
        // PERMISOS
        // =====================================

        [Authorize(Policy = "permiso.ver")]
        [HttpPost]
        public async Task<IActionResult> GetPermissionsData(
            [FromForm] DataTablesRequestDto request)
        {
            var result =
                await _rolePermissionService
                    .GetPermissionsPagedAsync(request);

            return Ok(result);
        }

        [Authorize(Policy = "permiso.ver")]
        [HttpGet]
        public async Task<IActionResult> Permissions()
        {
            var effective =
                await GetEffectivePermissionAsync();

            ViewBag.CanCreate = effective.Any(p =>
                string.Equals(
                    p,
                    "permiso.crear",
                    StringComparison.OrdinalIgnoreCase));

            ViewBag.CanUpdate = effective.Any(p =>
                string.Equals(
                    p,
                    "permiso.actualizar",
                    StringComparison.OrdinalIgnoreCase));

            ViewBag.CanDelete = effective.Any(p =>
                string.Equals(
                    p,
                    "permiso.borrar",
                    StringComparison.OrdinalIgnoreCase));

            return View();
        }

        // =====================================
        // OBTENER PERMISO POR ID
        // =====================================

        [Authorize(Policy = "permiso.ver")]
        [HttpGet]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            var permission =
                await _rolePermissionService
                    .GetPermissionByIdAsync(id);

            if (permission == null)
            {
                return NotFound(new
                {
                    message = "Permiso no encontrado."
                });
            }

            return Ok(permission);
        }

        // =====================================
        // CREAR PERMISO
        // =====================================

        [Authorize(Policy = "permiso.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePermission(
            [FromBody] SavePermissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Revise los datos del formulario."
                });
            }

            var (success, _, errorMessage) =
                await _rolePermissionService
                    .CreatePermissionAsync(dto);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Permiso '{dto.Name}' creado correctamente."
            });
        }

        // =====================================
        // ACTUALIZAR PERMISO
        // =====================================

        [Authorize(Policy = "permiso.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePermission(
            [FromBody] SavePermissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Revise los datos del formulario."
                });
            }

            var (success, errorMessage) =
                await _rolePermissionService
                    .UpdatePermissionAsync(dto);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Permiso '{dto.Name}' actualizado correctamente."
            });
        }

        // =====================================
        // ELIMINAR PERMISO
        // =====================================

        [Authorize(Policy = "permiso.borrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var (success, errorMessage) =
                await _rolePermissionService
                    .DeletePermissionAsync(id);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
            }

            return Ok(new
            {
                success = true,
                message = "Permiso eliminado correctamente."
            });
        }

        // =====================================
        // ROLES
        // =====================================

        [Authorize(Policy = "rol.ver")]
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var roles =
                await _rolePermissionService.GetAllRolesAsync();

            return View(roles);
        }

        // =====================================
        // CREAR ROL
        // =====================================

        [Authorize(Policy = "rol.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(
            CreateRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                var roles =
                    await _rolePermissionService.GetAllRolesAsync();

                ViewBag.CreateError =
                    "El nombre del rol es obligatorio.";

                return View("Roles", roles);
            }

            var (success, _, errorMessage) =
                await _rolePermissionService.CreateRoleAsync(dto);

            if (!success)
            {
                var roles =
                    await _rolePermissionService.GetAllRolesAsync();

                ViewBag.CreateError = errorMessage;

                return View("Roles", roles);
            }

            TempData["Success"] =
                $"Rol '{dto.Name}' creado correctamente.";

            return RedirectToAction(nameof(Roles));
        }

        // =====================================
        // DETALLES DEL ROL
        // =====================================

        [Authorize(Policy = "rol.asignar")]
        [HttpGet]
        public async Task<IActionResult> RoleDetails(int id)
        {
            var role =
                await _rolePermissionService.GetRoleByIdAsync(id);

            if (role == null)
                return NotFound();

            var allPermissions =
                await _rolePermissionService.GetAllPermissionsAsync();

            ViewBag.AllPermission = allPermissions;

            return View(role);
        }

        // =====================================
        // ASIGNAR PERMISO A ROL
        // =====================================

        [Authorize(Policy = "rol.asignar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPermissionToRole(
            int roleId,
            int permissionId)
        {
            var (success, errorMessage) =
                await _rolePermissionService
                    .AssignPermissionToRoleAsync(
                        roleId,
                        permissionId);

            return Json(new
            {
                success,
                message = success
                    ? "Permiso asignado correctamente."
                    : errorMessage
            });
        }

        // =====================================
        // RETIRAR PERMISO DE ROL
        // =====================================

        [Authorize(Policy = "rol.asignar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePermissionFromRole(
            int roleId,
            int permissionId)
        {
            var (success, errorMessage) =
                await _rolePermissionService
                    .RemovePermissionFromRoleAsync(
                        roleId,
                        permissionId);

            return Json(new
            {
                success,
                message = success
                    ? "Permiso retirado correctamente."
                    : errorMessage
            });
        }

        // =====================================
        // USUARIOS
        // =====================================

        [Authorize(Policy = "usuario.ver")]
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users =
                await _rolePermissionService.GetAllUsersAsync();

            return View(users);
        }

        // =====================================
        // PERMISOS DEL USUARIO
        // =====================================

        [Authorize(Policy = "usuario.asignar")]
        [HttpGet]
        public async Task<IActionResult> UserPermissions(int id)
        {
            var user =
                await _rolePermissionService
                    .GetUserPermissionsAsync(id);

            if (user == null)
                return NotFound();

            var allPermissions =
                await _rolePermissionService.GetAllPermissionsAsync();

            var allRoles =
                await _rolePermissionService.GetAllRolesAsync();

            var effective =
                await GetEffectivePermissionAsync();

            ViewBag.AllPermissions = allPermissions;
            ViewBag.AllRoles = allRoles;

            ViewBag.CanChangerRole = effective.Any(p =>
                string.Equals(
                    p,
                    "usuario.cambiar.rol",
                    StringComparison.OrdinalIgnoreCase));

            return View(user);
        }

        // =====================================
        // ASIGNAR PERMISO INDIVIDUAL
        // =====================================

        [Authorize(Policy = "usuario.asignar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssingPermissionToUser(
            int userId,
            [FromBody] AssignUserPermissionDto dto)
        {
            var (success, errorMessage) =
                await _rolePermissionService
                    .AssignPermissionToUserAsync(userId, dto);

            return Json(new
            {
                success,
                message = success
                    ? (dto.IsGranted
                        ? "Permiso concedido al usuario."
                        : "Permiso denegado al usuario.")
                    : errorMessage
            });
        }

        // =====================================
        // RETIRAR EXCEPCION INDIVIDUAL
        // =====================================

        [Authorize(Policy = "usuario.asignar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserPermission(
            int userId,
            int permissionId)
        {
            var (success, errorMessage) =
                await _rolePermissionService
                    .RemoveUserPermissionAsync(
                        userId,
                        permissionId);

            return Json(new
            {
                success,
                message = success
                    ? "Asignación individual retirada correctamente."
                    : errorMessage
            });
        }

        // =====================================
        // ACTUALIZAR ROL DEL USUARIO
        // =====================================

        [Authorize(Policy = "usuario.cambiar.rol")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(
            int userId,
            [FromBody] UpdateUserRoleDto dto)
        {
            var (success, errorMessage) =
                await _rolePermissionService
                    .UpdateUserRoleAsync(userId, dto.RolId);

            return Json(new
            {
                success,
                message = success
                    ? "Rol actualizado correctamente."
                    : errorMessage
            });
        }
    }
}
