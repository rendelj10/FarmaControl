
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Service
{
    public interface IAjusteInventarioService
    {
        // ========================================
        // 1. CONSULTAR TODOS LOS AJUSTES
        // ========================================

        Task<List<AjusteInventarioDto>>
            ObtenerTodosAsync();

        // ========================================
        // 2. CONSULTAR AJUSTE POR ID
        // ========================================

        Task<AjusteInventarioDto?>
            ObtenerPorIdAsync(int id);

        // ========================================
        // 3. CONSULTAR AJUSTES PAGINADOS
        // ========================================

        Task<DataTablesResponseDto<AjusteInventarioDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // 4. CREAR AJUSTE EN BORRADOR
        // ========================================

        Task<AjusteInventarioDto> CrearBorradorAsync(
            AjusteInventarioGuardarDto dto,
            int usuarioId);

        // ========================================
        // 5. CONFIRMAR AJUSTE DE INVENTARIO
        // ========================================

        Task<AjusteInventarioDto> ConfirmarAsync(
            int id,
            int usuarioId);

        // ========================================
        // 6. ANULAR AJUSTE EN BORRADOR
        // ========================================

        Task<AjusteInventarioDto> AnularAsync(int id);
    }
}
