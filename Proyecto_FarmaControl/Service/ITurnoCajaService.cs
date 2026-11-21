using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Service
{
    public interface ITurnoCajaService
    {
        Task<List<TurnoCajaDto>> ObtenerTodosAsync();
        Task<TurnoCajaDto?> ObtenerPorIdAsync(int id);
        Task<TurnoCajaDto?> ObtenerAbiertoPorUsuarioAsync(int usuarioId);
        Task<DataTablesResponseDto<TurnoCajaDto>> ObtenerPaginadasAsync(
            DataTablesRequestDto request);
        Task<TurnoCajaDto> AbrirAsync(TurnoCajaAbrirDto dto, int usuarioId);
        Task<TurnoCajaDto> CerrarAsync(int id, TurnoCajaCerrarDto dto, int usuarioId);
    }
}
