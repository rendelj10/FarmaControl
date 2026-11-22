
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;

namespace Proyecto_FarmaControl.Controllers
{
    public class VentaController : Controller
    {
        private readonly IVentaService _service;

        public VentaController(IVentaService service)
        {
            _service = service;
        }

        // ========================================
        // 1. VISTA PRINCIPAL
        // GET: /Venta/Index
        // ========================================

        [Authorize(Policy = "venta.ver")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ========================================
        // 2. DATATABLES
        // POST: /Venta/GetData
        // ========================================

        [Authorize(Policy = "venta.ver")]
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
        // 3. CONSULTAR TODAS LAS VENTAS
        // GET: /Venta/GetAll
        // ========================================

        [Authorize(Policy = "venta.ver")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ventas = await _service.ObtenerTodasAsync();

            return Ok(ventas);
        }

        // ========================================
        // 4. CONSULTAR VENTA POR ID
        // GET: /Venta/GetById?id=1
        // ========================================

        [Authorize(Policy = "venta.ver")]
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

            var venta = await _service.ObtenerPorIdAsync(id);

            if (venta == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "La venta no existe."
                });
            }

            return Ok(venta);
        }

        // ========================================
        // 5. CREAR VENTA EN BORRADOR
        // POST: /Venta/Create
        // ========================================

        [Authorize(Policy = "venta.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] VentaGuardarDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos de la venta no son válidos."
                });
            }

            try
            {
                var venta = await _service
                    .CrearBorradorAsync(dto);

                return Ok(new
                {
                    success = true,

                    id = venta.Id,

                    numeroDocumento =
                        venta.NumeroDocumento,

                    estado = venta.Estado,

                    subtotal = venta.Subtotal,

                    impuesto = venta.Impuesto,

                    total = venta.Total,

                    message =
                        "Venta registrada en borrador correctamente."
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
        // 6. CONFIRMAR VENTA
        // POST: /Venta/Confirmar?id=1
        // ========================================

        [Authorize(Policy = "venta.confirmar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El identificador no es válido."
                });
            }

            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int usuarioId) || usuarioId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "No se pudo identificar al usuario autenticado."
                });
            }

            try
            {
                var venta = await _service.ConfirmarAsync(id, usuarioId);

                return Ok(new
                {
                    success = true,

                    id = venta.Id,

                    estado = venta.Estado,

                    fechaConfirmacion =
                        venta.FechaConfirmacion,

                    total = venta.Total,

                    message =
                        "Venta confirmada correctamente. " +
                        "Las existencias fueron descontadas."
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
        // 7. ANULAR VENTA EN BORRADOR
        // POST: /Venta/Anular?id=1
        // ========================================

        [Authorize(Policy = "venta.anular")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id)
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
                var venta = await _service.AnularAsync(id);

                return Ok(new
                {
                    success = true,

                    id = venta.Id,

                    estado = venta.Estado,

                    message =
                        "Venta en borrador anulada correctamente."
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
    }
}
