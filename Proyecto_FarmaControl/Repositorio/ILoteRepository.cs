
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface ILoteRepository
    {
        // Consultas
        Task<List<Lote>> ObtenerTodasAsync();

        Task<Lote?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<LoteDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request);

        // Operaciones
        Task AgregarAsync(Lote lote);

        Task ActualizarAsync(Lote lote);

        // Validaciones
        Task<bool> ExisteLoteAsync(
            int medicamentoId,
            string numeroLote,
            int almacenId,
            int? excluirId = null);

        Task<bool> AlmacenDisponibleAsync(int almacenId);
    }
}
