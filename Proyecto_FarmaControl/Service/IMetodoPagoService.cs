using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Service
{
    public interface IMetodoPagoService
    {
        Task<List<MetodoPago>> ObtenerTodosAsync();

        Task<MetodoPago?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<MetodoPagoDto>> ObtenerPaginadosAsync(
            DataTablesRequestDto request);

        Task CrearAsync(MetodoPago metodoPago);

        Task ActualizarAsync(MetodoPago metodoPago);

        Task DesactivarAsync(int id);
    }
}
