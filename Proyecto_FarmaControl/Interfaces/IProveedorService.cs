
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface IProveedorService
    {
        // ========================================
        // CONSULTAR TODOS LOS PROVEEDORES
        // ========================================

        Task<List<Proveedor>> ObtenerTodosAsync();

        // ========================================
        // CONSULTAR PROVEEDOR POR ID
        // ========================================

        Task<Proveedor?> ObtenerPorIdAsync(int id);

        // ========================================
        // PAGINACION PARA DATATABLES
        // ========================================

        Task<DataTablesResponseDto<ProveedorDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // OPERACIONES
        // ========================================

        Task CrearAsync(Proveedor proveedor);

        Task ActualizarAsync(Proveedor proveedor);

        Task DesactivarAsync(int id);
    }
}
