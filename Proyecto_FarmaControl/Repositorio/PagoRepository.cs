using System.Data;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class PagoRepository : IPagoRepository
    {
        private readonly AppDbContext _context;

        public PagoRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<PagoDto> ConsultarPagos()
        {
            return _context.Pagos.AsNoTracking().Select(p => new PagoDto
            {
                Id = p.Id,
                CompraId = p.CompraId,
                VentaId = p.VentaId,
                Documento = p.CompraId != null
                    ? p.Compra!.NumeroDocumento : p.Venta!.NumeroDocumento,
                MetodoPagoId = p.MetodoPagoId,
                MetodoPagoNombre = p.MetodoPago.Nombre,
                UsuarioId = p.UsuarioId,
                UsuarioNombre = p.Usuario.UserName,
                TurnoCajaId = p.TurnoCajaId,
                FechaPago = p.FechaPago,
                Monto = p.Monto,
                Referencia = p.Referencia,
                Estado = p.Estado,
                Observaciones = p.Observaciones
            });
        }

        public Task<List<PagoDto>> ObtenerTodosAsync() =>
            ConsultarPagos().OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.Id).ToListAsync();

        public Task<PagoDto?> ObtenerPorIdAsync(int id) =>
            ConsultarPagos().FirstOrDefaultAsync(p => p.Id == id);

        public async Task<DataTablesResponseDto<PagoDto>> ObtenerPaginadasAsync(
            DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);
            IQueryable<Pago> query = _context.Pagos.AsNoTracking();
            int total = await query.CountAsync();
            string? buscar = request.Search?.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                bool idValido = int.TryParse(buscar, out int id);
                query = query.Where(p =>
                    (idValido && p.Id == id) ||
                    p.MetodoPago.Nombre.Contains(buscar) ||
                    p.Estado.Contains(buscar) ||
                    (p.Referencia != null && p.Referencia.Contains(buscar)) ||
                    (p.Compra != null && p.Compra.NumeroDocumento.Contains(buscar)) ||
                    (p.Venta != null && p.Venta.NumeroDocumento.Contains(buscar)));
            }
            int filtrados = await query.CountAsync();
            var orden = request.Order?.FirstOrDefault();
            int indice = orden?.Column ?? -1;
            string? columna = indice >= 0
                ? request.Columns?.ElementAtOrDefault(indice)?.Data : null;
            bool desc = string.Equals(orden?.Dir, "desc", StringComparison.OrdinalIgnoreCase);
            IOrderedQueryable<Pago> ordenados = columna?.ToLowerInvariant() switch
            {
                "id" => desc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
                "fechapago" => desc ? query.OrderByDescending(p => p.FechaPago) : query.OrderBy(p => p.FechaPago),
                "monto" => desc ? query.OrderByDescending(p => p.Monto) : query.OrderBy(p => p.Monto),
                "metodopagonombre" => desc ? query.OrderByDescending(p => p.MetodoPago.Nombre) : query.OrderBy(p => p.MetodoPago.Nombre),
                "estado" => desc ? query.OrderByDescending(p => p.Estado) : query.OrderBy(p => p.Estado),
                _ => query.OrderByDescending(p => p.FechaPago)
            };
            int inicio = Math.Max(0, request.Start);
            int cantidad = request.Length == -1
                ? Math.Min(filtrados, 500)
                : request.Length > 0 ? Math.Min(request.Length, 100) : 10;
            var datos = await ordenados.ThenBy(p => p.Id)
                .Skip(inicio).Take(cantidad)
                .Select(p => new PagoDto
                {
                    Id = p.Id,
                    CompraId = p.CompraId,
                    VentaId = p.VentaId,
                    Documento = p.CompraId != null
                        ? p.Compra!.NumeroDocumento : p.Venta!.NumeroDocumento,
                    MetodoPagoId = p.MetodoPagoId,
                    MetodoPagoNombre = p.MetodoPago.Nombre,
                    UsuarioId = p.UsuarioId,
                    UsuarioNombre = p.Usuario.UserName,
                    TurnoCajaId = p.TurnoCajaId,
                    FechaPago = p.FechaPago,
                    Monto = p.Monto,
                    Referencia = p.Referencia,
                    Estado = p.Estado,
                    Observaciones = p.Observaciones
                }).ToListAsync();

            return new DataTablesResponseDto<PagoDto>
            {
                Draw = Math.Max(0, request.Draw),
                RecordsTotal = total,
                RecordsFiltered = filtrados,
                Data = datos
            };
        }

        private void RequerirTransaccion()
        {
            if (_context.Database.CurrentTransaction == null)
                throw new InvalidOperationException("La operación requiere una transacción activa.");
        }

        public async Task<Compra?> BloquearCompraAsync(int id)
        {
            RequerirTransaccion();
            return await _context.Compras.FromSqlInterpolated(
                $@"SELECT * FROM [Compras] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}")
                .FirstOrDefaultAsync();
        }

        public async Task<Venta?> BloquearVentaAsync(int id)
        {
            RequerirTransaccion();
            return await _context.Ventas.FromSqlInterpolated(
                $@"SELECT * FROM [Ventas] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}")
                .FirstOrDefaultAsync();
        }

        public async Task<TurnoCaja?> BloquearTurnoAsync(int id)
        {
            RequerirTransaccion();
            return await _context.TurnosCaja.FromSqlInterpolated(
                $@"SELECT * FROM [TurnosCaja] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}")
                .FirstOrDefaultAsync();
        }

        public async Task<Pago?> BloquearPagoAsync(int id)
        {
            RequerirTransaccion();
            return await _context.Pagos.FromSqlInterpolated(
                $@"SELECT * FROM [Pagos] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}")
                .FirstOrDefaultAsync();
        }

        public Task<Pago?> ObtenerPagoParaAnularAsync(int id) =>
            _context.Pagos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

        public Task<MetodoPago?> ObtenerMetodoAsync(int id) =>
            _context.MetodosPago.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

        public Task<int?> ObtenerSucursalAlmacenAsync(int almacenId) =>
            _context.Almacenes.AsNoTracking().Where(a => a.Id == almacenId)
                .Select(a => (int?)a.SucursalId).FirstOrDefaultAsync();

        public async Task<decimal> TotalPagadoCompraAsync(int compraId) =>
            await _context.Pagos.AsNoTracking()
                .Where(p => p.CompraId == compraId && p.Estado == "Registrado")
                .SumAsync(p => (decimal?)p.Monto) ?? 0m;

        public async Task<decimal> TotalPagadoVentaAsync(int ventaId) =>
            await _context.Pagos.AsNoTracking()
                .Where(p => p.VentaId == ventaId && p.Estado == "Registrado")
                .SumAsync(p => (decimal?)p.Monto) ?? 0m;

        public async Task AgregarAsync(Pago pago)
        {
            ArgumentNullException.ThrowIfNull(pago);
            await _context.Pagos.AddAsync(pago);
        }

        public async Task EjecutarEnTransaccionAsync(Func<Task> operacion)
        {
            ArgumentNullException.ThrowIfNull(operacion);
            await using var t = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);
            try
            {
                await operacion();
                await _context.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch
            {
                await t.RollbackAsync();
                throw;
            }
        }
    }
}
