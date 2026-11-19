
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
        // OPERACIONES DE COMPRAS
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
        // LOTES PARA RECEPCION DE COMPRAS
        // ========================================

        Task<Lote?> ObtenerLoteAsync(
            int medicamentoId,
            int almacenId,
            string numeroLote);

        Task AgregarLoteAsync(Lote lote);

        // ========================================
        // TRANSACCIONES SQL SERVER
        // ========================================

        Task EjecutarEnTransaccionAsync(
            Func<Task> operacion);
    }
}
