
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> ObtenerTodasAsync();

        Task<Categoria?> ObtenerPorIdAsync(int id);

        Task CrearAsync(Categoria categoria);

        Task ActualizarAsync(Categoria categoria);

        Task DesactivarAsync(int id);

        Task<DataTablesResponseDto<CategoriaTablaDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request);
    }
}
