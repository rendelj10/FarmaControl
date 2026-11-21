using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Service;

namespace Proyecto_FarmaControl.Controllers
{
    public class MetodoPagoController : Controller
    {
        private readonly IMetodoPagoService _service;

        public MetodoPagoController(IMetodoPagoService service)
        {
            _service = service;
        }

        [Authorize(Policy = "metodopago.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData(
            [FromForm] DataTablesRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Solicitud no válida."
                });
            }

            return Ok(await _service.ObtenerPaginadosAsync(request));
        }

        [Authorize(Policy = "metodopago.ver")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.ObtenerTodosAsync();
            return Ok(items.Select(MapearDto));
        }

        [Authorize(Policy = "metodopago.ver")]
        [HttpGet]
        public async Task<IActionResult> GetActivos()
        {
            var items = await _service.ObtenerTodosAsync();

            return Ok(items
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    id = m.Id,
                    nombre = m.Nombre,
                    requiereReferencia = m.RequiereReferencia
                }));
        }

        [Authorize(Policy = "metodopago.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El identificador no es válido."
                });
            }

            var item = await _service.ObtenerPorIdAsync(id);

            if (item == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "El método de pago no existe."
                });
            }

            return Ok(MapearDto(item));
        }

        [Authorize(Policy = "metodopago.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] MetodoPagoGuardarDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos del método de pago no son válidos."
                });
            }

            try
            {
                var metodoPago = new MetodoPago
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    RequiereReferencia = dto.RequiereReferencia
                };

                await _service.CrearAsync(metodoPago);

                return Ok(new
                {
                    success = true,
                    id = metodoPago.Id,
                    message = "Método de pago registrado correctamente."
                });
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

        [Authorize(Policy = "metodopago.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] MetodoPagoGuardarDto? dto)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El identificador no es válido."
                });
            }

            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos del método de pago no son válidos."
                });
            }

            try
            {
                await _service.ActualizarAsync(new MetodoPago
                {
                    Id = id,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    RequiereReferencia = dto.RequiereReferencia
                });

                return Ok(new
                {
                    success = true,
                    message = "Método de pago actualizado correctamente."
                });
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

        [Authorize(Policy = "metodopago.borrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DesactivarAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Método de pago desactivado correctamente."
                });
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

        private static MetodoPagoDto MapearDto(MetodoPago item) => new()
        {
            Id = item.Id,
            Nombre = item.Nombre,
            Descripcion = item.Descripcion,
            RequiereReferencia = item.RequiereReferencia,
            Activo = item.Activo
        };
    }
}
