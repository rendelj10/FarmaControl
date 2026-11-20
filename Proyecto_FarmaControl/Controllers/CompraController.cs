
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;

namespace Proyecto_FarmaControl.Controllers
{
    public class CompraController : Controller
    {
        private readonly ICompraService _service;

        public CompraController(
            ICompraService service)
        {
            _service = service;
        }

        // ========================================
        // 1. VISTA PRINCIPAL
        // GET: /Compra/Index
        // ========================================

        [Authorize(Policy = "compra.ver")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ========================================
        // 2. DATATABLES - PAGINACION SQL
        // POST: /Compra/GetData
        // ========================================

        [Authorize(Policy = "compra.ver")]
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
        // 3. CONSULTAR COMPRA POR ID
        // GET: /Compra/GetById?id=1
        // ========================================

        [Authorize(Policy = "compra.ver")]
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

            var compra = await _service
                .ObtenerPorIdAsync(id);

            if (compra == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "La compra no existe."
                });
            }

            return Ok(compra);
        }

        // ========================================
        // 4. REGISTRAR COMPRA EN BORRADOR
        // POST: /Compra/Create
        // ========================================

        [Authorize(Policy = "compra.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] CompraGuardarDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos de la compra no son válidos."
                });
            }

            try
            {
                var compra = await _service
                    .CrearBorradorAsync(dto);

                return Ok(new
                {
                    success = true,
                    id = compra.Id,
                    estado = compra.Estado,
                    subtotal = compra.Subtotal,
                    impuesto = compra.Impuesto,
                    total = compra.Total,
                    message = "Compra registrada en borrador correctamente."
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
            catch (OverflowException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El importe de la compra supera el límite permitido."
                });
            }
        }

        // ========================================
        // 5. CONFIRMAR RECEPCION DE COMPRA
        // POST: /Compra/Recibir?id=1
        // ========================================

        [Authorize(Policy = "compra.recibir")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recibir(int id)
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
                var compra = await _service
                    .RecibirAsync(id, usuarioId);

                return Ok(new
                {
                    success = true,
                    id = compra.Id,
                    estado = compra.Estado,
                    fechaRecepcion = compra.FechaRecepcion,
                    total = compra.Total,
                    message = "Compra recibida e inventario actualizado correctamente."
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
            catch (OverflowException)
            {
                return Conflict(new
                {
                    success = false,
                    message = "No se puede recibir la compra porque las existencias superarían el límite permitido."
                });
            }
        }

        // ========================================
        // 6. ANULAR COMPRA EN BORRADOR
        // POST: /Compra/Anular?id=1
        // ========================================

        [Authorize(Policy = "compra.anular")]
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
                var compra = await _service
                    .AnularAsync(id);

                return Ok(new
                {
                    success = true,
                    id = compra.Id,
                    estado = compra.Estado,
                    message = "Compra anulada correctamente."
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
