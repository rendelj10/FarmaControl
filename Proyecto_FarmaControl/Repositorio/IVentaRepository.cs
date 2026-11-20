
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IVentaRepository
    {
        Task<List<Venta>> ObtenerTodasAsync();

        Task<Venta?> ObtenerPorIdAsync(int id);

        Task<Venta?> ObtenerParaProcesarAsync(int id);

        Task<DataTablesResponseDto<VentaDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request);

        Task AgregarAsync(Venta venta);

        Task<bool> ExisteDocumentoAsync(
            string numeroDocumento);

        Task<bool> ClienteDisponibleAsync(int clienteId);

        Task<bool> AlmacenDisponibleAsync(int almacenId);

        Task<List<int>> ObtenerMedicamentosActivosAsync(
            IEnumerable<int> ids);

        Task<Lote?> ObtenerLoteParaConsultaAsync(int loteId);

        Task<Lote?> ObtenerLoteParaDescontarAsync(int loteId);

        Task AgregarMovimientoAsync(
            MovimientoInventario movimiento);

        Task EjecutarEnTransaccionAsync(
            Func<Task> operacion);
    }
}
