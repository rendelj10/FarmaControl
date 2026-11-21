using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_FarmaControl.Service;

namespace Proyecto_FarmaControl.Controllers
{
    [Authorize(Policy = "lote.ver")]
    public class MovimientoController : Controller
    {
        private readonly IReporteService _service;
        public MovimientoController(IReporteService service) => _service = service;

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> Consultar(DateTime? desde, DateTime? hasta)
        {
            try { return Ok(await _service.ConsultarAsync("movimientos", desde, hasta)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
