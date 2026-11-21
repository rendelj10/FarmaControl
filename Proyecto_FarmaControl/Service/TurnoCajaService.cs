using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Service
{
    public class TurnoCajaService : ITurnoCajaService
    {
        private readonly ITurnoCajaRepository _repository;

        public TurnoCajaService(ITurnoCajaRepository repository)
        {
            _repository = repository;
        }

        public Task<List<TurnoCajaDto>> ObtenerTodosAsync() =>
            _repository.ObtenerTodosAsync();

        public Task<TurnoCajaDto?> ObtenerPorIdAsync(int id) =>
            id > 0 ? _repository.ObtenerPorIdAsync(id)
                   : Task.FromResult<TurnoCajaDto?>(null);

        public Task<TurnoCajaDto?> ObtenerAbiertoPorUsuarioAsync(int usuarioId) =>
            usuarioId > 0 ? _repository.ObtenerAbiertoPorUsuarioAsync(usuarioId)
                          : Task.FromResult<TurnoCajaDto?>(null);

        public Task<DataTablesResponseDto<TurnoCajaDto>> ObtenerPaginadasAsync(
            DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return _repository.ObtenerPaginadasAsync(request);
        }

        public async Task<TurnoCajaDto> AbrirAsync(TurnoCajaAbrirDto dto, int usuarioId)
        {
            ArgumentNullException.ThrowIfNull(dto);
            if (usuarioId <= 0)
                throw new ArgumentException("El usuario autenticado no es válido.");

            var errores = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, new ValidationContext(dto),
                errores, validateAllProperties: true))
            {
                throw new ArgumentException(
                    errores.FirstOrDefault()?.ErrorMessage ?? "Los datos no son válidos.");
            }

            if (decimal.Round(dto.MontoInicial, 2, MidpointRounding.AwayFromZero)
                != dto.MontoInicial)
            {
                throw new ArgumentException("El monto inicial admite dos decimales.");
            }

            var turno = new TurnoCaja
            {
                SucursalId = dto.SucursalId,
                UsuarioAperturaId = usuarioId,
                FechaApertura = DateTime.UtcNow,
                MontoInicial = dto.MontoInicial,
                Estado = "Abierto",
                Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones)
                    ? null : dto.Observaciones.Trim(),
                FechaCierre = null,
                UsuarioCierreId = null,
                MontoEsperado = null,
                MontoContado = null,
                Diferencia = null
            };

            try
            {
                await _repository.EjecutarEnTransaccionAsync(async () =>
                {
                    if (!await _repository.UsuarioExisteAsync(usuarioId))
                        throw new ArgumentException("El usuario ya no existe.");

                    if (!await _repository.SucursalActivaAsync(dto.SucursalId))
                        throw new ArgumentException("La sucursal no existe o está inactiva.");

                    if (await _repository.ExisteTurnoAbiertoAsync(usuarioId))
                        throw new InvalidOperationException(
                            "El usuario ya tiene un turno de caja abierto.");

                    await _repository.AgregarAsync(turno);
                });
            }
            catch (DbUpdateException ex) when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "No se pudo abrir el turno: ya existe uno abierto o hay un conflicto de datos.", ex);
            }
            catch (SqlException ex) when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "Hubo un conflicto concurrente al abrir caja; consulte el turno abierto antes de reintentar.", ex);
            }

            return await _repository.ObtenerPorIdAsync(turno.Id)
                ?? throw new InvalidOperationException("No se pudo consultar el turno registrado.");
        }

        // Solo el usuario autorizado con el permiso turnocaja.cerrar
        // puede ejecutar esta operación. El controlador exige ese permiso.
        public async Task<TurnoCajaDto> CerrarAsync(
            int id, TurnoCajaCerrarDto dto, int usuarioId)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (id <= 0)
                throw new ArgumentException("El identificador del turno no es válido.");
            if (usuarioId <= 0)
                throw new ArgumentException("El usuario autenticado no es válido.");

            var errores = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, new ValidationContext(dto),
                errores, validateAllProperties: true))
            {
                throw new ArgumentException(
                    errores.FirstOrDefault()?.ErrorMessage ??
                    "Los datos del cierre no son válidos.");
            }

            if (!dto.MontoContado.HasValue)
                throw new ArgumentException("Debe indicar el monto contado en caja.");

            decimal montoContado = dto.MontoContado.Value;
            if (montoContado < 0m ||
                decimal.Round(montoContado, 2, MidpointRounding.AwayFromZero)
                != montoContado)
            {
                throw new ArgumentException(
                    "El monto contado debe ser no negativo y tener máximo dos decimales.");
            }

            try
            {
                await _repository.EjecutarEnTransaccionAsync(async () =>
                {
                    // Bloquear antes de consultar los pagos para evitar cierres
                    // simultáneos y registros o anulaciones concurrentes.
                    var turno = await _repository.ObtenerParaCerrarAsync(id)
                        ?? throw new KeyNotFoundException("El turno de caja no existe.");

                    if (turno.Estado != "Abierto")
                        throw new InvalidOperationException(
                            "Solo se puede cerrar un turno en estado Abierto.");

                    if (!await _repository.UsuarioExisteAsync(usuarioId))
                        throw new ArgumentException(
                            "El usuario que realiza el cierre ya no existe.");

                    var (ingresos, egresos) = await _repository
                        .ObtenerTotalesEfectivoAsync(turno.Id);

                    decimal montoEsperado = turno.MontoInicial + ingresos - egresos;
                    if (montoEsperado < 0m)
                        throw new InvalidOperationException(
                            "Los egresos en efectivo superan el saldo disponible. " +
                            "Revise los movimientos antes de cerrar el turno.");
                    if (montoEsperado > 9999999999999999.99m)
                        throw new InvalidOperationException(
                            "El monto esperado supera el rango permitido.");

                    string? notasCierre = string.IsNullOrWhiteSpace(dto.Observaciones)
                        ? null : dto.Observaciones.Trim();

                    if (notasCierre != null)
                    {
                        string observaciones = string.IsNullOrWhiteSpace(turno.Observaciones)
                            ? "Cierre: " + notasCierre
                            : turno.Observaciones.Trim() + " | Cierre: " + notasCierre;

                        if (observaciones.Length > 250)
                            throw new ArgumentException(
                                "Las observaciones de apertura y cierre juntas " +
                                "superan 250 caracteres. Acorte las observaciones del cierre.");

                        turno.Observaciones = observaciones;
                    }

                    turno.MontoEsperado = montoEsperado;
                    turno.MontoContado = montoContado;
                    turno.Diferencia = montoContado - montoEsperado;
                    turno.UsuarioCierreId = usuarioId;
                    turno.FechaCierre = DateTime.UtcNow;
                    turno.Estado = "Cerrado";
                });
            }
            catch (DbUpdateException ex) when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "No se pudo cerrar el turno por un conflicto en SQL Server.", ex);
            }
            catch (SqlException ex) when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "Operación concurrente de caja: consulte el turno antes de reintentar.", ex);
            }

            return await _repository.ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException(
                    "El turno fue cerrado, pero no pudo consultarse.");
        }

        private static bool EsConflictoSql(Exception error)
        {
            for (Exception? actual = error; actual != null; actual = actual.InnerException)
            {
                if (actual is SqlException sql)
                {
                    foreach (SqlError detalle in sql.Errors)
                    {
                        if (detalle.Number is 2601 or 2627 or 1205 or 1222)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
