
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly IProveedorService _service;

        public ProveedorController(
            IProveedorService service)
        {
            _service = service;
        }

        // ========================================
        // 1. VISTA PRINCIPAL
        // GET: /Proveedor/Index
        // ========================================

        [Authorize(Policy = "proveedor.ver")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ========================================
        // 2. DATATABLES - PAGINACION SQL SERVER
        // POST: /Proveedor/GetData
        // ========================================

        [Authorize(Policy = "proveedor.ver")]
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
        // 3. CONSULTAR PROVEEDOR POR ID
        // GET: /Proveedor/GetById?id=1
        // ========================================

        [Authorize(Policy = "proveedor.ver")]
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

            var proveedor = await _service
                .ObtenerPorIdAsync(id);

            if (proveedor == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "El proveedor no existe."
                });
            }

            return Ok(MapearDto(proveedor));
        }

        // ========================================
        // 4. LISTAR PROVEEDORES ACTIVOS
        // GET: /Proveedor/GetActivos
        // ========================================

        [Authorize(Policy = "proveedor.ver")]
        [HttpGet]
        public async Task<IActionResult> GetActivos()
        {
            var proveedores = await _service
                .ObtenerTodosAsync();

            var resultado = proveedores
                .Where(p => p.Activo)
                .OrderBy(p => p.RazonSocial)
                .Select(p => new
                {
                    id = p.Id,
                    codigo = p.Codigo,
                    razonSocial = p.RazonSocial,
                    nombreComercial = p.NombreComercial
                })
                .ToList();

            return Ok(resultado);
        }

        // ========================================
        // 5. CREAR PROVEEDOR - AJAX
        // POST: /Proveedor/Create
        // ========================================

        [Authorize(Policy = "proveedor.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] ProveedorGuardarDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos del proveedor no son válidos."
                });
            }

            try
            {
                var proveedor = new Proveedor
                {
                    Codigo = dto.Codigo,
                    RazonSocial = dto.RazonSocial,
                    NombreComercial = dto.NombreComercial,
                    Ruc = dto.Ruc,
                    Contacto = dto.Contacto,
                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    Direccion = dto.Direccion
                };

                await _service.CrearAsync(proveedor);

                return Ok(new
                {
                    success = true,
                    id = proveedor.Id,
                    message = "Proveedor registrado correctamente."
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
        // 6. ACTUALIZAR PROVEEDOR - AJAX
        // POST: /Proveedor/Update?id=1
        // ========================================

        [Authorize(Policy = "proveedor.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] ProveedorGuardarDto? dto)
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
                    message = "Los datos del proveedor no son válidos."
                });
            }

            try
            {
                var proveedor = new Proveedor
                {
                    Id = id,
                    Codigo = dto.Codigo,
                    RazonSocial = dto.RazonSocial,
                    NombreComercial = dto.NombreComercial,
                    Ruc = dto.Ruc,
                    Contacto = dto.Contacto,
                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    Direccion = dto.Direccion
                };

                await _service.ActualizarAsync(proveedor);

                return Ok(new
                {
                    success = true,
                    message = "Proveedor actualizado correctamente."
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
        // 7. DESACTIVAR PROVEEDOR
        // POST: /Proveedor/Delete?id=1
        // ========================================

        [Authorize(Policy = "proveedor.borrar")]
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
                    message = "Proveedor desactivado correctamente."
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
        // 8. MAPEAR ENTIDAD A DTO
        // ========================================

        private static ProveedorDto MapearDto(
            Proveedor proveedor)
        {
            return new ProveedorDto
            {
                Id = proveedor.Id,
                Codigo = proveedor.Codigo,
                RazonSocial = proveedor.RazonSocial,
                NombreComercial = proveedor.NombreComercial,
                Ruc = proveedor.Ruc,
                Contacto = proveedor.Contacto,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                Direccion = proveedor.Direccion,
                Activo = proveedor.Activo
            };
        }
    }
}
