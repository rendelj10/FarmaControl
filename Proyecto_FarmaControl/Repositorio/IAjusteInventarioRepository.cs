
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IAjusteInventarioRepository
    {
        // ========================================
        // CONSULTAS
        // ========================================

        Task<List<AjusteInventario>> ObtenerTodosAsync();

        Task<AjusteInventario?> ObtenerPorIdAsync(int id);

        Task<DataTablesResponseDto<AjusteInventarioDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // CONSULTA CON BLOQUEO
        // ========================================

        Task<AjusteInventario?> ObtenerParaProcesarAsync(
            int id);

        // ========================================
        // REGISTRAR AJUSTE
        // ========================================

        Task AgregarAsync(AjusteInventario ajuste);

        // ========================================
        // VALIDACIONES
        // ========================================

        Task<bool> ExisteDocumentoAsync(
            string numeroDocumento);

        Task<bool> AlmacenDisponibleAsync(
            int almacenId);

        Task<List<int>> ObtenerMedicamentosActivosAsync(
            IEnumerable<int> ids);

        // ========================================
        // LOTES
        // ========================================

        Task<Lote?> ObtenerLoteParaConsultaAsync(
            int loteId);

        Task<Lote?> ObtenerLoteParaAjustarAsync(
            int loteId);

        // ========================================
        // TRANSACCIONES
        // ========================================

        Task EjecutarEnTransaccionAsync(
            Func<Task> operacion);
    }
}
