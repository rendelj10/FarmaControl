
using System.Data;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class CompraRepository : ICompraRepository
    {
        private readonly AppDbContext _context;

        public CompraRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // 1. CONSULTAR TODAS LAS COMPRAS
        // ========================================

        public async Task<List<Compra>> ObtenerTodasAsync()
        {
            return await _context.Set<Compra>()
                .Include(c => c.Proveedor)
                .Include(c => c.Almacen)
                .AsNoTracking()
                .OrderByDescending(c => c.FechaCompra)
                .ThenByDescending(c => c.Id)
                .ToListAsync();
        }

        // ========================================
        // 2. CONSULTAR COMPRA POR ID
        // SIN BLOQUEO EXCLUSIVO DE PROCESAMIENTO
        // ========================================

        public async Task<Compra?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _context.Set<Compra>()
                .Include(c => c.Proveedor)
                .Include(c => c.Almacen)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Medicamento)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Lote)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // ========================================
        // 3. NUEVO:
        // OBTENER COMPRA CON BLOQUEO
        // ========================================

        public async Task<Compra?> ObtenerParaProcesarAsync(
            int id)
        {
            if (id <= 0)
            {
                return null;
            }

            // Este metodo debe ejecutarse dentro
            // de EjecutarEnTransaccionAsync.
            //
            // UPDLOCK:
            // solicita bloqueo de actualizacion.
            //
            // HOLDLOCK:
            // conserva la proteccion hasta finalizar
            // la transaccion.
            //
            // Una segunda recepcion de esta misma
            // compra tendra que esperar y despues
            // comprobar nuevamente su estado.

            if (_context.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException(
                    "La consulta de procesamiento requiere " +
                    "una transacción activa.");
            }

            return await _context.Set<Compra>()
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM [Compras] WITH (UPDLOCK, HOLDLOCK)
                       WHERE [Id] = {id}")
                .Include(c => c.Proveedor)
                .Include(c => c.Almacen)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Medicamento)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Lote)
                .AsSplitQuery()
                .FirstOrDefaultAsync();
        }

        // ========================================
        // 4. AGREGAR COMPRA Y DETALLES
        // ========================================

        public async Task AgregarAsync(Compra compra)
        {
            ArgumentNullException.ThrowIfNull(compra);

            await _context.Set<Compra>()
                .AddAsync(compra);

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 5. ACTUALIZAR COMPRA
        // ========================================

        public async Task ActualizarAsync(Compra compra)
        {
            ArgumentNullException.ThrowIfNull(compra);

            // Se espera una entidad rastreada
            // por el contexto actual.

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 6. GUARDAR CAMBIOS
        // ========================================

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }

        // ========================================
        // 7. VALIDAR DOCUMENTO DUPLICADO
        // ========================================

        public async Task<bool> ExisteDocumentoAsync(
            int proveedorId,
            string numeroDocumento,
            int? excluirId = null)
        {
            if (proveedorId <= 0 ||
                string.IsNullOrWhiteSpace(numeroDocumento))
            {
                return false;
            }

            string documentoNormalizado =
                numeroDocumento.Trim().ToUpperInvariant();

            return await _context.Set<Compra>()
                .AsNoTracking()
                .AnyAsync(c =>
                    c.ProveedorId == proveedorId &&
                    c.NumeroDocumento.Trim().ToUpper()
                        == documentoNormalizado &&
                    (!excluirId.HasValue ||
                     c.Id != excluirId.Value));
        }

        // ========================================
        // 8. VALIDAR PROVEEDOR ACTIVO
        // ========================================

        public async Task<bool> ProveedorDisponibleAsync(
            int proveedorId)
        {
            if (proveedorId <= 0)
            {
                return false;
            }

            return await _context.Set<Proveedor>()
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id == proveedorId &&
                    p.Activo);
        }

        // ========================================
        // 9. VALIDAR ALMACEN Y SUCURSAL
        // ========================================

        public async Task<bool> AlmacenDisponibleAsync(
            int almacenId)
        {
            if (almacenId <= 0)
            {
                return false;
            }

            return await _context.Set<Almacen>()
                .AsNoTracking()
                .AnyAsync(a =>
                    a.Id == almacenId &&
                    a.Activo &&
                    a.Sucursal.Activo);
        }

        // ========================================
        // 10. MEDICAMENTOS ACTIVOS
        // ========================================

        public async Task<List<int>>
            ObtenerMedicamentosActivosAsync(
                IEnumerable<int> medicamentosIds)
        {
            ArgumentNullException.ThrowIfNull(
                medicamentosIds);

            var ids = medicamentosIds
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (ids.Length == 0)
            {
                return new List<int>();
            }

            return await _context.Set<Medicamento>()
                .AsNoTracking()
                .Where(m =>
                    ids.Contains(m.Id) &&
                    m.Activo)
                .Select(m => m.Id)
                .ToListAsync();
        }

        // ========================================
        // 11. BUSCAR LOTE CON BLOQUEO
        // ========================================

        public async Task<Lote?> ObtenerLoteAsync(
            int medicamentoId,
            int almacenId,
            string numeroLote)
        {
            if (medicamentoId <= 0 ||
                almacenId <= 0 ||
                string.IsNullOrWhiteSpace(numeroLote))
            {
                return null;
            }

            if (_context.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException(
                    "La consulta del lote para recepción " +
                    "requiere una transacción activa.");
            }

            string numeroNormalizado =
                numeroLote.Trim().ToUpperInvariant();

            // El indice unico existente:
            //
            // MedicamentoId, AlmacenId, NumeroLote
            //
            // ayuda a localizar el lote exacto.
            //
            // La combinacion de bloqueos protege
            // la lectura y la posible insercion
            // durante la transaccion.
            //
            // Los numeros de lote se almacenan
            // normalizados en mayusculas.

            return await _context.Set<Lote>()
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM [Lotes] WITH (UPDLOCK, HOLDLOCK)
                       WHERE [MedicamentoId] = {medicamentoId}
                         AND [AlmacenId] = {almacenId}
                         AND [NumeroLote] = {numeroNormalizado}")
                .FirstOrDefaultAsync();
        }

        // ========================================
        // 12. PREPARAR NUEVO LOTE
        // ========================================

        public async Task AgregarLoteAsync(Lote lote)
        {
            ArgumentNullException.ThrowIfNull(lote);

            // No guardar aqui.
            // La transaccion confirma conjuntamente
            // compra, detalles y existencias.

            await _context.Set<Lote>()
                .AddAsync(lote);
        }

        // ========================================
        // 13. REGISTRAR MOVIMIENTO DE INVENTARIO
        // ========================================

        public async Task AgregarMovimientoAsync(
            MovimientoInventario movimiento)
        {
            ArgumentNullException.ThrowIfNull(movimiento);

            // No guardar aqui. El movimiento debe confirmarse
            // junto con la compra y las existencias.

            await _context.Set<MovimientoInventario>()
                .AddAsync(movimiento);
        }

        // ========================================
        // 14. EJECUTAR TRANSACCION
        // ========================================

        public async Task EjecutarEnTransaccionAsync(
            Func<Task> operacion)
        {
            ArgumentNullException.ThrowIfNull(operacion);

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                await operacion();

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();

                throw;
            }
        }

        // ========================================
        // 14. DATATABLES - PAGINACION SQL SERVER
        // ========================================

        public async Task<DataTablesResponseDto<CompraDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _context.Set<Compra>()
                .AsNoTracking()
                .AsQueryable();

            // TOTAL DE REGISTROS

            int recordsTotal =
                await query.CountAsync();

            // FILTRO DE BUSQUEDA

            string? search =
                request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.NumeroDocumento.Contains(search) ||
                    c.Estado.Contains(search) ||
                    c.Proveedor.RazonSocial.Contains(search) ||
                    c.Almacen.Nombre.Contains(search) ||
                    (c.Observaciones != null &&
                     c.Observaciones.Contains(search)));
            }

            int recordsFiltered =
                await query.CountAsync();

            // ORDENAMIENTO

            var orden =
                request.Order?.FirstOrDefault();

            int indiceColumna =
                orden?.Column ?? -1;

            string? columna = indiceColumna >= 0
                ? request.Columns?
                    .ElementAtOrDefault(indiceColumna)?.Data
                : null;

            bool descendente = string.Equals(
                orden?.Dir,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            IOrderedQueryable<Compra> ordenada =
                columna?.ToLowerInvariant() switch
                {
                    "id" => descendente
                        ? query.OrderByDescending(c => c.Id)
                        : query.OrderBy(c => c.Id),

                    "proveedornombre" => descendente
                        ? query.OrderByDescending(
                            c => c.Proveedor.RazonSocial)
                        : query.OrderBy(
                            c => c.Proveedor.RazonSocial),

                    "almacennombre" => descendente
                        ? query.OrderByDescending(
                            c => c.Almacen.Nombre)
                        : query.OrderBy(
                            c => c.Almacen.Nombre),

                    "numerodocumento" => descendente
                        ? query.OrderByDescending(
                            c => c.NumeroDocumento)
                        : query.OrderBy(
                            c => c.NumeroDocumento),

                    "fechacompra" => descendente
                        ? query.OrderByDescending(
                            c => c.FechaCompra)
                        : query.OrderBy(
                            c => c.FechaCompra),

                    "fecharecepcion" => descendente
                        ? query.OrderByDescending(
                            c => c.FechaRecepcion)
                        : query.OrderBy(
                            c => c.FechaRecepcion),

                    "estado" => descendente
                        ? query.OrderByDescending(c => c.Estado)
                        : query.OrderBy(c => c.Estado),

                    "subtotal" => descendente
                        ? query.OrderByDescending(c => c.Subtotal)
                        : query.OrderBy(c => c.Subtotal),

                    "impuesto" => descendente
                        ? query.OrderByDescending(c => c.Impuesto)
                        : query.OrderBy(c => c.Impuesto),

                    "total" => descendente
                        ? query.OrderByDescending(c => c.Total)
                        : query.OrderBy(c => c.Total),

                    _ => query.OrderByDescending(
                        c => c.FechaCompra)
                };

            // Orden secundario estable.

            ordenada = ordenada.ThenBy(c => c.Id);

            // PAGINACION

            int start = Math.Max(0, request.Start);

            int length = request.Length == -1
                ? Math.Min(recordsFiltered, 500)
                : request.Length > 0
                    ? Math.Min(request.Length, 100)
                    : 10;

            // PROYECCION DTO

            var data = await ordenada
                .Skip(start)
                .Take(length)
                .Select(c => new CompraDto
                {
                    Id = c.Id,

                    ProveedorId = c.ProveedorId,
                    ProveedorNombre =
                        c.Proveedor.RazonSocial,

                    AlmacenId = c.AlmacenId,
                    AlmacenNombre =
                        c.Almacen.Nombre,

                    NumeroDocumento =
                        c.NumeroDocumento,

                    FechaCompra = c.FechaCompra,

                    FechaRecepcion = c.FechaRecepcion,

                    Estado = c.Estado,

                    Subtotal = c.Subtotal,

                    Impuesto = c.Impuesto,

                    Total = c.Total,

                    Observaciones = c.Observaciones
                })
                .ToListAsync();

            return new DataTablesResponseDto<CompraDto>
            {
                Draw = Math.Max(0, request.Draw),

                RecordsTotal = recordsTotal,

                RecordsFiltered = recordsFiltered,

                Data = data
            };
        }
    }
}
