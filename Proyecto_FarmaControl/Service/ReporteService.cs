using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Service
{
    public class ReporteService : IReporteService
    {
        private const int Limite = 500;
        private readonly IReporteRepository _repository;
        public ReporteService(IReporteRepository repository) => _repository = repository;

        public Task<ReporteResultadoDto> ConsultarAsync(string tipo, DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                throw new ArgumentException("La fecha inicial no puede ser posterior a la final.");
            if (hasta.HasValue && hasta.Value.Date == DateTime.MaxValue.Date)
                throw new ArgumentException("La fecha final no es válida.");

            return tipo.ToLowerInvariant() switch
            {
                "ventas" => _repository.VentasAsync(desde, hasta, Limite),
                "compras" => _repository.ComprasAsync(desde, hasta, Limite),
                "pagos" => _repository.PagosAsync(desde, hasta, Limite),
                "lotes" => _repository.LotesAsync(Limite),
                "movimientos" => _repository.MovimientosAsync(desde, hasta, Limite),
                _ => throw new ArgumentException("El tipo de reporte no existe.")
            };
        }
    }
}
