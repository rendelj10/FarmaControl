
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ICategoriaService _service;

        public CategoriaController(ICategoriaService service)
        {
            _service = service;
        }

        // GET: Categoria/Index
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

        // GET: Categoria/Crear
        [Authorize(Policy = "categoria.crear")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new CategoriaGuardarDto());
        }

        // POST: Categoria/Crear
        [Authorize(Policy = "categoria.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            CategoriaGuardarDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var categoria = new Categoria
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion
                };

                await _service.CrearAsync(categoria);

                TempData["Mensaje"] =
                    "Categoría registrada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);
            }

            return View(dto);
        }

        // GET: Categoria/Editar/1
        [Authorize(Policy = "categoria.actualizar")]
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            ViewData["Id"] = id;

            var categoria = await _service.ObtenerPorIdAsync(id);

            if (categoria == null)
                return NotFound();

            var dto = new CategoriaGuardarDto
            {
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };

            return View(dto);
        }

        // POST: Categoria/Editar/1
        [Authorize(Policy = "categoria.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            CategoriaGuardarDto dto)
        {
            ViewData["Id"] = id;

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var categoria = new Categoria
                {
                    Id = id,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion
                };

                await _service.ActualizarAsync(categoria);

                TempData["Mensaje"] =
                    "Categoría actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty, ex.Message);
            }

            return View(dto);
        }

        // POST: Categoria/Desactivar/1
        [Authorize(Policy = "categoria.borrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            try
            {
                await _service.DesactivarAsync(id);

                TempData["Mensaje"] =
                    "Categoría desactivada correctamente.";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
