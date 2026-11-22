
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    public class LoteController : Controller
    {
        private readonly ILoteService _service;

        public LoteController(ILoteService service)
        {
            _service = service;
        }

        // ========================================
        // VISTA PRINCIPAL
        // GET: /Lote/Index
        // ========================================

        [Authorize(Policy = "lote.ver")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ========================================
        // DATATABLES - PAGINACION SQL SERVER
        // POST: /Lote/GetData
        // ========================================

        [Authorize(Policy = "lote.ver")]
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

            var resultado =
                await _service.ObtenerPaginadasAsync(request);

            return Ok(resultado);
        }

        // ========================================
        // CONSULTAR LOTE POR ID
        // GET: /Lote/GetById?id=1
        // ========================================

        [Authorize(Policy = "lote.ver")]
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

            var lote = await _service.ObtenerPorIdAsync(id);

            if (lote == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Lote no encontrado."
                });
            }

            return Ok(MapearDto(lote));
        }

        // ========================================
        // CREAR LOTE
        // POST: /Lote/Create
        // ========================================

        [Authorize(Policy = "lote.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] LoteGuardarDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos del lote no son válidos."
                });
            }

            try
            {
                var lote = new Lote
                {
                    MedicamentoId = dto.MedicamentoId,
                    NumeroLote = dto.NumeroLote,
                    AlmacenId = dto.AlmacenId,

                    CantidadInicial = dto.CantidadInicial,
                    CostoUnitario = dto.CostoUnitario,

                    FechaIngreso = dto.FechaIngreso,
                    FechaVencimiento = dto.FechaVencimiento
                };

                await _service.CrearAsync(lote);

                return Ok(new
                {
                    success = true,
                    id = lote.Id,
                    message = "Lote registrado correctamente."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ========================================
        // ACTUALIZAR LOTE
        // POST: /Lote/Update?id=1
        // ========================================

        [Authorize(Policy = "lote.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] LoteActualizarDto dto)
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
                    message = "Los datos del lote no son válidos."
                });
            }

            try
            {
                var lote = new Lote
                {
                    Id = id,
                    MedicamentoId = dto.MedicamentoId,
                    NumeroLote = dto.NumeroLote,
                    AlmacenId = dto.AlmacenId,

                    CostoUnitario = dto.CostoUnitario,
                    FechaIngreso = dto.FechaIngreso,
                    FechaVencimiento = dto.FechaVencimiento
                };

                await _service.ActualizarAsync(lote);

                return Ok(new
                {
                    success = true,
                    message = "Lote actualizado correctamente."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ========================================
        // DESACTIVAR LOTE
        // POST: /Lote/Delete?id=1
        // ========================================

        [Authorize(Policy = "lote.borrar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El identificador no es válido."
                });
            }

            try
            {
                await _service.DesactivarAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Lote desactivado correctamente."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
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
                MedicamentoNombre =
                    lote.Medicamento?.Nombre ?? string.Empty,

                NumeroLote = lote.NumeroLote,

                AlmacenId = lote.AlmacenId,
                AlmacenNombre =
                    lote.Almacen?.Nombre ?? string.Empty,

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
