
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface IAlmacenService
    {
        // ========================================
        // CONSULTAR TODOS LOS ALMACENES
        // ========================================

        Task<List<Almacen>> ObtenerTodosAsync();

        // ========================================
        // CONSULTAR ALMACEN POR ID
        // ========================================

        Task<Almacen?> ObtenerPorIdAsync(int id);

        // ========================================
        // PAGINACION PARA DATATABLES
        // ========================================

        Task<DataTablesResponseDto<AlmacenDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // OPERACIONES
        // ========================================

        Task CrearAsync(Almacen almacen);

        Task ActualizarAsync(Almacen almacen);

        Task DesactivarAsync(int id);
    }
}
