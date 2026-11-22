using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Service
{
    public interface IReporteService
    {
        Task<ReporteResultadoDto> ConsultarAsync(string tipo, DateTime? desde, DateTime? hasta);
    }
}
