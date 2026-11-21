
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    public class PresentacionController : Controller
    {
        private readonly IPresentacionService _service;

        public PresentacionController(IPresentacionService service)
        {
            _service = service;
        }

        [Authorize(Policy = "presentacion.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromForm] DataTablesRequestDto request)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Solicitud no válida." });

            return Ok(await _service.ObtenerPaginadasAsync(request));
        }

        [Authorize(Policy = "presentacion.ver")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.ObtenerTodasAsync();
            return Ok(items.Select(MapearDto));
        }

        [Authorize(Policy = "presentacion.ver")]
        [HttpGet]
        public async Task<IActionResult> GetActivas()
        {
            var items = await _service.ObtenerTodasAsync();
            return Ok(items.Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .Select(x => new
                {
                    id = x.Id,
                    nombre = x.Nombre,
                    cantidadContenido = x.CantidadContenido,
                    unidadContenido = x.UnidadContenido
                }));
        }

        [Authorize(Policy = "presentacion.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "El identificador no es válido." });

            var item = await _service.ObtenerPorIdAsync(id);
            if (item == null)
                return NotFound(new { success = false, message = "La presentación no existe." });

            return Ok(MapearDto(item));
        }

        [Authorize(Policy = "presentacion.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] PresentacionGuardarDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(new { success = false, message = "Los datos de la presentación no son válidos." });

            try
            {
                var item = new Presentacion
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    CantidadContenido = dto.CantidadContenido,
                    UnidadContenido = dto.UnidadContenido
                };

                await _service.CrearAsync(item);
                return Ok(new { success = true, id = item.Id, message = "Presentación registrada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Policy = "presentacion.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromBody] PresentacionGuardarDto? dto)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "El identificador no es válido." });

            if (dto == null || !ModelState.IsValid)
                return BadRequest(new { success = false, message = "Los datos de la presentación no son válidos." });

            try
            {
                await _service.ActualizarAsync(new Presentacion
                {
                    Id = id,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    CantidadContenido = dto.CantidadContenido,
                    UnidadContenido = dto.UnidadContenido
                });

                return Ok(new { success = true, message = "Presentación actualizada correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Policy = "presentacion.borrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DesactivarAsync(id);
                return Ok(new { success = true, message = "Presentación desactivada correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        private static PresentacionDto MapearDto(Presentacion item) => new()
        {
            Id = item.Id,
            Nombre = item.Nombre,
            Descripcion = item.Descripcion,
            CantidadContenido = item.CantidadContenido,
            UnidadContenido = item.UnidadContenido,
            Activo = item.Activo
        };
    }
}
