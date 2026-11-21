using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface ITurnoCajaRepository
    {
        Task<List<TurnoCajaDto>> ObtenerTodosAsync();
        Task<TurnoCajaDto?> ObtenerPorIdAsync(int id);
        Task<TurnoCajaDto?> ObtenerAbiertoPorUsuarioAsync(int usuarioId);
        Task<DataTablesResponseDto<TurnoCajaDto>> ObtenerPaginadasAsync(
            DataTablesRequestDto request);

        Task<bool> SucursalActivaAsync(int sucursalId);
        Task<bool> UsuarioExisteAsync(int usuarioId);
        Task<bool> ExisteTurnoAbiertoAsync(int usuarioId);

        Task<TurnoCaja?> ObtenerParaCerrarAsync(int id);
        Task<(decimal Ingresos, decimal Egresos)> ObtenerTotalesEfectivoAsync(int turnoId);

        Task AgregarAsync(TurnoCaja turno);
        Task EjecutarEnTransaccionAsync(Func<Task> operacion);
    }
}
