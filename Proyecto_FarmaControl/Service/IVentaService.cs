
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Service
{
    public interface IVentaService
    {
        Task<List<VentaDto>> ObtenerTodasAsync();

        Task<VentaDto?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<VentaDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request);

        Task<VentaDto> CrearBorradorAsync(
            VentaGuardarDto dto);

        Task<VentaDto> ConfirmarAsync(int id, int usuarioId);

        Task<VentaDto> AnularAsync(int id);
    }
}
