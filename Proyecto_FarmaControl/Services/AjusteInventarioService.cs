
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Services
{
    public class AjusteInventarioService
        : IAjusteInventarioService
    {
        private readonly IAjusteInventarioRepository _repository;

        public AjusteInventarioService(
            IAjusteInventarioRepository repository)
        {
            _repository = repository;
        }

        // ========================================
        // 1. CONSULTAR TODOS
        // ========================================

        public async Task<List<AjusteInventarioDto>>
            ObtenerTodosAsync()
        {
            var ajustes = await _repository.ObtenerTodosAsync();

            return ajustes
                .Select(MapearDto)
                .ToList();
        }

        // ========================================
        // 2. CONSULTAR POR ID
        // ========================================

        public async Task<AjusteInventarioDto?>
            ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var ajuste = await _repository.ObtenerPorIdAsync(id);

            return ajuste == null
                ? null
                : MapearDto(ajuste);
        }

        // ========================================
        // 3. DATATABLES
        // ========================================

        public Task<DataTablesResponseDto<AjusteInventarioDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return _repository.ObtenerPaginadasAsync(request);
        }

        // ========================================
        // 4. CREAR AJUSTE EN BORRADOR
        // ========================================

        public async Task<AjusteInventarioDto>
            CrearBorradorAsync(AjusteInventarioGuardarDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            ValidarAjuste(dto);

            string documento = dto.NumeroDocumento
                .Trim()
                .ToUpperInvariant();

            string motivo = dto.Motivo.Trim();

            // VALIDAR ALMACEN

            if (!await _repository.AlmacenDisponibleAsync(
                dto.AlmacenId))
            {
                throw new ArgumentException(
                    "El almacén no existe o está inactivo.");
            }

            // VALIDAR DOCUMENTO UNICO

            if (await _repository.ExisteDocumentoAsync(documento))
            {
                throw new InvalidOperationException(
                    "Ya existe un ajuste con ese documento.");
            }

            // VALIDAR MEDICAMENTOS

            await ValidarMedicamentosAsync(
                dto.Detalles.Select(d => d.MedicamentoId));

            // NO REPETIR LOTES EN UN MISMO AJUSTE

            var lotesProcesados = new HashSet<int>();

            var detalles = new List<DetalleAjusteInventario>();

            foreach (var detalle in dto.Detalles)
            {
                if (!lotesProcesados.Add(detalle.LoteId))
                {
                    throw new ArgumentException(
                        "Un lote no puede repetirse " +
                        "dentro del mismo ajuste.");
                }

                string tipo = NormalizarTipoMovimiento(
                    detalle.TipoMovimiento);

                // CONSULTAR LOTE

                var lote = await _repository
                    .ObtenerLoteParaConsultaAsync(detalle.LoteId);

                if (lote == null ||
                    !lote.Activo ||
                    lote.AlmacenId != dto.AlmacenId ||
                    lote.MedicamentoId != detalle.MedicamentoId)
                {
                    throw new ArgumentException(
                        "El lote no existe, está inactivo " +
                        "o no corresponde al medicamento " +
                        "y almacén seleccionados.");
                }

                // No incrementar unidades de un
                // lote vencido.
                //
                // Se permite una Salida por pérdidas
                // o daños de un lote vencido.

                if (tipo == "Entrada" &&
                    lote.FechaVencimiento.Date <= DateTime.Today)
                {
                    throw new InvalidOperationException(
                        "No se pueden ingresar existencias " +
                        "a un lote vencido.");
                }

                // Comprobacion preliminar.
                // El borrador NO reserva existencias.
                // Se revisara nuevamente al aplicar.

                if (tipo == "Salida" &&
                    lote.Existencias < detalle.Cantidad)
                {
                    throw new InvalidOperationException(
                        $"Existencias insuficientes " +
                        $"en el lote {lote.NumeroLote}.");
                }

                if (tipo == "Entrada" &&
                    lote.Existencias >
                    int.MaxValue - detalle.Cantidad)
                {
                    throw new InvalidOperationException(
                        "La entrada supera el límite " +
                        "permitido de existencias.");
                }

                detalles.Add(new DetalleAjusteInventario
                {
                    MedicamentoId = detalle.MedicamentoId,

                    LoteId = detalle.LoteId,

                    TipoMovimiento = tipo,

                    Cantidad = detalle.Cantidad,

                    ExistenciasAntes = null,

                    ExistenciasDespues = null,

                    Observaciones = NormalizarOpcional(
                        detalle.Observaciones)
                });
            }

            // CREAR ENCABEZADO

            var ajuste = new AjusteInventario
            {
                AlmacenId = dto.AlmacenId,

                NumeroDocumento = documento,

                FechaAjuste = dto.FechaAjuste.Date,

                FechaAplicacion = null,

                Estado = "Borrador",

                Motivo = motivo,

                Observaciones = NormalizarOpcional(
                    dto.Observaciones),

                Detalles = detalles
            };

            // GUARDAR SIN AFECTAR INVENTARIO

            try
            {
                await _repository.AgregarAsync(ajuste);
            }
            catch (DbUpdateException ex)
                when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "No se pudo registrar el ajuste " +
                    "porque existe un conflicto " +
                    "con otro registro.",
                    ex);
            }

            return await ObtenerPorIdAsync(ajuste.Id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar el ajuste registrado.");
        }

        // ========================================
        // 5. APLICAR AJUSTE AL INVENTARIO
        // ========================================

        public async Task<AjusteInventarioDto> AplicarAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del ajuste no es válido.");
            }

            await EjecutarSeguroAsync(async () =>
            {
                // OBTENER AJUSTE CON BLOQUEO

                var ajuste = await _repository
                    .ObtenerParaProcesarAsync(id);

                if (ajuste == null)
                {
                    throw new KeyNotFoundException(
                        "El ajuste no existe.");
                }

                // COMPROBAR ESTADO

                if (ajuste.Estado != "Borrador")
                {
                    throw new InvalidOperationException(
                        "Solamente se pueden aplicar " +
                        "ajustes en estado Borrador.");
                }

                if (ajuste.Detalles.Count == 0)
                {
                    throw new InvalidOperationException(
                        "El ajuste no contiene detalles.");
                }

                // VALIDAR ALMACEN

                if (!await _repository.AlmacenDisponibleAsync(
                    ajuste.AlmacenId))
                {
                    throw new InvalidOperationException(
                        "El almacén está inactivo.");
                }

                // VALIDAR MEDICAMENTOS

                await ValidarMedicamentosAsync(
                    ajuste.Detalles.Select(d => d.MedicamentoId));

                // ORDEN CONSISTENTE DE LOTES
                // PARA REDUCIR BLOQUEOS CRUZADOS

                var detallesOrdenados = ajuste.Detalles
                    .OrderBy(d => d.LoteId)
                    .ThenBy(d => d.Id)
                    .ToList();

                var lotesProcesados = new HashSet<int>();

                foreach (var detalle in detallesOrdenados)
                {
                    if (detalle.Cantidad <= 0)
                    {
                        throw new InvalidOperationException(
                            "El ajuste contiene una cantidad inválida.");
                    }

                    if (!lotesProcesados.Add(detalle.LoteId))
                    {
                        throw new InvalidOperationException(
                            "El ajuste contiene un lote repetido.");
                    }

                    string tipo = NormalizarTipoMovimiento(
                        detalle.TipoMovimiento);

                    // BLOQUEAR LOTE ANTES DE LEER STOCK

                    var lote = await _repository
                        .ObtenerLoteParaAjustarAsync(
                            detalle.LoteId);

                    if (lote == null)
                    {
                        throw new InvalidOperationException(
                            "Uno de los lotes ya no existe.");
                    }

                    if (!lote.Activo)
                    {
                        throw new InvalidOperationException(
                            "Uno de los lotes está inactivo.");
                    }

                    if (lote.AlmacenId != ajuste.AlmacenId ||
                        lote.MedicamentoId != detalle.MedicamentoId)
                    {
                        throw new InvalidOperationException(
                            "El lote no corresponde " +
                            "al medicamento o al almacén.");
                    }

                    int antes = lote.Existencias;

                    int despues;

                    // ====================================
                    // ENTRADA DE INVENTARIO
                    // ====================================

                    if (tipo == "Entrada")
                    {
                        if (lote.FechaVencimiento.Date
                            <= DateTime.Today)
                        {
                            throw new InvalidOperationException(
                                "No se pueden incrementar " +
                                "las existencias de un lote vencido.");
                        }

                        if (antes > int.MaxValue - detalle.Cantidad)
                        {
                            throw new InvalidOperationException(
                                "La entrada supera el límite " +
                                "permitido de existencias.");
                        }

                        despues = antes + detalle.Cantidad;
                    }
                    // ====================================
                    // SALIDA DE INVENTARIO
                    // ====================================
                    else
                    {
                        if (antes < detalle.Cantidad)
                        {
                            throw new InvalidOperationException(
                                $"Existencias insuficientes " +
                                $"en el lote {lote.NumeroLote}. " +
                                $"Disponible: {antes}. " +
                                $"Solicitado: {detalle.Cantidad}.");
                        }

                        despues = antes - detalle.Cantidad;
                    }

                    // ====================================
                    // GUARDAR HISTORIAL Y EXISTENCIAS
                    // ====================================

                    detalle.TipoMovimiento = tipo;

                    detalle.ExistenciasAntes = antes;

                    detalle.ExistenciasDespues = despues;

                    lote.Existencias = despues;
                }

                // EL ESTADO SE GUARDA EN LA MISMA
                // TRANSACCION QUE LAS EXISTENCIAS

                ajuste.Estado = "Aplicado";

                ajuste.FechaAplicacion = DateTime.UtcNow;
            });

            return await ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar el ajuste aplicado.");
        }

        // ========================================
        // 6. ANULAR AJUSTE EN BORRADOR
        // ========================================

        public async Task<AjusteInventarioDto> AnularAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del ajuste no es válido.");
            }

            await EjecutarSeguroAsync(async () =>
            {
                var ajuste = await _repository
                    .ObtenerParaProcesarAsync(id);

                if (ajuste == null)
                {
                    throw new KeyNotFoundException(
                        "El ajuste no existe.");
                }

                if (ajuste.Estado != "Borrador")
                {
                    throw new InvalidOperationException(
                        "Solamente se pueden anular " +
                        "ajustes en estado Borrador.");
                }

                // Un borrador no modifico existencias.
                // No es necesario revertir inventario.

                ajuste.Estado = "Anulado";
            });

            return await ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar el ajuste anulado.");
        }

        // ========================================
        // 7. VALIDAR DTO Y DETALLES
        // ========================================

        private static void ValidarAjuste(
            AjusteInventarioGuardarDto dto)
        {
            var errores = new List<ValidationResult>();

            bool valido = Validator.TryValidateObject(
                dto,
                new ValidationContext(dto),
                errores,
                validateAllProperties: true);

            if (!valido)
            {
                throw new ArgumentException(
                    errores.FirstOrDefault()?.ErrorMessage
                    ?? "Los datos del ajuste no son válidos.");
            }

            if (string.IsNullOrWhiteSpace(dto.NumeroDocumento))
            {
                throw new ArgumentException(
                    "El documento del ajuste es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(dto.Motivo))
            {
                throw new ArgumentException(
                    "El motivo del ajuste es obligatorio.");
            }

            if (dto.FechaAjuste == default)
            {
                throw new ArgumentException(
                    "La fecha del ajuste no es válida.");
            }

            if (dto.Detalles == null ||
                dto.Detalles.Count == 0)
            {
                throw new ArgumentException(
                    "Debe registrar al menos un detalle.");
            }

            if (dto.Detalles.Count > 200)
            {
                throw new ArgumentException(
                    "Un ajuste no puede superar 200 detalles.");
            }

            foreach (var detalle in dto.Detalles)
            {
                if (detalle == null)
                {
                    throw new ArgumentException(
                        "El ajuste contiene un detalle vacío.");
                }

                errores.Clear();

                valido = Validator.TryValidateObject(
                    detalle,
                    new ValidationContext(detalle),
                    errores,
                    validateAllProperties: true);

                if (!valido)
                {
                    throw new ArgumentException(
                        errores.FirstOrDefault()?.ErrorMessage
                        ?? "Hay un detalle inválido.");
                }

                NormalizarTipoMovimiento(
                    detalle.TipoMovimiento);
            }
        }

        // ========================================
        // 8. VALIDAR MEDICAMENTOS ACTIVOS
        // ========================================

        private async Task ValidarMedicamentosAsync(
            IEnumerable<int> ids)
        {
            var requeridos = ids.Distinct().ToArray();

            if (requeridos.Length == 0 ||
                requeridos.Any(id => id <= 0))
            {
                throw new ArgumentException(
                    "Debe seleccionar medicamentos válidos.");
            }

            var activos = await _repository
                .ObtenerMedicamentosActivosAsync(requeridos);

            if (activos.Distinct().Count() != requeridos.Length)
            {
                throw new ArgumentException(
                    "Uno o más medicamentos no existen " +
                    "o se encuentran inactivos.");
            }
        }

        // ========================================
        // 9. NORMALIZAR MOVIMIENTO
        // ========================================

        private static string NormalizarTipoMovimiento(
            string? tipo)
        {
            string valor = (tipo ?? string.Empty).Trim();

            if (valor.Equals(
                "Entrada",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Entrada";
            }

            if (valor.Equals(
                "Salida",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Salida";
            }

            throw new ArgumentException(
                "El tipo de movimiento debe ser " +
                "Entrada o Salida.");
        }

        // ========================================
        // 10. CONTROL DE CONCURRENCIA SQL SERVER
        // ========================================

        private async Task EjecutarSeguroAsync(
            Func<Task> operacion)
        {
            ArgumentNullException.ThrowIfNull(operacion);

            try
            {
                await _repository.EjecutarEnTransaccionAsync(
                    operacion);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException(
                    "Otra operación modificó los datos " +
                    "durante el ajuste de inventario.",
                    ex);
            }
            catch (DbUpdateException ex)
                when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "Se produjo un conflicto al guardar " +
                    "el ajuste o las existencias.",
                    ex);
            }
            catch (SqlException ex)
                when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "SQL Server detectó operaciones " +
                    "simultáneas en conflicto. " +
                    "Consultá el estado del ajuste " +
                    "antes de reintentar.",
                    ex);
            }
        }

        private static bool EsConflictoSql(Exception error)
        {
            for (
                Exception? actual = error;
                actual != null;
                actual = actual.InnerException)
            {
                if (actual is SqlException sql)
                {
                    foreach (SqlError detalle in sql.Errors)
                    {
                        // 1205: Interbloqueo
                        // 1222: Tiempo de bloqueo agotado
                        // 2601: Indice unico duplicado
                        // 2627: Restriccion unica duplicada

                        if (detalle.Number == 1205 ||
                            detalle.Number == 1222 ||
                            detalle.Number == 2601 ||
                            detalle.Number == 2627)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // ========================================
        // 11. NORMALIZAR TEXTOS OPCIONALES
        // ========================================

        private static string? NormalizarOpcional(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        // ========================================
        // 12. MAPEAR MODELO A DTO
        // ========================================

        private static AjusteInventarioDto MapearDto(
            AjusteInventario ajuste)
        {
            return new AjusteInventarioDto
            {
                Id = ajuste.Id,

                AlmacenId = ajuste.AlmacenId,

                AlmacenNombre =
                    ajuste.Almacen?.Nombre ?? string.Empty,

                NumeroDocumento = ajuste.NumeroDocumento,

                FechaAjuste = ajuste.FechaAjuste,

                FechaAplicacion = ajuste.FechaAplicacion,

                Estado = ajuste.Estado,

                Motivo = ajuste.Motivo,

                Observaciones = ajuste.Observaciones,

                Detalles = ajuste.Detalles
                    .OrderBy(d => d.Id)
                    .Select(d => new DetalleAjusteInventarioDto
                    {
                        Id = d.Id,

                        AjusteInventarioId =
                            d.AjusteInventarioId,

                        MedicamentoId = d.MedicamentoId,

                        MedicamentoNombre =
                            d.Medicamento?.Nombre
                            ?? string.Empty,

                        LoteId = d.LoteId,

                        NumeroLote =
                            d.Lote?.NumeroLote
                            ?? string.Empty,

                        TipoMovimiento = d.TipoMovimiento,

                        Cantidad = d.Cantidad,

                        ExistenciasAntes =
                            d.ExistenciasAntes,

                        ExistenciasDespues =
                            d.ExistenciasDespues,

                        Observaciones = d.Observaciones
                    })
                    .ToList()
            };
        }
    }
}
