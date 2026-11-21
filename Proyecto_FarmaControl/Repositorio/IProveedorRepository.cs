
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IProveedorRepository
    {
        // ========================================
        // CONSULTAS
        // ========================================

        Task<List<Proveedor>> ObtenerTodosAsync();

        Task<Proveedor?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<ProveedorDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // OPERACIONES
        // ========================================

        Task AgregarAsync(Proveedor proveedor);

        Task ActualizarAsync(Proveedor proveedor);

        // ========================================
        // VALIDACIONES
        // ========================================

        Task<bool> ExisteCodigoAsync(
            string codigo,
            int? excluirId = null);

        // NUEVO:
        // Verificar compras pendientes
        // antes de desactivar al proveedor.

        Task<bool> TieneComprasPendientesAsync(
            int proveedorId);
    }
}
