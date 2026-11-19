
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface ICategoriaRepository
    {
        Task<IReadOnlyList<Categoria>> ObtenerTodasAsync();

        Task<Categoria?> ObtenerPorIdAsync(int id);

        Task<Categoria> AgregarAsync(Categoria categoria);

        Task<bool> ActualizarAsync(Categoria categoria);

        Task<bool> ExisteNombreAsync(
            string nombre,
            int? excluirId = null);

        Task<DataTablesResponseDto<CategoriaTablaDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request);
    }
}
