
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    [Route("api/lotes")]
    [ApiController]
    public class LoteController : ControllerBase
    {
        private readonly ILoteService _service;

        public LoteController(ILoteService service)
        {
            _service = service;
        }

        // ========================================
        // GET: api/lotes
        // CONSULTAR TODOS LOS LOTES
        // ========================================

        [Authorize(Policy = "lote.ver")]
        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var lotes = await _service.ObtenerTodasAsync();

            var resultado = lotes.Select(MapearDto).ToList();

            return Ok(resultado);
        }

        // ========================================
        // GET: api/lotes/1
        // CONSULTAR LOTE POR ID
        // ========================================

        [Authorize(Policy = "lote.ver")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var lote = await _service.ObtenerPorIdAsync(id);

            if (lote == null)
            {
                return NotFound(new
                {
                    mensaje = "Lote no encontrado."
                });
            }

            return Ok(MapearDto(lote));
        }

        // ========================================
        // POST: api/lotes
        // REGISTRAR NUEVO LOTE
        // ========================================

        [Authorize(Policy = "lote.crear")]
        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] LoteGuardarDto dto)
        {
            try
            {
                var lote = new Lote
                {
                    MedicamentoId = dto.MedicamentoId,
                    NumeroLote = dto.NumeroLote,
                    Almacen = dto.Almacen,
                    CantidadInicial = dto.CantidadInicial,
                    CostoUnitario = dto.CostoUnitario,
                    FechaIngreso = dto.FechaIngreso,
                    FechaVencimiento = dto.FechaVencimiento
                };

                await _service.CrearAsync(lote);

                var creado = await _service
                    .ObtenerPorIdAsync(lote.Id);

                if (creado == null)
                {
                    return StatusCode(500, new
                    {
                        mensaje = "El lote fue registrado, pero no se pudo recuperar."
                    });
                }

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = lote.Id },
                    MapearDto(creado));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    mensaje = ex.Message
                });
            }
        }

        // ========================================
        // PUT: api/lotes/1
        // ACTUALIZAR LOTE
        // ========================================

        [Authorize(Policy = "lote.actualizar")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] LoteGuardarDto dto)
        {
            try
            {
                var lote = new Lote
                {
                    Id = id,
                    MedicamentoId = dto.MedicamentoId,
                    NumeroLote = dto.NumeroLote,
                    Almacen = dto.Almacen,
                    CantidadInicial = dto.CantidadInicial,
                    CostoUnitario = dto.CostoUnitario,
                    FechaIngreso = dto.FechaIngreso,
                    FechaVencimiento = dto.FechaVencimiento
                };

                await _service.ActualizarAsync(lote);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    mensaje = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    mensaje = ex.Message
                });
            }
        }

        // ========================================
        // DELETE: api/lotes/1
        // DESACTIVAR LOTE
        // ========================================

        [Authorize(Policy = "lote.borrar")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Desactivar(int id)
        {
            try
            {
                await _service.DesactivarAsync(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    mensaje = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    mensaje = ex.Message
                });
            }
        }

        // ========================================
        // MAPEAR ENTIDAD A DTO
        // ========================================

        private static LoteDto MapearDto(Lote lote)
        {
            return new LoteDto
            {
                Id = lote.Id,
                MedicamentoId = lote.MedicamentoId,

                MedicamentoNombre = lote.Medicamento?.Nombre
                    ?? string.Empty,

                NumeroLote = lote.NumeroLote,
                Almacen = lote.Almacen,
                CantidadInicial = lote.CantidadInicial,
                Existencias = lote.Existencias,
                CostoUnitario = lote.CostoUnitario,
                FechaIngreso = lote.FechaIngreso,
                FechaVencimiento = lote.FechaVencimiento,
                Activo = lote.Activo
            };
        }
    }
}
