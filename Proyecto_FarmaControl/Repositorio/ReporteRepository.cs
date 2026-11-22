using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Repositorio
{
    // Consultas de solo lectura: no modifican existencias ni estados.
    public class ReporteRepository : IReporteRepository
    {
        private readonly AppDbContext _db;
        public ReporteRepository(AppDbContext db) => _db = db;

        public async Task<ReporteResultadoDto> VentasAsync(DateTime? desde, DateTime? hasta, int limite)
        {
            var q = _db.Ventas.AsNoTracking().AsQueryable();
            if (desde.HasValue) q = q.Where(x => x.FechaVenta >= desde.Value.Date);
            if (hasta.HasValue) q = q.Where(x => x.FechaVenta < hasta.Value.Date.AddDays(1));
            return new ReporteResultadoDto
            {
                Limite = limite,
                TotalRegistros = await q.CountAsync(),
                // Solo las ventas confirmadas representan operaciones efectivas.
                TotalMonetario = await q.Where(x => x.Estado == "Confirmada")
                    .SumAsync(x => (decimal?)x.Total) ?? 0m,
                Filas = await q.OrderByDescending(x => x.FechaVenta).ThenByDescending(x => x.Id)
                    .Take(limite).Select(x => new ReporteFilaDto
                    {
                        Id = x.Id, Fecha = x.FechaVenta,
                        Referencia = x.NumeroDocumento,
                        Relacionado = x.Cliente.Nombre,
                        Descripcion = x.Almacen.Nombre,
                        Estado = x.Estado, Monto = x.Total
                    }).ToListAsync()
            };
        }

        public async Task<ReporteResultadoDto> ComprasAsync(DateTime? desde, DateTime? hasta, int limite)
        {
            var q = _db.Compras.AsNoTracking().AsQueryable();
            if (desde.HasValue) q = q.Where(x => x.FechaCompra >= desde.Value.Date);
            if (hasta.HasValue) q = q.Where(x => x.FechaCompra < hasta.Value.Date.AddDays(1));
            return new ReporteResultadoDto
            {
                Limite = limite,
                TotalRegistros = await q.CountAsync(),
                TotalMonetario = await q.Where(x => x.Estado == "Recibida")
                    .SumAsync(x => (decimal?)x.Total) ?? 0m,
                Filas = await q.OrderByDescending(x => x.FechaCompra).ThenByDescending(x => x.Id)
                    .Take(limite).Select(x => new ReporteFilaDto
                    {
                        Id = x.Id, Fecha = x.FechaCompra,
                        Referencia = x.NumeroDocumento,
                        Relacionado = x.Proveedor.RazonSocial,
                        Descripcion = x.Almacen.Nombre,
                        Estado = x.Estado, Monto = x.Total
                    }).ToListAsync()
            };
        }

        public async Task<ReporteResultadoDto> PagosAsync(DateTime? desde, DateTime? hasta, int limite)
        {
            var q = _db.Pagos.AsNoTracking().AsQueryable();
            if (desde.HasValue) q = q.Where(x => x.FechaPago >= desde.Value.Date);
            if (hasta.HasValue) q = q.Where(x => x.FechaPago < hasta.Value.Date.AddDays(1));
            return new ReporteResultadoDto
            {
                Limite = limite,
                TotalRegistros = await q.CountAsync(),
                TotalMonetario = await q.Where(x => x.Estado == "Registrado")
                    .SumAsync(x => (decimal?)x.Monto) ?? 0m,
                Filas = await q.OrderByDescending(x => x.FechaPago).ThenByDescending(x => x.Id)
                    .Take(limite).Select(x => new ReporteFilaDto
                    {
                        Id = x.Id, Fecha = x.FechaPago,
                        Referencia = x.Referencia ?? (x.VentaId != null ? "Venta" : "Compra"),
                        Relacionado = x.MetodoPago.Nombre,
                        Descripcion = x.VentaId != null ? "Venta" : "Compra",
                        Estado = x.Estado, Monto = x.Monto
                    }).ToListAsync()
            };
        }

        public async Task<ReporteResultadoDto> LotesAsync(int limite)
        {
            var q = _db.Lotes.AsNoTracking();
            return new ReporteResultadoDto
            {
                Limite = limite,
                TotalRegistros = await q.CountAsync(),
                TotalCantidad = await q.Where(x => x.Activo)
                    .SumAsync(x => (int?)x.Existencias) ?? 0,
                Filas = await q.OrderBy(x => x.FechaVencimiento).ThenBy(x => x.Id)
                    .Take(limite).Select(x => new ReporteFilaDto
                    {
                        Id = x.Id, Fecha = x.FechaIngreso,
                        Referencia = x.NumeroLote,
                        Relacionado = x.Medicamento.Nombre,
                        Descripcion = x.Almacen.Nombre,
                        Estado = x.Activo ? "Activo" : "Inactivo",
                        Cantidad = x.Existencias,
                        Vencimiento = x.FechaVencimiento
                    }).ToListAsync()
            };
        }

        public async Task<ReporteResultadoDto> MovimientosAsync(DateTime? desde, DateTime? hasta, int limite)
        {
            var q = _db.MovimientosInventario.AsNoTracking().AsQueryable();
            if (desde.HasValue) q = q.Where(x => x.FechaMovimiento >= desde.Value.Date);
            if (hasta.HasValue) q = q.Where(x => x.FechaMovimiento < hasta.Value.Date.AddDays(1));
            return new ReporteResultadoDto
            {
                Limite = limite,
                TotalRegistros = await q.CountAsync(),
                // Es una suma de unidades involucradas, no el stock actual.
                TotalCantidad = await q.SelectMany(x => x.Detalles)
                    .SumAsync(x => (int?)x.Cantidad) ?? 0,
                Filas = await q.OrderByDescending(x => x.FechaMovimiento).ThenByDescending(x => x.Id)
                    .Take(limite).Select(x => new ReporteFilaDto
                    {
                        Id = x.Id, Fecha = x.FechaMovimiento,
                        Referencia = x.Referencia ?? "",
                        Relacionado = x.TipoMovimiento,
                        Descripcion = (x.AlmacenOrigen != null ? x.AlmacenOrigen.Nombre : "—")
                            + " → " + (x.AlmacenDestino != null ? x.AlmacenDestino.Nombre : "—"),
                        Estado = x.Estado,
                        Cantidad = x.Detalles.Sum(d => (int?)d.Cantidad) ?? 0
                    }).ToListAsync()
            };
        }
    }
}
