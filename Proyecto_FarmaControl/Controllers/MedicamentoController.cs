
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;

namespace Proyecto_FarmaControl.Controllers
{
    public class MedicamentoController : Controller
    {
        private readonly IMedicamentoService _service;
        private readonly ICategoriaService _categoriaService;

        public MedicamentoController(
            IMedicamentoService service,
            ICategoriaService categoriaService)
        {
            _service = service;
            _categoriaService = categoriaService;
        }

        // =====================================
        // INDEX
        // =====================================

        [Authorize(Policy = "medicamento.ver")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var medicamentos = await _service.ObtenerTodasAsync();

            return View(medicamentos);
        }

        // =====================================
        // DATATABLES
        // =====================================

        [Authorize(Policy = "medicamento.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData(
            [FromForm] DataTablesRequestDto request)
        {
            var result =
                await _service.ObtenerPaginadosAsync(request);

            return Ok(result);
        }

        // =====================================
        // OBTENER POR ID
        // =====================================

        [Authorize(Policy = "medicamento.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.ObtenerPorIdAsync(id);

            if (result == null)
                return NotFound(new
                {
                    message = "Medicamento no encontrado."
                });

            return Ok(result);
        }

        // =====================================
        // OBTENER CATEGORÍAS ACTIVAS
        // =====================================

        [Authorize(Policy = "medicamento.ver")]
        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias =
                await _categoriaService.ObtenerTodasAsync();

            var resultado = categorias
                .Where(c => c.Activo)
                .Select(c => new
                {
                    id = c.Id,
                    nombre = c.Nombre
                })
                .ToList();

            return Ok(resultado);
        }

        // =====================================
        // CREAR - AJAX
        // =====================================

        [Authorize(Policy = "medicamento.crear")]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] MedicamentoGuardarDto dto)
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
                    message = "Medicamento creado correctamente."
                })
                : BadRequest(new
                {
                    success = false,
                    errorMessage
                });
        }

        // =====================================
        // ACTUALIZAR - AJAX
        // =====================================

        [Authorize(Policy = "medicamento.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update(
            [FromBody] MedicamentoGuardarDto dto)
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
                    message = "Medicamento actualizado correctamente."
                })
                : BadRequest(new
                {
                    success = false,
                    errorMessage
                });
        }

        // =====================================
        // DESACTIVAR - AJAX
        // =====================================

        [Authorize(Policy = "medicamento.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) =
                await _service.DesactivarAsync(id);

            return success
                ? Ok(new
                {
                    success = true,
                    message = "Medicamento desactivado correctamente."
                })
                : BadRequest(new
                {
                    success = false,
                    errorMessage
                });
        }

        // =====================================
        // FORMULARIOS RAZOR TEMPORALES
        // =====================================

        [Authorize(Policy = "medicamento.crear")]
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarCategoriasAsync();

            return View(new MedicamentoGuardarDto());
        }

        [Authorize(Policy = "medicamento.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            MedicamentoGuardarDto dto)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(dto.CategoriaId);
                return View(dto);
            }

            var (success, errorMessage) =
                await _service.CrearAsync(dto);

            if (!success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    errorMessage ?? "No se pudo registrar el medicamento.");

                await CargarCategoriasAsync(dto.CategoriaId);

                return View(dto);
            }

            TempData["Mensaje"] =
                "Medicamento registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "medicamento.actualizar")]
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var dto = await _service.ObtenerPorIdAsync(id);

            if (dto == null)
                return NotFound();

            ViewData["Id"] = id;

            await CargarCategoriasAsync(dto.CategoriaId);

            return View(dto);
        }

        [Authorize(Policy = "medicamento.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            MedicamentoGuardarDto dto)
        {
            dto.Id = id;
            ViewData["Id"] = id;

            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(dto.CategoriaId);
                return View(dto);
            }

            var (success, errorMessage) =
                await _service.ActualizarAsync(dto);

            if (!success)
            {
                if (errorMessage == "El medicamento no existe.")
                    return NotFound();

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage ?? "No se pudo actualizar el medicamento.");

                await CargarCategoriasAsync(dto.CategoriaId);

                return View(dto);
            }

            TempData["Mensaje"] =
                "Medicamento actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "medicamento.borrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var (success, errorMessage) =
                await _service.DesactivarAsync(id);

            TempData[success ? "Mensaje" : "Error"] =
                success
                    ? "Medicamento desactivado correctamente."
                    : errorMessage;

            return RedirectToAction(nameof(Index));
        }

        // =====================================
        // CARGAR CATEGORÍAS PARA FORMULARIOS
        // =====================================

        private async Task CargarCategoriasAsync(
            int? categoriaSeleccionada = null)
        {
            var categorias =
                await _categoriaService.ObtenerTodasAsync();

            ViewBag.Categorias = new SelectList(
                categorias.Where(c => c.Activo),
                "Id",
                "Nombre",
                categoriaSeleccionada);
        }
    }
}
