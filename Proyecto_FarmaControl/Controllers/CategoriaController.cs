
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;

namespace Proyecto_FarmaControl.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ICategoriaService _service;

        public CategoriaController(ICategoriaService service)
        {
            _service = service;
        }

        // =====================================
        // INDEX
        // =====================================

        [Authorize(Policy = "categoria.ver")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categorias = await _service.ObtenerTodasAsync();

            var resultado = categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo
            }).ToList();

            return View(resultado);
        }

        // =====================================
        // DATATABLES - OBTENER DATOS
        // =====================================

        [Authorize(Policy = "categoria.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData(
            [FromForm] DataTablesRequestDto request)
        {
            var result = await _service.ObtenerPaginadasAsync(request);

            return Ok(result);
        }

        // =====================================
        // OBTENER CATEGORIA POR ID
        // =====================================

        [Authorize(Policy = "categoria.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.ObtenerPorIdAsync(id);

            if (result == null)
                return NotFound(new
                {
                    message = "Categoría no encontrada."
                });

            return Ok(result);
        }

        // =====================================
        // CREAR CATEGORIA - AJAX
        // =====================================

        [Authorize(Policy = "categoria.crear")]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CategoriaGuardarDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Error en los datos enviados."
                });

            var (success, errorMessage) =
                await _service.CrearAsync(dto);

            return success
                ? Ok(new
                {
                    success = true,
                    message = "Categoría creada correctamente."
                })
                : BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
        }

        // =====================================
        // ACTUALIZAR CATEGORIA - AJAX
        // =====================================

        [Authorize(Policy = "categoria.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update(
            [FromBody] CategoriaGuardarDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Error en los datos enviados."
                });

            var (success, errorMessage) =
                await _service.ActualizarAsync(dto);

            return success
                ? Ok(new
                {
                    success = true,
                    message = "Categoría actualizada correctamente."
                })
                : BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
        }

        // =====================================
        // DESACTIVAR CATEGORIA - AJAX
        // =====================================

        [Authorize(Policy = "categoria.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) =
                await _service.DesactivarAsync(id);

            return success
                ? Ok(new
                {
                    success = true,
                    message = "Categoría desactivada correctamente."
                })
                : BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
        }

        // =====================================
        // FORMULARIOS RAZOR TEMPORALES
        // Se conservan mientras se integra
        // la nueva interfaz AJAX.
        // =====================================

        [Authorize(Policy = "categoria.crear")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new CategoriaGuardarDto());
        }

        [Authorize(Policy = "categoria.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            CategoriaGuardarDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var (success, errorMessage) =
                await _service.CrearAsync(dto);

            if (!success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    errorMessage ?? "No se pudo registrar la categoría.");

                return View(dto);
            }

            TempData["Mensaje"] =
                "Categoría registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "categoria.actualizar")]
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var dto = await _service.ObtenerPorIdAsync(id);

            if (dto == null)
                return NotFound();

            ViewData["Id"] = id;

            return View(dto);
        }

        [Authorize(Policy = "categoria.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            CategoriaGuardarDto dto)
        {
            ViewData["Id"] = id;
            dto.Id = id;

            if (!ModelState.IsValid)
                return View(dto);

            var (success, errorMessage) =
                await _service.ActualizarAsync(dto);

            if (!success)
            {
                if (errorMessage == "La categoría no existe.")
                    return NotFound();

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage ?? "No se pudo actualizar la categoría.");

                return View(dto);
            }

            TempData["Mensaje"] =
                "Categoría actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "categoria.borrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var (success, errorMessage) =
                await _service.DesactivarAsync(id);

            TempData[success ? "Mensaje" : "Error"] =
                success
                    ? "Categoría desactivada correctamente."
                    : errorMessage;

            return RedirectToAction(nameof(Index));
        }
    }
}
