using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;

namespace Proyecto_FarmaControl.Controllers
{
    public class TurnoCajaController : Controller
    {
        private readonly ITurnoCajaService _service;

        public TurnoCajaController(ITurnoCajaService service)
        {
            _service = service;
        }

        // POST /TurnoCaja/GetData
        [Authorize(Policy = "turnocaja.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromForm] DataTablesRequestDto request)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "La solicitud no es válida." });
            return Ok(await _service.ObtenerPaginadasAsync(request));
        }

        // GET /TurnoCaja/GetAll
        [Authorize(Policy = "turnocaja.ver")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.ObtenerTodosAsync());
        }

        // GET /TurnoCaja/GetById?id=1
        [Authorize(Policy = "turnocaja.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Identificador inválido." });
            var turno = await _service.ObtenerPorIdAsync(id);
            return turno == null
                ? NotFound(new { success = false, message = "El turno no existe." })
                : Ok(turno);
        }

        // GET /TurnoCaja/MiTurnoAbierto
        [Authorize(Policy = "turnocaja.ver")]
        [HttpGet]
        public async Task<IActionResult> MiTurnoAbierto()
        {
            if (!ObtenerUsuarioId(out int usuarioId))
                return Unauthorized(new { success = false, message = "No se pudo identificar al usuario." });
            var turno = await _service.ObtenerAbiertoPorUsuarioAsync(usuarioId);
            return turno == null
                ? NotFound(new { success = false, message = "No hay turno abierto para el usuario." })
                : Ok(turno);
        }

        // POST /TurnoCaja/Abrir
        [Authorize(Policy = "turnocaja.abrir")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abrir([FromBody] TurnoCajaAbrirDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(new { success = false, message = "Los datos del turno no son válidos." });

            if (!ObtenerUsuarioId(out int usuarioId))
                return Unauthorized(new { success = false, message = "No se pudo identificar al usuario." });

            try
            {
                var turno = await _service.AbrirAsync(dto, usuarioId);
                return Ok(new
                {
                    success = true,
                    id = turno.Id,
                    sucursalId = turno.SucursalId,
                    fechaApertura = turno.FechaApertura,
                    montoInicial = turno.MontoInicial,
                    estado = turno.Estado,
                    message = "Turno de caja abierto correctamente."
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

        // POST /TurnoCaja/Cerrar?id=1
        [Authorize(Policy = "turnocaja.cerrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cerrar(
            int id, [FromBody] TurnoCajaCerrarDto? dto)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Identificador inválido." });

            if (dto == null || !ModelState.IsValid)
                return BadRequest(new { success = false, message = "Los datos del cierre no son válidos." });

            if (!ObtenerUsuarioId(out int usuarioId))
                return Unauthorized(new { success = false, message = "No se pudo identificar al usuario." });

            try
            {
                var turno = await _service.CerrarAsync(id, dto, usuarioId);
                return Ok(new
                {
                    success = true,
                    id = turno.Id,
                    estado = turno.Estado,
                    fechaCierre = turno.FechaCierre,
                    montoInicial = turno.MontoInicial,
                    montoEsperado = turno.MontoEsperado,
                    montoContado = turno.MontoContado,
                    diferencia = turno.Diferencia,
                    usuarioCierreId = turno.UsuarioCierreId,
                    message = "Turno de caja cerrado correctamente."
                });
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
        private bool ObtenerUsuarioId(out int usuarioId)
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),
                out usuarioId) && usuarioId > 0;
        }
    }
}
