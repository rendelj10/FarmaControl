using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Service
{
    public interface ILaboratorioService
    {
        Task<List<Laboratorio>> ObtenerTodosAsync();
        Task<Laboratorio?> ObtenerPorIdAsync(int id);
        Task<DataTablesResponseDto<LaboratorioDto>> ObtenerPaginadosAsync(DataTablesRequestDto request);
        Task CrearAsync(Laboratorio laboratorio);
        Task ActualizarAsync(Laboratorio laboratorio);
        Task DesactivarAsync(int id);
    }
}
