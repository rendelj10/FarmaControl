
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Services
{
    public class CompraService : ICompraService
    {
        private readonly ICompraRepository _repository;

        public CompraService(ICompraRepository repository)
        {
            _repository = repository;
        }

        // ========================================
        // 1. CONSULTAR TODAS LAS COMPRAS
        // ========================================

        public async Task<List<CompraDto>> ObtenerTodasAsync()
        {
            var compras = await _repository.ObtenerTodasAsync();

            return compras
                .Select(MapearDto)
                .ToList();
        }

        // ========================================
        // 2. CONSULTAR COMPRA POR ID
        // ========================================

        public async Task<CompraDto?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var compra = await _repository.ObtenerPorIdAsync(id);

            return compra == null
                ? null
                : MapearDto(compra);
        }

        // ========================================
        // 3. PAGINACION DATATABLES
        // ========================================

        public async Task<DataTablesResponseDto<CompraDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await _repository
                .ObtenerPaginadasAsync(request);
        }

        // ========================================
        // 4. CREAR COMPRA EN BORRADOR
        // ========================================

        public async Task<CompraDto> CrearBorradorAsync(
            CompraGuardarDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            ValidarCompra(dto);

            string documento = dto.NumeroDocumento
                .Trim()
                .ToUpperInvariant();

            // Verificar proveedor activo.

            if (!await _repository.ProveedorDisponibleAsync(
                dto.ProveedorId))
            {
                throw new ArgumentException(
                    "El proveedor no existe o está inactivo.");
            }

            // Verificar almacen y sucursal activos.

            if (!await _repository.AlmacenDisponibleAsync(
                dto.AlmacenId))
            {
                throw new ArgumentException(
                    "El almacén no existe o está inactivo.");
            }

            // Evitar documentos duplicados.

            if (await _repository.ExisteDocumentoAsync(
                dto.ProveedorId,
                documento))
            {
                throw new InvalidOperationException(
                    "Ya existe una compra con ese número " +
                    "de documento para el proveedor.");
            }

            // Comprobar medicamentos activos.

            await ValidarMedicamentosAsync(
                dto.Detalles.Select(d => d.MedicamentoId));

            // Calcular cada subtotal en el backend.

            var detalles = dto.Detalles
                .Select(d => new DetalleCompra
                {
                    MedicamentoId = d.MedicamentoId,

                    Cantidad = d.Cantidad,

                    CostoUnitario = d.CostoUnitario,

                    Subtotal = CalcularSubtotal(
                        d.Cantidad,
                        d.CostoUnitario),

                    NumeroLote = NormalizarLote(
                        d.NumeroLote),

                    FechaVencimiento =
                        d.FechaVencimiento?.Date
                })
                .ToList();

            decimal subtotal = detalles.Sum(d => d.Subtotal);

            ValidarTotal(subtotal);

            // Impuesto provisional.
            // Todavia no se implementa
            // una regla tributaria especifica.

            decimal impuesto = 0m;

            var compra = new Compra
            {
                ProveedorId = dto.ProveedorId,

                AlmacenId = dto.AlmacenId,

                NumeroDocumento = documento,

                FechaCompra = dto.FechaCompra.Date,

                FechaRecepcion = null,

                Estado = "Borrador",

                Subtotal = subtotal,

                Impuesto = impuesto,

                Total = subtotal + impuesto,

                Observaciones = NormalizarOpcional(
                    dto.Observaciones),

                Detalles = detalles
            };

            // Solo registra compra y detalles.
            // No modifica inventario.

            await _repository.AgregarAsync(compra);

            return await ObtenerPorIdAsync(compra.Id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar la compra registrada.");
        }

        // ========================================
        // 5. RECIBIR COMPRA
        // ========================================

        public async Task<CompraDto> RecibirAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la compra no es válido.");
            }

            // Toda la operacion se ejecuta
            // dentro de una transaccion SQL.
            //
            // Se captura un posible conflicto
            // sin reintentar automaticamente.

            await EjecutarConControlDeConcurrenciaAsync(
                async () =>
                {
                    // ====================================
                    // OBTENER COMPRA CON BLOQUEO
                    // ====================================

                    var compra = await _repository
                        .ObtenerParaProcesarAsync(id);

                    if (compra == null)
                    {
                        throw new KeyNotFoundException(
                            "La compra no existe.");
                    }

                    // Comprobar el estado DESPUES
                    // de obtener el bloqueo.

                    if (compra.Estado != "Borrador")
                    {
                        throw new InvalidOperationException(
                            "Solamente se pueden recibir " +
                            "compras en estado Borrador.");
                    }

                    if (compra.Detalles.Count == 0)
                    {
                        throw new InvalidOperationException(
                            "La compra no contiene detalles.");
                    }

                    // ====================================
                    // VALIDAR PROVEEDOR Y ALMACEN
                    // ====================================

                    if (!await _repository.ProveedorDisponibleAsync(
                        compra.ProveedorId))
                    {
                        throw new InvalidOperationException(
                            "El proveedor está inactivo.");
                    }

                    if (!await _repository.AlmacenDisponibleAsync(
                        compra.AlmacenId))
                    {
                        throw new InvalidOperationException(
                            "El almacén está inactivo.");
                    }

                    // ====================================
                    // VALIDAR MEDICAMENTOS ACTIVOS
                    // ====================================

                    await ValidarMedicamentosAsync(
                        compra.Detalles.Select(
                            d => d.MedicamentoId));

                    // ====================================
                    // ORDENAR LOS DETALLES
                    // ====================================

                    // Orden consistente para reducir
                    // conflictos al procesar lotes
                    // compartidos por distintas compras.

                    var detallesOrdenados = compra.Detalles
                        .OrderBy(d => d.MedicamentoId)
                        .ThenBy(d =>
                            d.NumeroLote ?? string.Empty,
                            StringComparer.OrdinalIgnoreCase)
                        .ThenBy(d => d.Id)
                        .ToList();

                    // ====================================
                    // VALIDAR NUMEROS DE LOTE
                    // ====================================

                    var claves = new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase);

                    foreach (var detalle in detallesOrdenados)
                    {
                        if (detalle.Cantidad <= 0)
                        {
                            throw new ArgumentException(
                                "La cantidad del detalle debe " +
                                "ser mayor que cero.");
                        }

                        if (string.IsNullOrWhiteSpace(
                            detalle.NumeroLote))
                        {
                            throw new ArgumentException(
                                "Todos los detalles deben tener " +
                                "número de lote.");
                        }

                        if (!detalle.FechaVencimiento.HasValue)
                        {
                            throw new ArgumentException(
                                "Todos los lotes deben tener " +
                                "fecha de vencimiento.");
                        }

                        if (detalle.FechaVencimiento.Value.Date
                            <= DateTime.Today)
                        {
                            throw new InvalidOperationException(
                                "No se pueden recibir " +
                                "medicamentos vencidos.");
                        }

                        string numero = NormalizarLote(
                            detalle.NumeroLote)!;

                        string clave =
                            $"{detalle.MedicamentoId}|{numero}";

                        if (!claves.Add(clave))
                        {
                            throw new InvalidOperationException(
                                "La compra contiene el mismo " +
                                "medicamento y lote más de una vez.");
                        }

                        detalle.NumeroLote = numero;
                    }

                    // ====================================
                    // ACTUALIZAR INVENTARIO
                    // ====================================

                    foreach (var detalle in detallesOrdenados)
                    {
                        string numero = detalle.NumeroLote!;

                        DateTime vencimiento =
                            detalle.FechaVencimiento!.Value.Date;

                        // Consulta con UPDLOCK y HOLDLOCK.
                        // El repositorio requiere
                        // una transaccion activa.

                        var lote = await _repository
                            .ObtenerLoteAsync(
                                detalle.MedicamentoId,
                                compra.AlmacenId,
                                numero);

                        if (lote != null)
                        {
                            // ====================================
                            // LOTE EXISTENTE
                            // ====================================

                            if (!lote.Activo)
                            {
                                throw new InvalidOperationException(
                                    "El lote existente está inactivo.");
                            }

                            if (lote.FechaVencimiento.Date
                                != vencimiento)
                            {
                                throw new InvalidOperationException(
                                    "El lote existente tiene otra " +
                                    "fecha de vencimiento.");
                            }

                            // Sumar existencias con
                            // comprobacion de desbordamiento.

                            lote.Existencias = checked(
                                lote.Existencias +
                                detalle.Cantidad);

                            // El costo de la entrada
                            // permanece en DetalleCompra.

                            detalle.Lote = lote;
                        }
                        else
                        {
                            // ====================================
                            // NUEVO LOTE
                            // ====================================

                            var nuevoLote = new Lote
                            {
                                MedicamentoId =
                                    detalle.MedicamentoId,

                                AlmacenId =
                                    compra.AlmacenId,

                                NumeroLote = numero,

                                CantidadInicial =
                                    detalle.Cantidad,

                                Existencias =
                                    detalle.Cantidad,

                                CostoUnitario =
                                    detalle.CostoUnitario,

                                FechaIngreso =
                                    DateTime.Today,

                                FechaVencimiento =
                                    vencimiento,

                                Activo = true
                            };

                            await _repository
                                .AgregarLoteAsync(nuevoLote);

                            detalle.Lote = nuevoLote;
                        }
                    }

                    // ====================================
                    // CONFIRMAR ESTADO
                    // ====================================

                    // El estado cambia dentro
                    // de la misma transaccion
                    // que actualiza el inventario.

                    compra.Estado = "Recibida";

                    compra.FechaRecepcion = DateTime.UtcNow;

                    // El repositorio ejecutara:
                    // SaveChangesAsync
                    // CommitAsync
                });

            return await ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar la compra recibida.");
        }

        // ========================================
        // 6. ANULAR COMPRA EN BORRADOR
        // ========================================

        public async Task<CompraDto> AnularAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la compra no es válido.");
            }

            await EjecutarConControlDeConcurrenciaAsync(
                async () =>
                {
                    // Utilizar el mismo bloqueo
                    // que se aplica en RecibirAsync.
                    //
                    // Esto ayuda a evitar que
                    // recepcion y anulacion
                    // procesen el mismo borrador.

                    var compra = await _repository
                        .ObtenerParaProcesarAsync(id);

                    if (compra == null)
                    {
                        throw new KeyNotFoundException(
                            "La compra no existe.");
                    }

                    if (compra.Estado != "Borrador")
                    {
                        throw new InvalidOperationException(
                            "Solamente se pueden anular " +
                            "compras en estado Borrador.");
                    }

                    // No se descuenta inventario:
                    // el borrador nunca lo incremento.

                    compra.Estado = "Anulada";
                });

            return await ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException(
                    "No se pudo consultar la compra anulada.");
        }

        // ========================================
        // 7. CONTROL DE CONFLICTOS SQL SERVER
        // ========================================

        private async Task EjecutarConControlDeConcurrenciaAsync(
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
                    "La operación no pudo completarse " +
                    "porque los datos fueron modificados " +
                    "por otra operación.",
                    ex);
            }
            catch (DbUpdateException ex)
                when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "Se produjo un conflicto al actualizar " +
                    "la compra o el inventario. " +
                    "Consultá el estado actual " +
                    "antes de volver a intentarlo.",
                    ex);
            }
            catch (SqlException ex)
                when (EsConflictoSql(ex))
            {
                throw new InvalidOperationException(
                    "SQL Server detectó un conflicto " +
                    "entre operaciones simultáneas. " +
                    "Consultá la compra antes " +
                    "de volver a intentarlo.",
                    ex);
            }
        }

        // ========================================
        // 8. IDENTIFICAR CONFLICTOS SQL
        // ========================================

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
                        // 1205: Deadlock
                        // 1222: Lock timeout
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
        // 9. VALIDAR COMPRA Y DETALLES
        // ========================================

        private static void ValidarCompra(CompraGuardarDto dto)
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
                    ?? "Los datos de la compra no son válidos.");
            }

            if (string.IsNullOrWhiteSpace(
                dto.NumeroDocumento))
            {
                throw new ArgumentException(
                    "El número de documento es obligatorio.");
            }

            if (dto.FechaCompra == default)
            {
                throw new ArgumentException(
                    "Debe ingresar una fecha de compra válida.");
            }

            if (dto.Detalles == null ||
                dto.Detalles.Count == 0)
            {
                throw new ArgumentException(
                    "La compra debe tener al menos un detalle.");
            }

            if (dto.Detalles.Count > 200)
            {
                throw new ArgumentException(
                    "Una compra no puede superar 200 detalles.");
            }

            foreach (var detalle in dto.Detalles)
            {
                if (detalle == null)
                {
                    throw new ArgumentException(
                        "Hay un detalle de compra vacío.");
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
                        ?? "Hay un detalle de compra inválido.");
                }

                if (decimal.Round(
                    detalle.CostoUnitario,
                    2,
                    MidpointRounding.AwayFromZero)
                    != detalle.CostoUnitario)
                {
                    throw new ArgumentException(
                        "El costo unitario admite " +
                        "como máximo dos decimales.");
                }
            }
        }

        // ========================================
        // 10. VALIDAR MEDICAMENTOS ACTIVOS
        // ========================================

        private async Task ValidarMedicamentosAsync(
            IEnumerable<int> ids)
        {
            var requeridos = ids
                .Distinct()
                .ToArray();

            if (requeridos.Length == 0 ||
                requeridos.Any(id => id <= 0))
            {
                throw new ArgumentException(
                    "Debe seleccionar medicamentos válidos.");
            }

            var activos = await _repository
                .ObtenerMedicamentosActivosAsync(requeridos);

            if (activos.Distinct().Count()
                != requeridos.Length)
            {
                throw new ArgumentException(
                    "Uno o más medicamentos no existen " +
                    "o se encuentran inactivos.");
            }
        }

        // ========================================
        // 11. CALCULAR SUBTOTAL
        // ========================================

        private static decimal CalcularSubtotal(
            int cantidad,
            decimal costoUnitario)
        {
            return decimal.Round(
                cantidad * costoUnitario,
                2,
                MidpointRounding.AwayFromZero);
        }

        // ========================================
        // 12. VALIDAR TOTAL
        // ========================================

        private static void ValidarTotal(decimal subtotal)
        {
            if (subtotal <= 0m ||
                subtotal > 999999999999.99m)
            {
                throw new ArgumentException(
                    "El importe total de la compra " +
                    "está fuera del rango permitido.");
            }
        }

        // ========================================
        // 13. NORMALIZAR TEXTOS
        // ========================================

        private static string? NormalizarOpcional(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        private static string? NormalizarLote(
            string? numero)
        {
            return string.IsNullOrWhiteSpace(numero)
                ? null
                : numero.Trim().ToUpperInvariant();
        }

        // ========================================
        // 14. MAPEAR COMPRA A DTO
        // ========================================

        private static CompraDto MapearDto(Compra compra)
        {
            return new CompraDto
            {
                Id = compra.Id,

                ProveedorId = compra.ProveedorId,

                ProveedorNombre =
                    compra.Proveedor?.RazonSocial
                    ?? string.Empty,

                AlmacenId = compra.AlmacenId,

                AlmacenNombre =
                    compra.Almacen?.Nombre
                    ?? string.Empty,

                NumeroDocumento =
                    compra.NumeroDocumento,

                FechaCompra = compra.FechaCompra,

                FechaRecepcion =
                    compra.FechaRecepcion,

                Estado = compra.Estado,

                Subtotal = compra.Subtotal,

                Impuesto = compra.Impuesto,

                Total = compra.Total,

                Observaciones = compra.Observaciones,

                Detalles = compra.Detalles
                    .OrderBy(d => d.Id)
                    .Select(d => new DetalleCompraDto
                    {
                        Id = d.Id,

                        CompraId = d.CompraId,

                        MedicamentoId =
                            d.MedicamentoId,

                        MedicamentoNombre =
                            d.Medicamento?.Nombre
                            ?? string.Empty,

                        Cantidad = d.Cantidad,

                        CostoUnitario =
                            d.CostoUnitario,

                        Subtotal = d.Subtotal,

                        NumeroLote =
                            d.NumeroLote,

                        FechaVencimiento =
                            d.FechaVencimiento,

                        LoteId = d.LoteId
                    })
                    .ToList()
            };
        }
    }
}
