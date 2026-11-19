
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    public class AlmacenController : Controller
    {
        private readonly IAlmacenService _service;

        public AlmacenController(
            IAlmacenService service)
        {
            _service = service;
        }

        // ========================================
        // VISTA PRINCIPAL
        // GET: /Almacen/Index
        // ========================================

        [Authorize(Policy = "almacen.ver")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ========================================
        // DATATABLES - PAGINACION SQL SERVER
        // POST: /Almacen/GetData
        // ========================================

        [Authorize(Policy = "almacen.ver")]
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

            var resultado = await _service
                .ObtenerPaginadasAsync(request);

            return Ok(resultado);
        }

        // ========================================
        // CONSULTAR ALMACEN POR ID
        // GET: /Almacen/GetById?id=1
        // ========================================

        [Authorize(Policy = "almacen.ver")]
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

            var almacen = await _service
                .ObtenerPorIdAsync(id);

            if (almacen == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "El almacén no existe."
                });
            }

            return Ok(MapearDto(almacen));
        }

        // ========================================
        // LISTAR ALMACENES ACTIVOS
        // GET: /Almacen/GetActivos
        // ========================================

        [Authorize(Policy = "almacen.ver")]
        [HttpGet]
        public async Task<IActionResult> GetActivos(
            int? sucursalId)
        {
            var almacenes =
                await _service.ObtenerTodosAsync();

            var disponibles = almacenes
                .Where(a =>
                    a.Activo &&
                    a.Sucursal.Activo);

            if (sucursalId.HasValue)
            {
                disponibles = disponibles
                    .Where(a =>
                        a.SucursalId == sucursalId.Value);
            }

            var resultado = disponibles
                .OrderBy(a => a.Nombre)
                .Select(a => new
                {
                    id = a.Id,
                    sucursalId = a.SucursalId,
                    codigo = a.Codigo,
                    nombre = a.Nombre
                })
                .ToList();

            return Ok(resultado);
        }

        // ========================================
        // CREAR ALMACEN
        // POST: /Almacen/Create
        // ========================================

        [Authorize(Policy = "almacen.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] AlmacenGuardarDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos del almacén no son válidos."
                });
            }

            try
            {
                var almacen = new Almacen
                {
                    SucursalId = dto.SucursalId,
                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,
                    Ubicacion = dto.Ubicacion
                };

                await _service.CrearAsync(almacen);

                return Ok(new
                {
                    success = true,
                    id = almacen.Id,
                    message = "Almacén registrado correctamente."
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
        // ACTUALIZAR ALMACEN
        // POST: /Almacen/Update?id=1
        // ========================================

        [Authorize(Policy = "almacen.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] AlmacenGuardarDto dto)
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
                    message = "Los datos del almacén no son válidos."
                });
            }

            try
            {
                var almacen = new Almacen
                {
                    Id = id,
                    SucursalId = dto.SucursalId,
                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,
                    Ubicacion = dto.Ubicacion
                };

                await _service.ActualizarAsync(almacen);

                return Ok(new
                {
                    success = true,
                    message = "Almacén actualizado correctamente."
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
        // DESACTIVAR ALMACEN
        // POST: /Almacen/Delete?id=1
        // ========================================

        [Authorize(Policy = "almacen.borrar")]
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
                    message = "Almacén desactivado correctamente."
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
        // MAPEAR ENTIDAD A DTO
        // ========================================

        private static AlmacenDto MapearDto(
            Almacen almacen)
        {
            return new AlmacenDto
            {
                Id = almacen.Id,
                SucursalId = almacen.SucursalId,
                SucursalNombre =
                    almacen.Sucursal?.Nombre ?? string.Empty,
                Codigo = almacen.Codigo,
                Nombre = almacen.Nombre,
                Ubicacion = almacen.Ubicacion,
                Activo = almacen.Activo
            };
        }
    }
}
