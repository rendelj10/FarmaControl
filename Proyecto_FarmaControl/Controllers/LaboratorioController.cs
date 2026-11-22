using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    public class LaboratorioController : Controller
    {
        private readonly ILaboratorioService _service;

        public LaboratorioController(ILaboratorioService service)
        {
            _service = service;
        }

        [Authorize(Policy = "laboratorio.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromForm] DataTablesRequestDto request)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Solicitud no válida." });

            return Ok(await _service.ObtenerPaginadosAsync(request));
        }

        [Authorize(Policy = "laboratorio.ver")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.ObtenerTodosAsync();
            return Ok(items.Select(MapearDto));
        }

        [Authorize(Policy = "laboratorio.ver")]
        [HttpGet]
        public async Task<IActionResult> GetActivos()
        {
            var items = await _service.ObtenerTodosAsync();
            return Ok(items.Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .Select(x => new { id = x.Id, nombre = x.Nombre }));
        }

        [Authorize(Policy = "laboratorio.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "El identificador no es válido." });

            var item = await _service.ObtenerPorIdAsync(id);
            if (item == null)
                return NotFound(new { success = false, message = "El laboratorio no existe." });

            return Ok(MapearDto(item));
        }

        [Authorize(Policy = "laboratorio.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] LaboratorioGuardarDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(new { success = false, message = "Los datos del laboratorio no son válidos." });

            try
            {
                var item = new Laboratorio
                {
                    Nombre = dto.Nombre,
                    Pais = dto.Pais,
                    Contacto = dto.Contacto,
                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    Direccion = dto.Direccion
                };

                await _service.CrearAsync(item);
                return Ok(new { success = true, id = item.Id, message = "Laboratorio registrado correctamente." });
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

        [Authorize(Policy = "laboratorio.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromBody] LaboratorioGuardarDto? dto)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "El identificador no es válido." });
            if (dto == null || !ModelState.IsValid)
                return BadRequest(new { success = false, message = "Los datos del laboratorio no son válidos." });

            try
            {
                await _service.ActualizarAsync(new Laboratorio
                {
                    Id = id,
                    Nombre = dto.Nombre,
                    Pais = dto.Pais,
                    Contacto = dto.Contacto,
                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    Direccion = dto.Direccion
                });

                return Ok(new { success = true, message = "Laboratorio actualizado correctamente." });
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

        [Authorize(Policy = "laboratorio.borrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DesactivarAsync(id);
                return Ok(new { success = true, message = "Laboratorio desactivado correctamente." });
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

        private static LaboratorioDto MapearDto(Laboratorio item) => new()
        {
            Id = item.Id,
            Nombre = item.Nombre,
            Pais = item.Pais,
            Contacto = item.Contacto,
            Telefono = item.Telefono,
            Email = item.Email,
            Direccion = item.Direccion,
            Activo = item.Activo
        };
    }
}
