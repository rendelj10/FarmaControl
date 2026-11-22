
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Service
{
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _repository;

        public VentaService(IVentaRepository repository)
        {
            _repository = repository;
        }

        // ========================================
        // 1. OBTENER TODAS
        // ========================================

        public async Task<List<VentaDto>> ObtenerTodasAsync()
        {
            var ventas = await _repository.ObtenerTodasAsync();

            return ventas.Select(MapearDto).ToList();
        }

        // ========================================
        // 2. CONSULTAR POR ID
        // ========================================

        public async Task<VentaDto?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                return null;

            var venta = await _repository.ObtenerPorIdAsync(id);

            return venta == null
                ? null
                : MapearDto(venta);
        }

        // ========================================
        // 3. DATATABLES
        // ========================================

        public Task<DataTablesResponseDto<VentaDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return _repository.ObtenerPaginadasAsync(request);
        }

        // ========================================
        // 4. CREAR BORRADOR
        // ========================================

        public async Task<VentaDto> CrearBorradorAsync(
            VentaGuardarDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            ValidarVenta(dto);

            string documento = dto.NumeroDocumento
                .Trim()
                .ToUpperInvariant();

            if (!await _repository.ClienteDisponibleAsync(
                dto.ClienteId))
            {
                throw new ArgumentException(
                    "El cliente no existe o está inactivo.");
            }

            if (!await _repository.AlmacenDisponibleAsync(
                dto.AlmacenId))
            {
                throw new ArgumentException(
                    "El almacén no existe o está inactivo.");
            }

            if (await _repository.ExisteDocumentoAsync(documento))
            {
                throw new InvalidOperationException(
                    "Ya existe una venta con ese documento.");
            }

            await ValidarMedicamentosAsync(
                dto.Detalles.Select(d => d.MedicamentoId));

            // No permitir que un mismo lote aparezca
            // repetido en la venta.

            var lotesIds = new HashSet<int>();

            foreach (var detalle in dto.Detalles)
            {
                if (!lotesIds.Add(detalle.LoteId))
                {
                    throw new ArgumentException(
                        "Un lote no puede repetirse " +
                        "en los detalles de una venta.");
                }

                var lote = await _repository
                    .ObtenerLoteParaConsultaAsync(detalle.LoteId);

                if (lote == null ||
                    !lote.Activo ||
                    lote.MedicamentoId != detalle.MedicamentoId ||
                    lote.AlmacenId != dto.AlmacenId)
                {
                    throw new ArgumentException(
                        "Uno de los lotes seleccionados " +
                        "no corresponde al medicamento " +
                        "o al almacén de la venta.");
                }

                if (lote.FechaVencimiento.Date <= Proyecto_FarmaControl.Utilidades.FechaNicaragua.Hoy)
                {
                    throw new InvalidOperationException(
                        "No se pueden vender lotes vencidos.");
                }

                // Esta comprobacion es informativa:
                // el borrador NO reserva existencias.
                // Se volvera a verificar el stock
                // bajo bloqueo al confirmar.

                if (lote.Existencias < detalle.Cantidad)
                {
                    throw new InvalidOperationException(
                        $"Existencias insuficientes " +
                        $"en el lote {lote.NumeroLote}.");
                }
            }

            var detalles = dto.Detalles
                .Select(d => new DetalleVenta
                {
                    MedicamentoId = d.MedicamentoId,
                    LoteId = d.LoteId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = CalcularSubtotal(
                        d.Cantidad,
                        d.PrecioUnitario)
                })
                .ToList();

            decimal subtotal =
                detalles.Sum(d => d.Subtotal);

            ValidarTotal(subtotal);

            // La regla fiscal se definira
            // posteriormente.

            decimal impuesto = 0m;

            var venta = new Venta
            {
                ClienteId = dto.ClienteId,

                AlmacenId = dto.AlmacenId,

                NumeroDocumento = documento,

                FechaVenta = dto.FechaVenta.Date,

                FechaConfirmacion = null,

                Estado = "Borrador",

                Subtotal = subtotal,
                Impuesto = impuesto,
                Total = subtotal + impuesto,

                Observaciones =
                    NormalizarOpcional(dto.Observaciones),

                Detalles = detalles
            };

            // El borrador no descuenta inventario.

            try
            {
                await _repository.AgregarAsync(venta);
            }
            catch (DbUpdateException ex)
                when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "No se pudo crear la venta porque " +
                    "existe un conflicto con otro registro.",
                    ex);
            }

            return await ObtenerPorIdAsync(venta.Id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar la venta registrada.");
        }

        // ========================================
        // 5. CONFIRMAR VENTA
        // ========================================

        public async Task<VentaDto> ConfirmarAsync(
            int id,
            int usuarioId)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la venta no es válido.");
            }

            if (usuarioId <= 0)
            {
                throw new ArgumentException(
                    "No se pudo identificar al usuario que confirma la venta.");
            }

            await EjecutarSeguroAsync(async () =>
            {
                // Bloquear la venta antes de
                // comprobar su estado.

                var venta = await _repository
                    .ObtenerParaProcesarAsync(id);

                if (venta == null)
                {
                    throw new KeyNotFoundException(
                        "La venta no existe.");
                }

                if (venta.Estado != "Borrador")
                {
                    throw new InvalidOperationException(
                        "Solamente se pueden confirmar " +
                        "ventas en estado Borrador.");
                }

                if (venta.Detalles.Count == 0)
                {
                    throw new InvalidOperationException(
                        "La venta no contiene detalles.");
                }

                // Comprobar nuevamente las relaciones.

                if (!await _repository.ClienteDisponibleAsync(
                    venta.ClienteId))
                {
                    throw new InvalidOperationException(
                        "El cliente está inactivo.");
                }

                if (!await _repository.AlmacenDisponibleAsync(
                    venta.AlmacenId))
                {
                    throw new InvalidOperationException(
                        "El almacén está inactivo.");
                }

                await ValidarMedicamentosAsync(
                    venta.Detalles.Select(d => d.MedicamentoId));

                // Bloquear los lotes en orden
                // ascendente de identificador.
                // Así reducimos el riesgo de
                // bloqueos cruzados.

                var detallesOrdenados = venta.Detalles
                    .OrderBy(d => d.LoteId)
                    .ThenBy(d => d.Id)
                    .ToList();

                var lotesProcesados = new HashSet<int>();

                var detallesMovimiento =
                    new List<DetalleMovimiento>();

                foreach (var detalle in detallesOrdenados)
                {
                    if (detalle.Cantidad <= 0)
                    {
                        throw new InvalidOperationException(
                            "La venta contiene una cantidad inválida.");
                    }

                    if (!lotesProcesados.Add(detalle.LoteId))
                    {
                        throw new InvalidOperationException(
                            "La venta tiene un lote repetido.");
                    }

                    // Obtener bloqueo de actualizacion.
                    // No se utiliza un lote cargado
                    // previamente sin bloqueo.

                    var lote = await _repository
                        .ObtenerLoteParaDescontarAsync(
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

                    if (lote.MedicamentoId != detalle.MedicamentoId ||
                        lote.AlmacenId != venta.AlmacenId)
                    {
                        throw new InvalidOperationException(
                            "El lote no corresponde al " +
                            "medicamento o al almacén.");
                    }

                    if (lote.FechaVencimiento.Date <= Proyecto_FarmaControl.Utilidades.FechaNicaragua.Hoy)
                    {
                        throw new InvalidOperationException(
                            "No se pueden confirmar ventas " +
                            "con lotes vencidos.");
                    }

                    // Verificar el stock ACTUAL
                    // bajo bloqueo SQL Server.

                    if (lote.Existencias < detalle.Cantidad)
                    {
                        throw new InvalidOperationException(
                            $"No hay suficientes existencias " +
                            $"del lote {lote.NumeroLote}. " +
                            $"Disponible: {lote.Existencias}. " +
                            $"Solicitado: {detalle.Cantidad}.");
                    }

                    // Descontar una sola vez.

                    lote.Existencias -= detalle.Cantidad;

                    // Registrar la salida del lote dentro
                    // del mismo movimiento de la venta.

                    detallesMovimiento.Add(
                        new DetalleMovimiento
                        {
                            LoteOrigen = lote,
                            Cantidad = detalle.Cantidad
                        });
                }

                DateTime fechaConfirmacion = DateTime.UtcNow;

                // El movimiento se guarda en la misma transacción
                // que los descuentos y el cambio de estado.

                var movimiento = new MovimientoInventario
                {
                    TipoMovimiento = "SalidaVenta",
                    AlmacenOrigenId = venta.AlmacenId,
                    UsuarioId = usuarioId,
                    VentaId = venta.Id,
                    FechaMovimiento = fechaConfirmacion,
                    Motivo = "Venta confirmada",
                    Referencia = venta.NumeroDocumento,
                    Estado = "Confirmado",
                    Detalles = detallesMovimiento
                };

                await _repository.AgregarMovimientoAsync(
                    movimiento);

                venta.Estado = "Confirmada";

                venta.FechaConfirmacion = fechaConfirmacion;
            });

            return await ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar la venta confirmada.");
        }

        // ========================================
        // 6. ANULAR BORRADOR
        // ========================================

        public async Task<VentaDto> AnularAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la venta no es válido.");
            }

            await EjecutarSeguroAsync(async () =>
            {
                var venta = await _repository
                    .ObtenerParaProcesarAsync(id);

                if (venta == null)
                {
                    throw new KeyNotFoundException(
                        "La venta no existe.");
                }

                if (venta.Estado != "Borrador")
                {
                    throw new InvalidOperationException(
                        "Solamente se pueden anular " +
                        "ventas en estado Borrador.");
                }

                // El borrador no afectó el stock.
                // No se devuelve inventario.

                venta.Estado = "Anulada";
            });

            return await ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar la venta anulada.");
        }

        // ========================================
        // 7. VALIDAR VENTA Y DETALLES
        // ========================================

        private static void ValidarVenta(VentaGuardarDto dto)
        {
            var errores = new List<ValidationResult>();

            if (!Validator.TryValidateObject(
                dto,
                new ValidationContext(dto),
                errores,
                validateAllProperties: true))
            {
                throw new ArgumentException(
                    errores.FirstOrDefault()?.ErrorMessage
                    ?? "Los datos de la venta no son válidos.");
            }

            if (string.IsNullOrWhiteSpace(dto.NumeroDocumento))
            {
                throw new ArgumentException(
                    "El documento es obligatorio.");
            }

            if (dto.FechaVenta == default)
            {
                throw new ArgumentException(
                    "La fecha de venta no es válida.");
            }

            if (dto.Detalles == null ||
                dto.Detalles.Count == 0)
            {
                throw new ArgumentException(
                    "La venta debe tener al menos un detalle.");
            }

            if (dto.Detalles.Count > 200)
            {
                throw new ArgumentException(
                    "La venta no puede superar 200 detalles.");
            }

            foreach (var detalle in dto.Detalles)
            {
                if (detalle == null)
                {
                    throw new ArgumentException(
                        "Existe un detalle vacío.");
                }

                errores.Clear();

                if (!Validator.TryValidateObject(
                    detalle,
                    new ValidationContext(detalle),
                    errores,
                    validateAllProperties: true))
                {
                    throw new ArgumentException(
                        errores.FirstOrDefault()?.ErrorMessage
                        ?? "Hay un detalle inválido.");
                }

                if (decimal.Round(
                    detalle.PrecioUnitario,
                    2,
                    MidpointRounding.AwayFromZero)
                    != detalle.PrecioUnitario)
                {
                    throw new ArgumentException(
                        "El precio admite como máximo " +
                        "dos decimales.");
                }
            }
        }

        // ========================================
        // 8. VALIDAR MEDICAMENTOS
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
        // 9. CONTROL DE CONCURRENCIA
        // ========================================

        private async Task EjecutarSeguroAsync(
            Func<Task> operacion)
        {
            try
            {
                await _repository.EjecutarEnTransaccionAsync(
                    operacion);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException(
                    "Otra operación modificó los datos " +
                    "durante la confirmación.",
                    ex);
            }
            catch (DbUpdateException ex)
                when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "Hubo un conflicto al actualizar " +
                    "la venta o el inventario. " +
                    "Consultá su estado antes de reintentar.",
                    ex);
            }
            catch (SqlException ex)
                when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "SQL Server detectó operaciones " +
                    "simultáneas en conflicto. " +
                    "Consultá el estado antes de reintentar.",
                    ex);
            }
        }

        private static bool EsConflictoSql(Exception error)
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
        // 10. CALCULOS
        // ========================================

        private static decimal CalcularSubtotal(
            int cantidad,
            decimal precioUnitario)
        {
            return decimal.Round(
                cantidad * precioUnitario,
                2,
                MidpointRounding.AwayFromZero);
        }

        private static void ValidarTotal(decimal subtotal)
        {
            if (subtotal <= 0m ||
                subtotal > 999999999999.99m)
            {
                throw new ArgumentException(
                    "El total de la venta está " +
                    "fuera del rango permitido.");
            }
        }

        private static string? NormalizarOpcional(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        // ========================================
        // 11. MAPEAR VENTA A DTO
        // ========================================

        private static VentaDto MapearDto(Venta venta)
        {
            return new VentaDto
            {
                Id = venta.Id,

                ClienteId = venta.ClienteId,

                ClienteNombre =
                    venta.Cliente?.Nombre ?? string.Empty,

                AlmacenId = venta.AlmacenId,

                AlmacenNombre =
                    venta.Almacen?.Nombre ?? string.Empty,

                NumeroDocumento = venta.NumeroDocumento,

                FechaVenta = venta.FechaVenta,

                FechaConfirmacion =
                    venta.FechaConfirmacion,

                Estado = venta.Estado,

                Subtotal = venta.Subtotal,

                Impuesto = venta.Impuesto,

                Total = venta.Total,

                Observaciones = venta.Observaciones,

                Detalles = venta.Detalles
                    .OrderBy(d => d.Id)
                    .Select(d => new DetalleVentaDto
                    {
                        Id = d.Id,

                        VentaId = d.VentaId,

                        MedicamentoId = d.MedicamentoId,

                        MedicamentoNombre =
                            d.Medicamento?.Nombre
                            ?? string.Empty,

                        LoteId = d.LoteId,

                        NumeroLote =
                            d.Lote?.NumeroLote
                            ?? string.Empty,

                        Cantidad = d.Cantidad,

                        PrecioUnitario = d.PrecioUnitario,

                        Subtotal = d.Subtotal
                    })
                    .ToList()
            };
        }
    }
}
