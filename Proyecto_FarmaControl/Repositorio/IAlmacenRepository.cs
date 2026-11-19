
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IAlmacenRepository
    {
        // ========================================
        // CONSULTAS
        // ========================================

        Task<List<Almacen>> ObtenerTodosAsync();

        Task<Almacen?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<AlmacenDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // OPERACIONES
        // ========================================

        Task AgregarAsync(Almacen almacen);

        Task ActualizarAsync(Almacen almacen);

        // ========================================
        // VALIDACIONES
        // ========================================

        Task<bool> ExisteCodigoAsync(
            int sucursalId,
            string codigo,
            int? excluirId = null);

        Task<bool> SucursalDisponibleAsync(int sucursalId);

        Task<bool> TieneLotesAsync(int almacenId);
    }
}
