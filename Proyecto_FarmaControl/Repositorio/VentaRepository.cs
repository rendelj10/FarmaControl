
using System.Data;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class VentaRepository : IVentaRepository
    {
        private readonly AppDbContext _context;

        public VentaRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // 1. OBTENER TODAS
        // ========================================

        public async Task<List<Venta>> ObtenerTodasAsync()
        {
            return await _context.Set<Venta>()
                .Include(v => v.Cliente)
                .Include(v => v.Almacen)
                .AsNoTracking()
                .OrderByDescending(v => v.FechaVenta)
                .ThenByDescending(v => v.Id)
                .ToListAsync();
        }

        // ========================================
        // 2. OBTENER POR ID
        // ========================================

        public async Task<Venta?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _context.Set<Venta>()
                .Include(v => v.Cliente)
                .Include(v => v.Almacen)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Medicamento)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Lote)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        // ========================================
        // 3. OBTENER VENTA CON BLOQUEO
        // ========================================

        public async Task<Venta?> ObtenerParaProcesarAsync(int id)
        {
            if (id <= 0)
                return null;

            if (_context.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException(
                    "Se requiere una transacción activa.");
            }

            // Bloquea la venta hasta finalizar
            // la transacción.
            //
            // Importante: cargar detalles, pero NO
            // cargar sus lotes en esta consulta.
            // Los lotes se leerán con bloqueo
            // individual antes de descontar stock.

            return await _context.Set<Venta>()
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM [Ventas] WITH (UPDLOCK, HOLDLOCK)
                       WHERE [Id] = {id}")
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync();
        }

        // ========================================
        // 4. AGREGAR VENTA
        // ========================================

        public async Task AgregarAsync(Venta venta)
        {
            ArgumentNullException.ThrowIfNull(venta);

            await _context.Set<Venta>().AddAsync(venta);

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 5. DOCUMENTO DUPLICADO
        // ========================================

        public async Task<bool> ExisteDocumentoAsync(
            string numeroDocumento)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
                return false;

            string documento = numeroDocumento
                .Trim()
                .ToUpperInvariant();

            return await _context.Set<Venta>()
                .AsNoTracking()
                .AnyAsync(v =>
                    v.NumeroDocumento.Trim().ToUpper()
                    == documento);
        }

        // ========================================
        // 6. CLIENTE ACTIVO
        // ========================================

        public Task<bool> ClienteDisponibleAsync(int clienteId)
        {
            if (clienteId <= 0)
                return Task.FromResult(false);

            return _context.Set<Cliente>()
                .AsNoTracking()
                .AnyAsync(c =>
                    c.Id == clienteId &&
                    c.Activo);
        }

        // ========================================
        // 7. ALMACEN Y SUCURSAL ACTIVOS
        // ========================================

        public Task<bool> AlmacenDisponibleAsync(int almacenId)
        {
            if (almacenId <= 0)
                return Task.FromResult(false);

            return _context.Set<Almacen>()
                .AsNoTracking()
                .AnyAsync(a =>
                    a.Id == almacenId &&
                    a.Activo &&
                    a.Sucursal.Activo);
        }

        // ========================================
        // 8. MEDICAMENTOS ACTIVOS
        // ========================================

        public async Task<List<int>>
            ObtenerMedicamentosActivosAsync(
                IEnumerable<int> ids)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var identificadores = ids
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (identificadores.Length == 0)
                return new List<int>();

            return await _context.Set<Medicamento>()
                .AsNoTracking()
                .Where(m =>
                    identificadores.Contains(m.Id) &&
                    m.Activo)
                .Select(m => m.Id)
                .ToListAsync();
        }

        // ========================================
        // 9. CONSULTAR LOTE SIN DESCONTAR
        // ========================================

        public async Task<Lote?>
            ObtenerLoteParaConsultaAsync(int loteId)
        {
            if (loteId <= 0)
                return null;

            return await _context.Set<Lote>()
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == loteId);
        }

        // ========================================
        // 10. CONSULTAR LOTE CON BLOQUEO
        // ========================================

        public async Task<Lote?>
            ObtenerLoteParaDescontarAsync(int loteId)
        {
            if (loteId <= 0)
                return null;

            if (_context.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException(
                    "El descuento de inventario " +
                    "requiere una transacción activa.");
            }

            // UPDLOCK:
            // protege la actualización del lote.
            //
            // HOLDLOCK:
            // conserva el bloqueo hasta que
            // termine la transacción.

            return await _context.Set<Lote>()
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM [Lotes] WITH (UPDLOCK, HOLDLOCK)
                       WHERE [Id] = {loteId}")
                .FirstOrDefaultAsync();
        }

        // ========================================
        // 11. REGISTRAR MOVIMIENTO DE INVENTARIO
        // ========================================

        public async Task AgregarMovimientoAsync(
            MovimientoInventario movimiento)
        {
            ArgumentNullException.ThrowIfNull(movimiento);

            // No guardar aqui. La venta, sus descuentos
            // y el movimiento se confirman juntos.

            await _context.Set<MovimientoInventario>()
                .AddAsync(movimiento);
        }

        // ========================================
        // 12. TRANSACCION SQL SERVER
        // ========================================

        public async Task EjecutarEnTransaccionAsync(
            Func<Task> operacion)
        {
            ArgumentNullException.ThrowIfNull(operacion);

            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
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
        // 12. DATATABLES
        // ========================================

        public async Task<DataTablesResponseDto<VentaDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _context.Set<Venta>()
                .AsNoTracking()
                .AsQueryable();

            int recordsTotal = await query.CountAsync();

            string? search =
                request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(v =>
                    v.NumeroDocumento.Contains(search) ||
                    v.Estado.Contains(search) ||
                    v.Cliente.Nombre.Contains(search) ||
                    v.Almacen.Nombre.Contains(search) ||
                    (v.Observaciones != null &&
                     v.Observaciones.Contains(search)));
            }

            int recordsFiltered = await query.CountAsync();

            var orden = request.Order?.FirstOrDefault();

            int indiceColumna = orden?.Column ?? -1;

            string? columna = indiceColumna >= 0
                ? request.Columns?
                    .ElementAtOrDefault(indiceColumna)?.Data
                : null;

            bool descendente = string.Equals(
                orden?.Dir,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            IOrderedQueryable<Venta> ordenada =
                columna?.ToLowerInvariant() switch
                {
                    "id" => descendente
                        ? query.OrderByDescending(v => v.Id)
                        : query.OrderBy(v => v.Id),

                    "clientenombre" => descendente
                        ? query.OrderByDescending(
                            v => v.Cliente.Nombre)
                        : query.OrderBy(
                            v => v.Cliente.Nombre),

                    "almacennombre" => descendente
                        ? query.OrderByDescending(
                            v => v.Almacen.Nombre)
                        : query.OrderBy(
                            v => v.Almacen.Nombre),

                    "numerodocumento" => descendente
                        ? query.OrderByDescending(
                            v => v.NumeroDocumento)
                        : query.OrderBy(
                            v => v.NumeroDocumento),

                    "fechaventa" => descendente
                        ? query.OrderByDescending(
                            v => v.FechaVenta)
                        : query.OrderBy(
                            v => v.FechaVenta),

                    "fechaconfirmacion" => descendente
                        ? query.OrderByDescending(
                            v => v.FechaConfirmacion)
                        : query.OrderBy(
                            v => v.FechaConfirmacion),

                    "estado" => descendente
                        ? query.OrderByDescending(v => v.Estado)
                        : query.OrderBy(v => v.Estado),

                    "total" => descendente
                        ? query.OrderByDescending(v => v.Total)
                        : query.OrderBy(v => v.Total),

                    _ => query.OrderByDescending(
                        v => v.FechaVenta)
                };

            ordenada = ordenada.ThenBy(v => v.Id);

            int start = Math.Max(0, request.Start);

            int length = request.Length == -1
                ? Math.Min(recordsFiltered, 500)
                : request.Length > 0
                    ? Math.Min(request.Length, 100)
                    : 10;

            var data = await ordenada
                .Skip(start)
                .Take(length)
                .Select(v => new VentaDto
                {
                    Id = v.Id,

                    ClienteId = v.ClienteId,
                    ClienteNombre = v.Cliente.Nombre,

                    AlmacenId = v.AlmacenId,
                    AlmacenNombre = v.Almacen.Nombre,

                    NumeroDocumento = v.NumeroDocumento,

                    FechaVenta = v.FechaVenta,
                    FechaConfirmacion =
                        v.FechaConfirmacion,

                    Estado = v.Estado,

                    Subtotal = v.Subtotal,
                    Impuesto = v.Impuesto,
                    Total = v.Total,

                    Observaciones = v.Observaciones
                })
                .ToListAsync();

            return new DataTablesResponseDto<VentaDto>
            {
                Draw = Math.Max(0, request.Draw),
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
