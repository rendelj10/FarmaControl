using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.Service;

namespace Proyecto_FarmaControl.Controllers
{
    // Las acciones de consulta reutilizan permisos de lectura existentes.
    // No se requieren tablas, migraciones ni permisos nuevos.
    [Authorize]
    public class ReporteController : Controller
    {
        private readonly IReporteService _service;
        public ReporteController(IReporteService service) => _service = service;

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        [Authorize(Policy = "venta.ver")]
        public Task<IActionResult> Ventas(DateTime? desde, DateTime? hasta) => Consultar("ventas", desde, hasta);

        [HttpGet]
        [Authorize(Policy = "compra.ver")]
        public Task<IActionResult> Compras(DateTime? desde, DateTime? hasta) => Consultar("compras", desde, hasta);

        [HttpGet]
        [Authorize(Policy = "pago.ver")]
        public Task<IActionResult> Pagos(DateTime? desde, DateTime? hasta) => Consultar("pagos", desde, hasta);

        [HttpGet]
        [Authorize(Policy = "lote.ver")]
        public Task<IActionResult> Lotes() => Consultar("lotes", null, null);

        private async Task<IActionResult> Consultar(string tipo, DateTime? desde, DateTime? hasta)
        {
            try { return Ok(await _service.ConsultarAsync(tipo, desde, hasta)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
