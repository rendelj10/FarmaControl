
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
        // 1. CONSULTAR TODOS LOS AJUSTES
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
        // 2. CONSULTAR AJUSTE POR ID
        // ========================================

        public async Task<AjusteInventarioDto?>
            ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var ajuste = await _repository
                .ObtenerPorIdAsync(id);

            return ajuste == null
                ? null
                : MapearDto(ajuste);
        }

        // ========================================
        // 3. CONSULTAR AJUSTES PAGINADOS
        // ========================================

        public Task<DataTablesResponseDto<AjusteInventarioDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return _repository.ObtenerPaginadasAsync(request);
        }

        // ========================================
        // 4. CREAR AJUSTE EN BORRADOR
        // ========================================

        public async Task<AjusteInventarioDto>
            CrearBorradorAsync(
                AjusteInventarioGuardarDto dto,
                int usuarioId)
        {
            ArgumentNullException.ThrowIfNull(dto);

            ValidarAjuste(dto, usuarioId);

            // VALIDAR ALMACEN

            if (!await _repository.AlmacenDisponibleAsync(
                dto.AlmacenId))
            {
                throw new ArgumentException(
                    "El almacén no existe o está inactivo.");
            }

            string tipoAjuste =
                NormalizarTipoAjuste(dto.TipoAjuste);

            string motivo = dto.Motivo.Trim();

            var lotesProcesados = new HashSet<int>();

            var detalles =
                new List<DetalleAjusteInventario>();

            // UN AJUSTE SOLO PUEDE GENERAR ENTRADAS O SALIDAS,
            // NUNCA AMBOS TIPOS DE MOVIMIENTO.
            // 1 = ENTRADA; -1 = SALIDA.
            int? sentidoAjuste = null;

            // VALIDAR CADA LOTE Y SU CONTEO

            foreach (var detalleDto in dto.Detalles)
            {
                if (!lotesProcesados.Add(detalleDto.LoteId))
                {
                    throw new ArgumentException(
                        "Un lote no puede repetirse " +
                        "dentro del mismo ajuste.");
                }

                var lote = await _repository
                    .ObtenerLoteParaConsultaAsync(
                        detalleDto.LoteId);

                if (lote == null ||
                    !lote.Activo ||
                    lote.AlmacenId != dto.AlmacenId ||
                    lote.Medicamento == null ||
                    !lote.Medicamento.Activo)
                {
                    throw new ArgumentException(
                        "El lote no existe, está inactivo " +
                        "o no pertenece a un medicamento " +
                        "activo del almacén seleccionado.");
                }

                if (lote.Existencias < 0)
                {
                    throw new InvalidOperationException(
                        "El lote tiene existencias inválidas.");
                }

                // EL SERVICIO CALCULA LA DIFERENCIA

                int diferencia =
                    detalleDto.CantidadFisica -
                    lote.Existencias;

                if (diferencia != 0)
                {
                    int sentidoActual = Math.Sign(diferencia);

                    if (sentidoAjuste.HasValue &&
                        sentidoAjuste.Value != sentidoActual)
                    {
                        throw new ArgumentException(
                            "Un ajuste no puede combinar " +
                            "entradas y salidas. Registre " +
                            "un ajuste de entrada y otro " +
                            "de salida por separado.");
                    }

                    sentidoAjuste = sentidoActual;
                }

                detalles.Add(new DetalleAjusteInventario
                {
                    LoteId = lote.Id,

                    ExistenciaSistema =
                        lote.Existencias,

                    CantidadFisica =
                        detalleDto.CantidadFisica,

                    Diferencia = diferencia,

                    Observacion = NormalizarOpcional(
                        detalleDto.Observacion)
                });
            }

            // NO SE REGISTRAN AJUSTES SIN DIFERENCIAS,
            // PORQUE NO GENERARÍAN UN MOVIMIENTO REAL.

            if (!sentidoAjuste.HasValue)
            {
                throw new ArgumentException(
                    "No existen diferencias entre las " +
                    "existencias del sistema y el conteo físico. " +
                    "No es necesario registrar un ajuste.");
            }

            // CREAR CABECERA DEL AJUSTE

            var ajuste = new AjusteInventario
            {
                AlmacenId = dto.AlmacenId,

                UsuarioId = usuarioId,

                FechaAjuste = dto.FechaAjuste,

                FechaConfirmacion = null,

                ConfirmadoPorUsuarioId = null,

                TipoAjuste = tipoAjuste,

                Estado = "Borrador",

                Motivo = motivo,

                Observaciones = NormalizarOpcional(
                    dto.Observaciones),

                Detalles = detalles
            };

            // GUARDAR SIN MODIFICAR EXISTENCIAS

            try
            {
                await _repository.AgregarAsync(ajuste);
            }
            catch (DbUpdateException ex)
                when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "No se pudo registrar el ajuste " +
                    "por un conflicto en la base de datos.",
                    ex);
            }

            return await ObtenerPorIdAsync(ajuste.Id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar el ajuste registrado.");
        }

        // ========================================
        // 5. CONFIRMAR AJUSTE
        // ========================================

        public async Task<AjusteInventarioDto>
            ConfirmarAsync(int id, int usuarioId)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del ajuste no es válido.");
            }

            if (usuarioId <= 0)
            {
                throw new ArgumentException(
                    "No se pudo identificar al usuario " +
                    "que confirma el ajuste.");
            }

            await EjecutarSeguroAsync(async () =>
            {
                // OBTENER AJUSTE CON BLOQUEO SQL

                var ajuste = await _repository
                    .ObtenerParaProcesarAsync(id);

                if (ajuste == null)
                {
                    throw new KeyNotFoundException(
                        "El ajuste no existe.");
                }

                // SOLO CONFIRMAR BORRADORES

                if (ajuste.Estado != "Borrador")
                {
                    throw new InvalidOperationException(
                        "Solamente se pueden confirmar " +
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

                // PROCESAR LOTES EN ORDEN Y VALIDAR QUE TODOS
                // TENGAN EL MISMO SENTIDO DE AJUSTE.

                var detallesOrdenados = ajuste.Detalles
                    .OrderBy(d => d.LoteId)
                    .ThenBy(d => d.Id)
                    .ToList();

                var lotesProcesados =
                    new HashSet<int>();

                var cambiosLote =
                    new List<(DetalleAjusteInventario Detalle,
                              Lote Lote,
                              int Diferencia)>();

                int? sentidoAjuste = null;

                foreach (var detalle in detallesOrdenados)
                {
                    // EVITAR LOTES REPETIDOS

                    if (!lotesProcesados.Add(detalle.LoteId))
                    {
                        throw new InvalidOperationException(
                            "El ajuste contiene un lote repetido.");
                    }

                    if (detalle.CantidadFisica < 0 ||
                        detalle.ExistenciaSistema < 0)
                    {
                        throw new InvalidOperationException(
                            "El ajuste contiene cantidades inválidas.");
                    }

                    // OBTENER LOTE CON BLOQUEO

                    var lote = await _repository
                        .ObtenerLoteParaAjustarAsync(
                            detalle.LoteId);

                    if (lote == null || !lote.Activo)
                    {
                        throw new InvalidOperationException(
                            "Uno de los lotes no existe " +
                            "o está inactivo.");
                    }

                    if (lote.AlmacenId != ajuste.AlmacenId ||
                        lote.Medicamento == null ||
                        !lote.Medicamento.Activo)
                    {
                        throw new InvalidOperationException(
                            "El lote no pertenece a un " +
                            "medicamento activo del almacén.");
                    }

                    // VALIDAR QUE EL INVENTARIO NO CAMBIÓ
                    // DESPUÉS DE REGISTRAR EL CONTEO.

                    if (lote.Existencias !=
                        detalle.ExistenciaSistema)
                    {
                        throw new InvalidOperationException(
                            $"Las existencias del lote " +
                            $"{lote.NumeroLote} cambiaron " +
                            "después del conteo. " +
                            "Es necesario registrar un " +
                            "conteo actualizado.");
                    }

                    // RECALCULAR LA DIFERENCIA ANTES DE CONFIRMAR.

                    int diferencia =
                        detalle.CantidadFisica -
                        lote.Existencias;

                    if (diferencia != 0)
                    {
                        int sentidoActual = Math.Sign(diferencia);

                        if (sentidoAjuste.HasValue &&
                            sentidoAjuste.Value != sentidoActual)
                        {
                            throw new InvalidOperationException(
                                "El ajuste contiene entradas y salidas. " +
                                "Divida el conteo en dos ajustes " +
                                "independientes: uno de entrada y otro " +
                                "de salida.");
                        }

                        sentidoAjuste = sentidoActual;
                    }

                    detalle.ExistenciaSistema = lote.Existencias;
                    detalle.Diferencia = diferencia;

                    cambiosLote.Add((
                        detalle,
                        lote,
                        diferencia));
                }

                // UN AJUSTE SIN DIFERENCIAS NO GENERA MOVIMIENTO.

                if (!sentidoAjuste.HasValue)
                {
                    throw new InvalidOperationException(
                        "No existen diferencias entre las " +
                        "existencias del sistema y el conteo físico. " +
                        "No se puede confirmar un ajuste sin cambios.");
                }

                // ACTUALIZAR LOTES Y CONSTRUIR LOS DETALLES
                // DEL MOVIMIENTO DESPUÉS DE VALIDAR TODO EL AJUSTE.

                var detallesMovimiento =
                    new List<DetalleMovimiento>();

                foreach (var cambio in cambiosLote)
                {
                    var detalle = cambio.Detalle;
                    var lote = cambio.Lote;
                    int diferencia = cambio.Diferencia;

                    lote.Existencias = detalle.CantidadFisica;

                    if (diferencia > 0)
                    {
                        detallesMovimiento.Add(
                            new DetalleMovimiento
                            {
                                LoteDestinoId = lote.Id,
                                Cantidad = diferencia,
                                Observacion = detalle.Observacion
                            });
                    }
                    else if (diferencia < 0)
                    {
                        detallesMovimiento.Add(
                            new DetalleMovimiento
                            {
                                LoteOrigenId = lote.Id,
                                Cantidad = Math.Abs(diferencia),
                                Observacion = detalle.Observacion
                            });
                    }
                }

                DateTime fechaConfirmacion =
                    DateTime.UtcNow;

                // CREAR MOVIMIENTO DE INVENTARIO CON EL TIPO
                // Y EL ALMACÉN CORRESPONDIENTES AL SENTIDO.

                var movimiento =
                    new MovimientoInventario
                    {
                        TipoMovimiento = sentidoAjuste.Value > 0
                            ? "AjusteEntrada"
                            : "AjusteSalida",

                        AlmacenOrigenId = sentidoAjuste.Value < 0
                            ? ajuste.AlmacenId
                            : null,

                        AlmacenDestinoId = sentidoAjuste.Value > 0
                            ? ajuste.AlmacenId
                            : null,

                        UsuarioId = usuarioId,

                        AjusteInventarioId = ajuste.Id,

                        FechaMovimiento =
                        fechaConfirmacion,

                        Motivo = ajuste.Motivo,

                        Estado = "Confirmado",

                        Observaciones =
                        ajuste.Observaciones,

                        Detalles = detallesMovimiento
                    };

                await _repository
                    .AgregarMovimientoAsync(movimiento);

                // CONFIRMAR AJUSTE

                ajuste.Estado = "Confirmado";

                ajuste.ConfirmadoPorUsuarioId =
                    usuarioId;

                ajuste.FechaConfirmacion =
                    fechaConfirmacion;

                // EL REPOSITORIO GUARDA TODO
                // DENTRO DE LA MISMA TRANSACCION
            });

            return await ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar el ajuste confirmado.");
        }

        // ========================================
        // 6. ANULAR AJUSTE EN BORRADOR
        // ========================================

        public async Task<AjusteInventarioDto>
            AnularAsync(int id)
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

                ajuste.Estado = "Anulado";
            });

            return await ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar el ajuste anulado.");
        }

        // ========================================
        // 7. VALIDAR DATOS DE ENTRADA
        // ========================================

        private static void ValidarAjuste(
            AjusteInventarioGuardarDto dto,
            int usuarioId)
        {
            if (usuarioId <= 0)
            {
                throw new ArgumentException(
                    "No se pudo identificar al usuario.");
            }

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

            if (dto.FechaAjuste == default)
            {
                throw new ArgumentException(
                    "La fecha del ajuste no es válida.");
            }

            NormalizarTipoAjuste(dto.TipoAjuste);

            if (string.IsNullOrWhiteSpace(dto.Motivo))
            {
                throw new ArgumentException(
                    "El motivo del ajuste es obligatorio.");
            }

            if (dto.Detalles == null ||
                dto.Detalles.Count == 0)
            {
                throw new ArgumentException(
                    "Debe registrar al menos un lote.");
            }

            if (dto.Detalles.Count > 200)
            {
                throw new ArgumentException(
                    "Un ajuste no puede tener " +
                    "más de 200 detalles.");
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
            }
        }

        // ========================================
        // 8. NORMALIZAR TIPO DE AJUSTE
        // ========================================

        private static string NormalizarTipoAjuste(
            string? tipoAjuste)
        {
            string valor =
                (tipoAjuste ?? string.Empty).Trim();

            if (valor.Equals(
                "Conteo",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Conteo";
            }

            if (valor.Equals(
                "Correccion",
                StringComparison.OrdinalIgnoreCase) ||
                valor.Equals(
                    "Corrección",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Correccion";
            }

            throw new ArgumentException(
                "El tipo de ajuste debe ser " +
                "Conteo o Correccion.");
        }

        // ========================================
        // 9. EJECUTAR OPERACION SEGURA
        // ========================================

        private async Task EjecutarSeguroAsync(
            Func<Task> operacion)
        {
            try
            {
                await _repository
                    .EjecutarEnTransaccionAsync(operacion);
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
                    "Se produjo un conflicto al " +
                    "guardar el ajuste o las existencias.",
                    ex);
            }
            catch (SqlException ex)
                when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "SQL Server detectó un conflicto " +
                    "entre operaciones simultáneas. " +
                    "Consultá el estado del ajuste " +
                    "antes de reintentar.",
                    ex);
            }
        }

        // ========================================
        // 10. DETECTAR CONFLICTOS SQL
        // ========================================

        private static bool EsConflictoSql(
            Exception error)
        {
            for (Exception? actual = error;
                 actual != null;
                 actual = actual.InnerException)
            {
                if (actual is SqlException sql)
                {
                    foreach (SqlError detalle in sql.Errors)
                    {
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
        // 11. NORMALIZAR TEXTO OPCIONAL
        // ========================================

        private static string? NormalizarOpcional(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        // ========================================
        // 12. CONVERTIR MODELO A DTO
        // ========================================

        private static AjusteInventarioDto MapearDto(
            AjusteInventario ajuste)
        {
            return new AjusteInventarioDto
            {
                Id = ajuste.Id,

                AlmacenId = ajuste.AlmacenId,

                AlmacenNombre =
                    ajuste.Almacen?.Nombre
                    ?? string.Empty,

                UsuarioId = ajuste.UsuarioId,

                UsuarioNombre =
                    ajuste.Usuario?.UserName
                    ?? string.Empty,

                ConfirmadoPorUsuarioId =
                    ajuste.ConfirmadoPorUsuarioId,

                ConfirmadoPorUsuarioNombre =
                    ajuste.ConfirmadoPorUsuario?.UserName,

                FechaAjuste = ajuste.FechaAjuste,

                FechaConfirmacion =
                    ajuste.FechaConfirmacion,

                TipoAjuste = ajuste.TipoAjuste,

                Estado = ajuste.Estado,

                Motivo = ajuste.Motivo,

                Observaciones = ajuste.Observaciones,

                MovimientoInventarioId =
                    ajuste.MovimientoInventario?.Id,

                Detalles = ajuste.Detalles
                    .OrderBy(d => d.Id)
                    .Select(d =>
                        new DetalleAjusteInventarioDto
                        {
                            Id = d.Id,

                            AjusteInventarioId =
                                d.AjusteInventarioId,

                            LoteId = d.LoteId,

                            NumeroLote =
                                d.Lote?.NumeroLote
                                ?? string.Empty,

                            MedicamentoId =
                                d.Lote?.MedicamentoId
                                ?? 0,

                            MedicamentoNombre =
                                d.Lote?.Medicamento?.Nombre
                                ?? string.Empty,

                            ExistenciaSistema =
                                d.ExistenciaSistema,

                            CantidadFisica =
                                d.CantidadFisica,

                            Diferencia =
                                d.Diferencia,

                            Observacion =
                                d.Observacion
                        })
                    .ToList()
            };
        }
    }
}
