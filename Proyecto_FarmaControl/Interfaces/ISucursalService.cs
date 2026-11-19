
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface ISucursalService
    {
        // ========================================
        // CONSULTAR TODAS LAS SUCURSALES
        // ========================================

        Task<List<Sucursal>> ObtenerTodasAsync();

        // ========================================
        // CONSULTAR SUCURSAL POR ID
        // ========================================

        Task<Sucursal?> ObtenerPorIdAsync(int id);

        // ========================================
        // CONSULTA PAGINADA PARA DATATABLES
        // ========================================

        Task<DataTablesResponseDto<SucursalDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // OPERACIONES
        // ========================================

        Task CrearAsync(Sucursal sucursal);

        Task ActualizarAsync(Sucursal sucursal);

        Task DesactivarAsync(int id);
    }
}
