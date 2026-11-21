using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IReporteRepository
    {
        Task<ReporteResultadoDto> VentasAsync(DateTime? desde, DateTime? hasta, int limite);
        Task<ReporteResultadoDto> ComprasAsync(DateTime? desde, DateTime? hasta, int limite);
        Task<ReporteResultadoDto> PagosAsync(DateTime? desde, DateTime? hasta, int limite);
        Task<ReporteResultadoDto> LotesAsync(int limite);
        Task<ReporteResultadoDto> MovimientosAsync(DateTime? desde, DateTime? hasta, int limite);
    }
}
