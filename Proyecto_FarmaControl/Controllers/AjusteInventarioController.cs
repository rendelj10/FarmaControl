
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;

namespace Proyecto_FarmaControl.Controllers
{
    public class AjusteInventarioController : Controller
    {
        private readonly IAjusteInventarioService _service;
        private readonly IAntiforgery _antiforgery;

        public AjusteInventarioController(
            IAjusteInventarioService service,
            IAntiforgery antiforgery)
        {
            _service = service;
            _antiforgery = antiforgery;
        }

        // ========================================
        // 1. VISTA PRINCIPAL
        // GET: /AjusteInventario/Index
        // ========================================

        [Authorize(Policy = "ajuste.ver")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ========================================
        // 2. OBTENER TOKEN ANTIFALSIFICACION
        // GET: /AjusteInventario/ObtenerToken
        // ========================================

        [Authorize]
        [HttpGet]
        [ResponseCache(
            NoStore = true,
            Location = ResponseCacheLocation.None)]
        public IActionResult ObtenerToken()
        {
            var tokens =
                _antiforgery.GetAndStoreTokens(HttpContext);

            return Ok(new
            {
                success = true,
                requestToken = tokens.RequestToken
            });
        }

        // ========================================
        // 3. DATATABLES
        // POST: /AjusteInventario/GetData
        // ========================================

        [Authorize(Policy = "ajuste.ver")]
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
        // 4. CONSULTAR TODOS LOS AJUSTES
        // GET: /AjusteInventario/GetAll
        // ========================================

        [Authorize(Policy = "ajuste.ver")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ajustes = await _service
                .ObtenerTodosAsync();

            return Ok(ajustes);
        }

        // ========================================
        // 5. CONSULTAR AJUSTE POR ID
        // GET: /AjusteInventario/GetById?id=1
        // ========================================

        [Authorize(Policy = "ajuste.ver")]
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

            var ajuste = await _service
                .ObtenerPorIdAsync(id);

            if (ajuste == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "El ajuste no existe."
                });
            }

            return Ok(ajuste);
        }

        // ========================================
        // 6. REGISTRAR AJUSTE EN BORRADOR
        // POST: /AjusteInventario/Create
        // ========================================

        [Authorize(Policy = "ajuste.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromBody] AjusteInventarioGuardarDto? dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Los datos del ajuste no son válidos."
                });
            }

            if (!ObtenerUsuarioId(out int usuarioId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "No se pudo identificar al usuario."
                });
            }

            try
            {
                var ajuste = await _service
                    .CrearBorradorAsync(dto, usuarioId);

                return Ok(new
                {
                    success = true,
                    id = ajuste.Id,
                    almacenId = ajuste.AlmacenId,
                    tipoAjuste = ajuste.TipoAjuste,
                    estado = ajuste.Estado,
                    motivo = ajuste.Motivo,
                    fechaAjuste = ajuste.FechaAjuste,
                    detalles = ajuste.Detalles,
                    message =
                        "Ajuste registrado en borrador. " +
                        "Las existencias no fueron modificadas."
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
        // 7. CONFIRMAR AJUSTE DE INVENTARIO
        // POST: /AjusteInventario/Confirmar?id=1
        // ========================================

        [Authorize(Policy = "ajuste.aplicar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Confirmar(int id)
        {
            return ConfirmarAjusteInterno(id);
        }

        // ========================================
        // 8. COMPATIBILIDAD CON ACCION ANTERIOR
        // POST: /AjusteInventario/Aplicar?id=1
        // ========================================

        [Authorize(Policy = "ajuste.aplicar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Aplicar(int id)
        {
            return ConfirmarAjusteInterno(id);
        }

        // ========================================
        // 9. ANULAR AJUSTE EN BORRADOR
        // POST: /AjusteInventario/Anular?id=1
        // ========================================

        [Authorize(Policy = "ajuste.anular")]
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
                var ajuste = await _service
                    .AnularAsync(id);

                return Ok(new
                {
                    success = true,
                    id = ajuste.Id,
                    estado = ajuste.Estado,
                    message =
                        "Ajuste anulado correctamente. " +
                        "Las existencias no fueron modificadas."
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
        // 10. LOGICA INTERNA DE CONFIRMACION
        // ========================================

        [NonAction]
        private async Task<IActionResult>
            ConfirmarAjusteInterno(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El identificador no es válido."
                });
            }

            if (!ObtenerUsuarioId(out int usuarioId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "No se pudo identificar al usuario."
                });
            }

            try
            {
                var ajuste = await _service
                    .ConfirmarAsync(id, usuarioId);

                return Ok(new
                {
                    success = true,
                    id = ajuste.Id,
                    estado = ajuste.Estado,
                    tipoAjuste = ajuste.TipoAjuste,
                    fechaConfirmacion =
                        ajuste.FechaConfirmacion,
                    confirmadoPorUsuarioId =
                        ajuste.ConfirmadoPorUsuarioId,
                    movimientoInventarioId =
                        ajuste.MovimientoInventarioId,
                    detalles = ajuste.Detalles,
                    message =
                        "Ajuste confirmado correctamente. " +
                        "Las existencias fueron actualizadas."
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
        // 11. IDENTIFICAR USUARIO AUTENTICADO
        // ========================================

        [NonAction]
        private bool ObtenerUsuarioId(out int usuarioId)
        {
            string? identificador = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(
                identificador,
                out usuarioId) && usuarioId > 0;
        }
    }
}
