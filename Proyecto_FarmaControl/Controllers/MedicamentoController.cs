
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

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

        // GET: Medicamento/Index
        [Authorize(Policy = "medicamento.ver")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var medicamentos = await _service.ObtenerTodasAsync();

            var resultado = medicamentos.Select(m => new MedicamentoDto
            {
                Id = m.Id,
                Codigo = m.Codigo,
                Nombre = m.Nombre,
                PrincipioActivo = m.PrincipioActivo,
                Concentracion = m.Concentracion,
                FormaFarmaceutica = m.FormaFarmaceutica,
                Presentacion = m.Presentacion,
                CategoriaId = m.CategoriaId,
                CategoriaNombre = m.Categoria?.Nombre ?? string.Empty,
                Activo = m.Activo
            }).ToList();

            return View(resultado);
        }

        // GET: Medicamento/Crear
        [Authorize(Policy = "medicamento.crear")]
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarCategoriasAsync();

            return View(new MedicamentoGuardarDto());
        }

        // POST: Medicamento/Crear
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

            try
            {
                var medicamento = new Medicamento
                {
                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,
                    PrincipioActivo = dto.PrincipioActivo,
                    Concentracion = dto.Concentracion,
                    FormaFarmaceutica = dto.FormaFarmaceutica,
                    Presentacion = dto.Presentacion,
                    CategoriaId = dto.CategoriaId
                };

                await _service.CrearAsync(medicamento);

                TempData["Mensaje"] =
                    "Medicamento registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            await CargarCategoriasAsync(dto.CategoriaId);

            return View(dto);
        }

        // GET: Medicamento/Editar/1
        [Authorize(Policy = "medicamento.actualizar")]
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var medicamento = await _service.ObtenerPorIdAsync(id);

            if (medicamento == null)
                return NotFound();

            ViewData["Id"] = id;

            var dto = new MedicamentoGuardarDto
            {
                Codigo = medicamento.Codigo,
                Nombre = medicamento.Nombre,
                PrincipioActivo = medicamento.PrincipioActivo,
                Concentracion = medicamento.Concentracion,
                FormaFarmaceutica = medicamento.FormaFarmaceutica,
                Presentacion = medicamento.Presentacion,
                CategoriaId = medicamento.CategoriaId
            };

            await CargarCategoriasAsync(dto.CategoriaId);

            return View(dto);
        }

        // POST: Medicamento/Editar/1
        [Authorize(Policy = "medicamento.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            MedicamentoGuardarDto dto)
        {
            ViewData["Id"] = id;

            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(dto.CategoriaId);
                return View(dto);
            }

            try
            {
                var medicamento = new Medicamento
                {
                    Id = id,
                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,
                    PrincipioActivo = dto.PrincipioActivo,
                    Concentracion = dto.Concentracion,
                    FormaFarmaceutica = dto.FormaFarmaceutica,
                    Presentacion = dto.Presentacion,
                    CategoriaId = dto.CategoriaId
                };

                await _service.ActualizarAsync(medicamento);

                TempData["Mensaje"] =
                    "Medicamento actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            await CargarCategoriasAsync(dto.CategoriaId);

            return View(dto);
        }

        // POST: Medicamento/Desactivar/1
        [Authorize(Policy = "medicamento.borrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            try
            {
                await _service.DesactivarAsync(id);

                TempData["Mensaje"] =
                    "Medicamento desactivado correctamente.";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Cargar categorías activas para los formularios
        private async Task CargarCategoriasAsync(
            int? categoriaSeleccionada = null)
        {
            var categorias = await _categoriaService.ObtenerTodasAsync();

            ViewBag.Categorias = new SelectList(
                categorias.Where(c => c.Activo),
                "Id",
                "Nombre",
                categoriaSeleccionada);
        }
    }
}
