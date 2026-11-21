using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface ILaboratorioRepository
    {
        Task<List<Laboratorio>> ObtenerTodosAsync();

        Task<Laboratorio?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<LaboratorioDto>> ObtenerPaginadosAsync(
            DataTablesRequestDto request);

        Task AgregarAsync(Laboratorio laboratorio);

        Task ActualizarAsync(Laboratorio laboratorio);
    }
}
