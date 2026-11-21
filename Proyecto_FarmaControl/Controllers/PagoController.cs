using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;

namespace Proyecto_FarmaControl.Controllers
{
    public class PagoController : Controller
    {
        private readonly IPagoService _service;

        public PagoController(IPagoService service)
        {
            _service = service;
        }

        [Authorize(Policy = "pago.ver")]
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.ObtenerTodosAsync());

        [Authorize(Policy = "pago.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Identificador inválido." });
            var pago = await _service.ObtenerPorIdAsync(id);
            return pago == null
                ? NotFound(new { success = false, message = "El pago no existe." })
                : Ok(pago);
        }

        [Authorize(Policy = "pago.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromForm] DataTablesRequestDto request)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Solicitud inválida." });
            return Ok(await _service.ObtenerPaginadasAsync(request));
        }

        [Authorize(Policy = "pago.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] PagoGuardarDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(new { success = false, message = "Datos de pago no válidos." });
            if (!ObtenerUsuarioId(out int usuarioId))
                return Unauthorized(new { success = false, message = "Usuario no identificado." });
            try
            {
                var pago = await _service.RegistrarAsync(dto, usuarioId);
                return Ok(new { success = true, id = pago.Id,
                    estado = pago.Estado, monto = pago.Monto,
                    message = "Pago registrado correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Policy = "pago.anular")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id)
        {
            try
            {
                var pago = await _service.AnularAsync(id);
                return Ok(new { success = true, id = pago.Id,
                    estado = pago.Estado, message = "Pago anulado correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        [NonAction]
        private bool ObtenerUsuarioId(out int usuarioId) =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),
                out usuarioId) && usuarioId > 0;
    }
}
