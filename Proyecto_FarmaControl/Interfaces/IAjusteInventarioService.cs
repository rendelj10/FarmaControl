
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface IAjusteInventarioService
    {
        // ========================================
        // CONSULTAS
        // ========================================

        Task<List<AjusteInventarioDto>> ObtenerTodosAsync();

        Task<AjusteInventarioDto?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<AjusteInventarioDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // OPERACIONES
        // ========================================

        Task<AjusteInventarioDto> CrearBorradorAsync(
            AjusteInventarioGuardarDto dto);

        Task<AjusteInventarioDto> AplicarAsync(int id);

        Task<AjusteInventarioDto> AnularAsync(int id);
    }
}
