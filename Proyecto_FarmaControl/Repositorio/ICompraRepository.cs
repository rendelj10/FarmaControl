
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface ICompraRepository
    {
        // ========================================
        // CONSULTAS
        // ========================================

        Task<List<Compra>> ObtenerTodasAsync();

        Task<Compra?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<CompraDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // CONSULTA CON BLOQUEO PARA OPERACIONES
        // DE RECEPCION Y ANULACION
        // ========================================

        Task<Compra?> ObtenerParaProcesarAsync(int id);

        // ========================================
        // OPERACIONES
        // ========================================

        Task AgregarAsync(Compra compra);

        Task ActualizarAsync(Compra compra);

        Task GuardarCambiosAsync();

        // ========================================
        // VALIDACIONES
        // ========================================

        Task<bool> ExisteDocumentoAsync(
            int proveedorId,
            string numeroDocumento,
            int? excluirId = null);

        Task<bool> ProveedorDisponibleAsync(
            int proveedorId);

        Task<bool> AlmacenDisponibleAsync(
            int almacenId);

        Task<List<int>> ObtenerMedicamentosActivosAsync(
            IEnumerable<int> medicamentosIds);

        // ========================================
        // LOTES
        // ========================================

        Task<Lote?> ObtenerLoteAsync(
            int medicamentoId,
            int almacenId,
            string numeroLote);

        Task AgregarLoteAsync(Lote lote);

        Task AgregarMovimientoAsync(
            MovimientoInventario movimiento);

        // ========================================
        // TRANSACCIONES
        // ========================================

        Task EjecutarEnTransaccionAsync(
            Func<Task> operacion);
    }
}
