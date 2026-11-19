
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IClienteRepository
    {
        // ========================================
        // CONSULTAS
        // ========================================

        Task<List<Cliente>> ObtenerTodosAsync();

        Task<Cliente?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<ClienteDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // OPERACIONES
        // ========================================

        Task AgregarAsync(Cliente cliente);

        Task ActualizarAsync(Cliente cliente);

        // ========================================
        // VALIDACIONES
        // ========================================

        Task<bool> ExisteCodigoAsync(
            string codigo,
            int? excluirId = null);

        // VERIFICAR VENTAS PENDIENTES
        Task<bool> TieneVentasPendientesAsync(int clienteId);
    }
}
