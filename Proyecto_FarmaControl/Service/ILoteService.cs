
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Service
{
    public interface ILoteService
    {
        // ========================================
        // CONSULTAR TODOS LOS LOTES
        // ========================================

        Task<List<Lote>> ObtenerTodasAsync();

        // ========================================
        // CONSULTAR LOTE POR ID
        // ========================================

        Task<Lote?> ObtenerPorIdAsync(int id);

        // ========================================
        // CONSULTAR DATATABLES CON PAGINACION
        // ========================================

        Task<DataTablesResponseDto<LoteDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // CREAR LOTE
        // ========================================

        Task CrearAsync(Lote lote);

        // ========================================
        // ACTUALIZAR LOTE
        // ========================================

        Task ActualizarAsync(Lote lote);

        // ========================================
        // DESACTIVAR LOTE
        // ========================================

        Task DesactivarAsync(int id);
    }
}
