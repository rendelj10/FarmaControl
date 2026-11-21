
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IClienteService _service;

        public ClienteController(
            IClienteService service)
        {
            _service = service;
        }

        // ========================================
        // 1. VISTA PRINCIPAL
        // GET: /Cliente/Index
        // ========================================

        [Authorize(Policy = "cliente.ver")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ========================================
        // 2. DATATABLES - PAGINACION SQL
        // POST: /Cliente/GetData
        // ========================================

        [Authorize(Policy = "cliente.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData(
            [FromForm] DataTablesRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "La solicitud no es válida."
                });
            }

            var resultado = await _service
                .ObtenerPaginadasAsync(request);

            return Ok(resultado);
        }

        // ========================================
        // 3. CONSULTAR CLIENTE POR ID
        // GET: /Cliente/GetById?id=1
        // ========================================

        [Authorize(Policy = "cliente.ver")]
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

            var cliente = await _service
                .ObtenerPorIdAsync(id);

            if (cliente == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "El cliente no existe."
                });
            }

            return Ok(MapearDto(cliente));
        }

        // ========================================
        // 4. CONSULTAR CLIENTES ACTIVOS
        // GET: /Cliente/GetActivos
        // ========================================

        [Authorize(Policy = "cliente.ver")]
        [HttpGet]
        public async Task<IActionResult> GetActivos()
        {
            var clientes = await _service
                .ObtenerTodosAsync();

            var resultado = clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Id)
                .Select(MapearDto)
                .ToList();

            return Ok(resultado);
        }

        // ========================================
        // 5. CREAR CLIENTE
        // POST: /Cliente/Create
        // ========================================

        [Authorize(Policy = "cliente.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] ClienteGuardarDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos del cliente no son válidos."
                });
            }

            try
            {
                var cliente = new Cliente
                {
                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,

                    DocumentoIdentidad =
                        dto.DocumentoIdentidad,

                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    Direccion = dto.Direccion,

                    Activo = true
                };

                await _service.CrearAsync(cliente);

                return Ok(new
                {
                    success = true,
                    id = cliente.Id,
                    message = "Cliente registrado correctamente."
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
        // 6. ACTUALIZAR CLIENTE
        // POST: /Cliente/Update?id=1
        // ========================================

        [Authorize(Policy = "cliente.actualizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] ClienteGuardarDto? dto)
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
                    message = "Los datos del cliente no son válidos."
                });
            }

            try
            {
                var cliente = new Cliente
                {
                    Id = id,

                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,

                    DocumentoIdentidad =
                        dto.DocumentoIdentidad,

                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    Direccion = dto.Direccion
                };

                await _service.ActualizarAsync(cliente);

                return Ok(new
                {
                    success = true,
                    id,
                    message = "Cliente actualizado correctamente."
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
        // 7. DESACTIVAR CLIENTE
        // POST: /Cliente/Delete?id=1
        // ========================================

        [Authorize(Policy = "cliente.borrar")]
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
                    id,
                    message = "Cliente desactivado correctamente."
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
        // 8. MAPEAR MODELO A DTO
        // ========================================

        private static ClienteDto MapearDto(
            Cliente cliente)
        {
            return new ClienteDto
            {
                Id = cliente.Id,

                Codigo = cliente.Codigo,
                Nombre = cliente.Nombre,

                DocumentoIdentidad =
                    cliente.DocumentoIdentidad,

                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Direccion = cliente.Direccion,

                Activo = cliente.Activo
            };
        }
    }
}
