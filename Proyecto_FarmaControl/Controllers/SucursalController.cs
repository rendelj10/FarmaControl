
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    public class SucursalController : Controller
    {
        private readonly ISucursalService _service;

        public SucursalController(
            ISucursalService service)
        {
            _service = service;
        }

        // ========================================
        // VISTA PRINCIPAL
        // GET: /Sucursal/Index
        // ========================================

        [Authorize(Policy = "sucursal.ver")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ========================================
        // DATATABLES - PAGINACION SQL SERVER
        // POST: /Sucursal/GetData
        // ========================================

        [Authorize(Policy = "sucursal.ver")]
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
        // CONSULTAR SUCURSAL POR ID
        // GET: /Sucursal/GetById?id=1
        // ========================================

        [Authorize(Policy = "sucursal.ver")]
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

            var sucursal =
                await _service.ObtenerPorIdAsync(id);

            if (sucursal == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "La sucursal no existe."
                });
            }

            return Ok(MapearDto(sucursal));
        }

        // ========================================
        // LISTAR SUCURSALES ACTIVAS
        // GET: /Sucursal/GetActivas
        // ========================================

        [Authorize(Policy = "sucursal.ver")]
        [HttpGet]
        public async Task<IActionResult> GetActivas()
        {
            var sucursales =
                await _service.ObtenerTodasAsync();

            var resultado = sucursales
                .Where(s => s.Activo)
                .OrderBy(s => s.Nombre)
                .Select(s => new
                {
                    id = s.Id,
                    codigo = s.Codigo,
                    nombre = s.Nombre
                })
                .ToList();

            return Ok(resultado);
        }

        // ========================================
        // CREAR SUCURSAL
        // POST: /Sucursal/Create
        // ========================================

        [Authorize(Policy = "sucursal.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] SucursalGuardarDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos de la sucursal no son válidos."
                });
            }

            try
            {
                var sucursal = new Sucursal
                {
                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,
                    Direccion = dto.Direccion,
                    Telefono = dto.Telefono,
                    Email = dto.Email
                };

                await _service.CrearAsync(sucursal);

                return Ok(new
                {
                    success = true,
                    id = sucursal.Id,
                    message = "Sucursal registrada correctamente."
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
        // ACTUALIZAR SUCURSAL
        // POST: /Sucursal/Update?id=1
        // ========================================

        [Authorize(Policy = "sucursal.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] SucursalGuardarDto dto)
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
                    message = "Los datos de la sucursal no son válidos."
                });
            }

            try
            {
                var sucursal = new Sucursal
                {
                    Id = id,
                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,
                    Direccion = dto.Direccion,
                    Telefono = dto.Telefono,
                    Email = dto.Email
                };

                await _service.ActualizarAsync(sucursal);

                return Ok(new
                {
                    success = true,
                    message = "Sucursal actualizada correctamente."
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
        // DESACTIVAR SUCURSAL
        // POST: /Sucursal/Delete?id=1
        // ========================================

        [Authorize(Policy = "sucursal.borrar")]
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
                    message = "Sucursal desactivada correctamente."
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

        private static SucursalDto MapearDto(
            Sucursal sucursal)
        {
            return new SucursalDto
            {
                Id = sucursal.Id,
                Codigo = sucursal.Codigo,
                Nombre = sucursal.Nombre,
                Direccion = sucursal.Direccion,
                Telefono = sucursal.Telefono,
                Email = sucursal.Email,
                Activo = sucursal.Activo
            };
        }
    }
}
