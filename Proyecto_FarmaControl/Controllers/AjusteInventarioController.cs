
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;

namespace Proyecto_FarmaControl.Controllers
{
    public class AjusteInventarioController : Controller
    {
        private readonly IAjusteInventarioService _service;

        public AjusteInventarioController(
            IAjusteInventarioService service)
        {
            _service = service;
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
        // 2. DATATABLES - PAGINACION SQL
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
        // 3. CONSULTAR TODOS LOS AJUSTES
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
        // 4. CONSULTAR AJUSTE POR ID
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
        // 5. CREAR AJUSTE EN BORRADOR
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
                    message =
                        "Los datos del ajuste no son válidos."
                });
            }

            try
            {
                var ajuste = await _service
                    .CrearBorradorAsync(dto);

                return Ok(new
                {
                    success = true,

                    id = ajuste.Id,

                    numeroDocumento =
                        ajuste.NumeroDocumento,

                    estado = ajuste.Estado,

                    motivo = ajuste.Motivo,

                    message =
                        "Ajuste registrado en borrador " +
                        "correctamente. No se modificaron " +
                        "las existencias."
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
        // 6. APLICAR AJUSTE AL INVENTARIO
        // POST: /AjusteInventario/Aplicar?id=1
        // ========================================

        [Authorize(Policy = "ajuste.aplicar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aplicar(int id)
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
                // El servicio verifica:
                //
                // - Estado Borrador.
                // - Almacen activo.
                // - Medicamentos activos.
                // - Lotes validos.
                // - Cantidades disponibles.
                // - Entradas y salidas.
                //
                // La modificacion del inventario
                // ocurre dentro de una transaccion
                // SQL Server.

                var ajuste = await _service
                    .AplicarAsync(id);

                return Ok(new
                {
                    success = true,

                    id = ajuste.Id,

                    numeroDocumento =
                        ajuste.NumeroDocumento,

                    estado = ajuste.Estado,

                    fechaAplicacion =
                        ajuste.FechaAplicacion,

                    detalles = ajuste.Detalles,

                    message =
                        "Ajuste aplicado correctamente. " +
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
        // 7. ANULAR AJUSTE EN BORRADOR
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
                // Solo pueden anularse ajustes
                // que permanezcan en Borrador.
                //
                // Un ajuste en Borrador no ha
                // modificado el inventario.

                var ajuste = await _service
                    .AnularAsync(id);

                return Ok(new
                {
                    success = true,

                    id = ajuste.Id,

                    numeroDocumento =
                        ajuste.NumeroDocumento,

                    estado = ajuste.Estado,

                    message =
                        "Ajuste en borrador anulado " +
                        "correctamente. No se modificaron " +
                        "las existencias."
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
