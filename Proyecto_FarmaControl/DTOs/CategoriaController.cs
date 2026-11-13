
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    [Route("api/categorias")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _service;

        public CategoriaController(ICategoriaService service)
        {
            _service = service;
        }

        // GET: api/categorias
        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var categorias = await _service.ObtenerTodasAsync();

            var resultado = categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo
            });

            return Ok(resultado);
        }

        // GET: api/categorias/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var categoria = await _service.ObtenerPorIdAsync(id);

            if (categoria == null)
                return NotFound(new
                {
                    mensaje = "Categoría no encontrada."
                });

            return Ok(new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo
            });
        }

        // POST: api/categorias
        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CategoriaGuardarDto dto)
        {
            try
            {
                var categoria = new Categoria
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion
                };

                await _service.CrearAsync(categoria);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = categoria.Id },
                    new CategoriaDto
                    {
                        Id = categoria.Id,
                        Nombre = categoria.Nombre,
                        Descripcion = categoria.Descripcion,
                        Activo = categoria.Activo
                    });
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

        // PUT: api/categorias/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] CategoriaGuardarDto dto)
        {
            try
            {
                var categoria = new Categoria
                {
                    Id = id,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion
                };

                await _service.ActualizarAsync(categoria);

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

        // DELETE: api/categorias/1
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
    }
}
    