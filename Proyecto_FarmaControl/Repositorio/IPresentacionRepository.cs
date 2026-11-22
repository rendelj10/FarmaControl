using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IPresentacionRepository
    {
        Task<List<Presentacion>> ObtenerTodasAsync();

        Task<Presentacion?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<PresentacionDto>> ObtenerPaginadasAsync(
            DataTablesRequestDto request);

        Task AgregarAsync(Presentacion presentacion);

        Task ActualizarAsync(Presentacion presentacion);

        Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null);
    }
}
