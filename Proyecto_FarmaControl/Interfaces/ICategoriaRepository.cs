
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<List<Categoria>> ObtenerTodasAsync();

        Task<Categoria?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Categoria categoria);

        Task ActualizarAsync(Categoria categoria);

        Task<bool> ExisteNombreAsync(
            string nombre, int? excluirId = null);

        Task<DataTablesResponseDto<CategoriaTablaDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request);
    }
}
