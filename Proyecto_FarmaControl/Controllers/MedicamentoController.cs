
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    [Route("api/medicamentos")]
    [ApiController]
    public class MedicamentoController : ControllerBase
    {
        private readonly IMedicamentoService _service;

        public MedicamentoController(IMedicamentoService service)
        {
            _service = service;
        }

        // GET: api/medicamentos
        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var medicamentos = await _service.ObtenerTodasAsync();

            var resultado = medicamentos.Select(MapearDto);

            return Ok(resultado);
        }

        // GET: api/medicamentos/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var medicamento = await _service.ObtenerPorIdAsync(id);

            if (medicamento == null)
                return NotFound(new
                {
                    mensaje = "Medicamento no encontrado."
                });

            return Ok(MapearDto(medicamento));
        }

        // POST: api/medicamentos
        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] MedicamentoGuardarDto dto)
        {
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

                var creado = await _service
                    .ObtenerPorIdAsync(medicamento.Id);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = medicamento.Id },
                    MapearDto(creado!));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        // PUT: api/medicamentos/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] MedicamentoGuardarDto dto)
        {
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

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        // DELETE: api/medicamentos/1
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
                return NotFound(new { mensaje = ex.Message });
            }
        }

        private static MedicamentoDto MapearDto(
            Medicamento medicamento)
        {
            return new MedicamentoDto
            {
                Id = medicamento.Id,
                Codigo = medicamento.Codigo,
                Nombre = medicamento.Nombre,
                PrincipioActivo = medicamento.PrincipioActivo,
                Concentracion = medicamento.Concentracion,
                FormaFarmaceutica = medicamento.FormaFarmaceutica,
                Presentacion = medicamento.Presentacion,
                CategoriaId = medicamento.CategoriaId,
                CategoriaNombre = medicamento.Categoria?.Nombre
                    ?? string.Empty,
                Activo = medicamento.Activo
            };
        }
    }
}
