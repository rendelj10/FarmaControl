
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IAjusteInventarioRepository
    {
        // ========================================
        // 1. CONSULTAR TODOS LOS AJUSTES
        // ========================================

        Task<List<AjusteInventario>> ObtenerTodosAsync();

        // ========================================
        // 2. CONSULTAR AJUSTE POR ID
        // ========================================

        Task<AjusteInventario?> ObtenerPorIdAsync(int id);

        // ========================================
        // 3. CONSULTAR AJUSTES PAGINADOS
        // ========================================

        Task<DataTablesResponseDto<AjusteInventarioDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request);

        // ========================================
        // 4. OBTENER AJUSTE PARA PROCESAR
        // ========================================

        Task<AjusteInventario?> ObtenerParaProcesarAsync(
            int id);

        // ========================================
        // 5. REGISTRAR AJUSTE EN BORRADOR
        // ========================================

        Task AgregarAsync(AjusteInventario ajuste);

        // ========================================
        // 6. REGISTRAR MOVIMIENTO DE INVENTARIO
        // ========================================

        Task AgregarMovimientoAsync(
            MovimientoInventario movimiento);

        // ========================================
        // 7. VALIDAR ALMACEN DISPONIBLE
        // ========================================

        Task<bool> AlmacenDisponibleAsync(
            int almacenId);

        // ========================================
        // 8. CONSULTAR LOTE
        // ========================================

        Task<Lote?> ObtenerLoteParaConsultaAsync(
            int loteId);

        // ========================================
        // 9. OBTENER LOTE PARA AJUSTAR
        // ========================================

        Task<Lote?> ObtenerLoteParaAjustarAsync(
            int loteId);

        // ========================================
        // 10. EJECUTAR TRANSACCION
        // ========================================

        Task EjecutarEnTransaccionAsync(
            Func<Task> operacion);
    }
}
