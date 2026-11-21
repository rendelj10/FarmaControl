using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Service
{
    public class PagoService : IPagoService
    {
        private readonly IPagoRepository _repository;

        public PagoService(IPagoRepository repository)
        {
            _repository = repository;
        }

        public Task<List<PagoDto>> ObtenerTodosAsync() =>
            _repository.ObtenerTodosAsync();

        public Task<PagoDto?> ObtenerPorIdAsync(int id) =>
            id > 0 ? _repository.ObtenerPorIdAsync(id) : Task.FromResult<PagoDto?>(null);

        public Task<DataTablesResponseDto<PagoDto>> ObtenerPaginadasAsync(
            DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return _repository.ObtenerPaginadasAsync(request);
        }

        public async Task<PagoDto> RegistrarAsync(PagoGuardarDto dto, int usuarioId)
        {
            ArgumentNullException.ThrowIfNull(dto);
            if (usuarioId <= 0)
                throw new ArgumentException("No se identificó al usuario autenticado.");

            var errores = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, new ValidationContext(dto),
                errores, validateAllProperties: true))
            {
                throw new ArgumentException(errores.FirstOrDefault()?.ErrorMessage
                    ?? "Los datos del pago no son válidos.");
            }

            if (dto.CompraId.HasValue == dto.VentaId.HasValue)
                throw new ArgumentException("Debe indicar una compra o una venta, pero no ambas.");

            if ((dto.CompraId.HasValue && dto.CompraId.Value <= 0) ||
                (dto.VentaId.HasValue && dto.VentaId.Value <= 0) ||
                (dto.TurnoCajaId.HasValue && dto.TurnoCajaId.Value <= 0))
                throw new ArgumentException("Los identificadores no son válidos.");

            if (dto.Monto <= 0m || dto.Monto > 9999999999999999.99m ||
                decimal.Round(dto.Monto, 2, MidpointRounding.AwayFromZero) != dto.Monto)
                throw new ArgumentException("El monto debe ser positivo y tener máximo dos decimales.");

            string? referencia = Limpiar(dto.Referencia);
            string? observaciones = Limpiar(dto.Observaciones);
            var pago = new Pago
            {
                CompraId = dto.CompraId,
                VentaId = dto.VentaId,
                MetodoPagoId = dto.MetodoPagoId,
                UsuarioId = usuarioId,
                TurnoCajaId = dto.TurnoCajaId,
                FechaPago = DateTime.UtcNow,
                Monto = dto.Monto,
                Referencia = referencia,
                Estado = "Registrado",
                Observaciones = observaciones
            };

            try
            {
                await _repository.EjecutarEnTransaccionAsync(async () =>
                {
                    // El documento se bloquea antes de verificar saldo y caja.
                    decimal totalDocumento;
                    decimal pagado;
                    int almacenId;
                    if (dto.VentaId.HasValue)
                    {
                        var venta = await _repository.BloquearVentaAsync(dto.VentaId.Value)
                            ?? throw new KeyNotFoundException("La venta no existe.");
                        if (venta.Estado != "Confirmada")
                            throw new InvalidOperationException("Solo se pueden pagar ventas confirmadas.");
                        totalDocumento = venta.Total;
                        almacenId = venta.AlmacenId;
                        pagado = await _repository.TotalPagadoVentaAsync(venta.Id);
                    }
                    else
                    {
                        var compra = await _repository.BloquearCompraAsync(dto.CompraId!.Value)
                            ?? throw new KeyNotFoundException("La compra no existe.");
                        if (compra.Estado != "Recibida")
                            throw new InvalidOperationException("Solo se pueden pagar compras recibidas.");
                        totalDocumento = compra.Total;
                        almacenId = compra.AlmacenId;
                        pagado = await _repository.TotalPagadoCompraAsync(compra.Id);
                    }

                    if (totalDocumento <= 0m || pagado + dto.Monto > totalDocumento)
                        throw new InvalidOperationException("El pago supera el saldo pendiente del documento.");

                    var metodo = await _repository.ObtenerMetodoAsync(dto.MetodoPagoId)
                        ?? throw new ArgumentException("El método de pago no existe.");
                    if (!metodo.Activo)
                        throw new InvalidOperationException("El método de pago está inactivo.");
                    if (metodo.RequiereReferencia && string.IsNullOrWhiteSpace(referencia))
                        throw new ArgumentException("Este método de pago requiere una referencia.");

                    // La clasificación de efectivo se basa en el nombre del catálogo.
                    // Mantener "Efectivo" como denominación única para caja.
                    bool esEfectivo = string.Equals(metodo.Nombre.Trim(),
                        "Efectivo", StringComparison.OrdinalIgnoreCase);
                    if (dto.VentaId.HasValue && esEfectivo && !dto.TurnoCajaId.HasValue)
                        throw new ArgumentException("El pago de venta en efectivo requiere un turno de caja.");

                    if (dto.TurnoCajaId.HasValue)
                    {
                        var turno = await _repository.BloquearTurnoAsync(dto.TurnoCajaId.Value)
                            ?? throw new ArgumentException("El turno de caja no existe.");
                        if (turno.Estado != "Abierto")
                            throw new InvalidOperationException("El turno de caja no está abierto.");
                        if (turno.UsuarioAperturaId != usuarioId)
                            throw new InvalidOperationException("El turno no pertenece al usuario autenticado.");
                        int? sucursal = await _repository.ObtenerSucursalAlmacenAsync(almacenId);
                        if (!sucursal.HasValue || turno.SucursalId != sucursal.Value)
                            throw new InvalidOperationException("La sucursal del turno no coincide con la del documento.");
                    }
                    await _repository.AgregarAsync(pago);
                });
            }
            catch (DbUpdateException ex) when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "Ocurrió un conflicto al registrar el pago; consulte el saldo antes de reintentar.", ex);
            }
            catch (SqlException ex) when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "SQL Server detectó una operación concurrente; consulte el estado antes de reintentar.", ex);
            }

            return await _repository.ObtenerPorIdAsync(pago.Id)
                ?? throw new InvalidOperationException("No se pudo consultar el pago registrado.");
        }

        public async Task<PagoDto> AnularAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Identificador de pago inválido.");

            try
            {
                await _repository.EjecutarEnTransaccionAsync(async () =>
                {
                    var previo = await _repository.ObtenerPagoParaAnularAsync(id)
                        ?? throw new KeyNotFoundException("El pago no existe.");

                    // Seguir el orden de bloqueos utilizado en RegistrarAsync:
                    // documento -> turno -> pago.
                    if (previo.VentaId.HasValue)
                    {
                        _ = await _repository.BloquearVentaAsync(previo.VentaId.Value)
                            ?? throw new KeyNotFoundException("La venta no existe.");
                    }
                    else if (previo.CompraId.HasValue)
                    {
                        _ = await _repository.BloquearCompraAsync(previo.CompraId.Value)
                            ?? throw new KeyNotFoundException("La compra no existe.");
                    }
                    else
                        throw new InvalidOperationException("El pago no tiene un documento válido.");

                    if (previo.TurnoCajaId.HasValue)
                    {
                        var turno = await _repository.BloquearTurnoAsync(previo.TurnoCajaId.Value)
                            ?? throw new InvalidOperationException("El turno del pago no existe.");
                        if (turno.Estado != "Abierto")
                            throw new InvalidOperationException(
                                "No se puede anular un pago de un turno de caja cerrado.");
                    }

                    var pago = await _repository.BloquearPagoAsync(id)
                        ?? throw new KeyNotFoundException("El pago no existe.");
                    if (pago.Estado != "Registrado")
                        throw new InvalidOperationException("El pago ya está anulado.");
                    pago.Estado = "Anulado";
                });
            }
            catch (DbUpdateException ex) when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException("No se pudo anular por conflicto en SQL Server.", ex);
            }
            catch (SqlException ex) when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException("Operación concurrente. Consulte el pago antes de reintentar.", ex);
            }

            return await _repository.ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException("No se pudo consultar el pago anulado.");
        }

        private static string? Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private static bool EsConflictoSql(Exception error)
        {
            for (Exception? actual = error; actual != null; actual = actual.InnerException)
            {
                if (actual is SqlException sql)
                {
                    foreach (SqlError detalle in sql.Errors)
                    {
                        if (detalle.Number is 1205 or 1222 or 2601 or 2627)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
