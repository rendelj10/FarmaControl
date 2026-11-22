
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Service
{
    public interface ICompraService
    {
        // ========================================
        // CONSULTAS
        // ========================================

        Task<List<CompraDto>> ObtenerTodasAsync();

        Task<CompraDto?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<CompraDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // OPERACIONES DE NEGOCIO
        // ========================================

        Task<CompraDto> CrearBorradorAsync(
            CompraGuardarDto dto);

        Task<CompraDto> RecibirAsync(int id, int usuarioId);

        Task<CompraDto> AnularAsync(int id);
    }
}
