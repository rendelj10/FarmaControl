using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Service
{
    public interface IPagoService
    {
        Task<List<PagoDto>> ObtenerTodosAsync();
        Task<PagoDto?> ObtenerPorIdAsync(int id);
        Task<DataTablesResponseDto<PagoDto>> ObtenerPaginadasAsync(DataTablesRequestDto request);
        Task<PagoDto> RegistrarAsync(PagoGuardarDto dto, int usuarioId);
        Task<PagoDto> AnularAsync(int id);
    }
}
