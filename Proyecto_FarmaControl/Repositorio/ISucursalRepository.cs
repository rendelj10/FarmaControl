
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface ISucursalRepository
    {
        // ========================================
        // CONSULTAS
        // ========================================

        Task<List<Sucursal>> ObtenerTodasAsync();

        Task<Sucursal?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<SucursalDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // OPERACIONES
        // ========================================

        Task AgregarAsync(Sucursal sucursal);

        Task ActualizarAsync(Sucursal sucursal);

        // ========================================
        // VALIDACIONES
        // ========================================

        Task<bool> ExisteCodigoAsync(
            string codigo,
            int? excluirId = null);

        Task<bool> TieneAlmacenesAsync(int sucursalId);
    }
}
