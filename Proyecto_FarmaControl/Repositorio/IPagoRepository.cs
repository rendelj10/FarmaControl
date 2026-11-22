using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IPagoRepository
    {
        Task<List<PagoDto>> ObtenerTodosAsync();
        Task<PagoDto?> ObtenerPorIdAsync(int id);
        Task<DataTablesResponseDto<PagoDto>> ObtenerPaginadasAsync(DataTablesRequestDto request);

        Task<Compra?> BloquearCompraAsync(int id);
        Task<Venta?> BloquearVentaAsync(int id);
        Task<TurnoCaja?> BloquearTurnoAsync(int id);
        Task<Pago?> BloquearPagoAsync(int id);
        Task<Pago?> ObtenerPagoParaAnularAsync(int id);
        Task<MetodoPago?> ObtenerMetodoAsync(int id);
        Task<int?> ObtenerSucursalAlmacenAsync(int almacenId);
        Task<decimal> TotalPagadoCompraAsync(int compraId);
        Task<decimal> TotalPagadoVentaAsync(int ventaId);
        Task AgregarAsync(Pago pago);
        Task EjecutarEnTransaccionAsync(Func<Task> operacion);
    }
}
