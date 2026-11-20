using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface IPresentacionService
    {
        Task<List<Presentacion>> ObtenerTodasAsync();
        Task<Presentacion?> ObtenerPorIdAsync(int id);
        Task<DataTablesResponseDto<PresentacionDto>> ObtenerPaginadasAsync(DataTablesRequestDto request);
        Task CrearAsync(Presentacion presentacion);
        Task ActualizarAsync(Presentacion presentacion);
        Task DesactivarAsync(int id);
    }
}
