
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Service
{
    public interface IClienteService
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

        Task CrearAsync(Cliente cliente);

        Task ActualizarAsync(Cliente cliente);

        Task DesactivarAsync(int id);
    }
}
