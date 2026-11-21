using System.Data;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class TurnoCajaRepository : ITurnoCajaRepository
    {
        private readonly AppDbContext _context;

        public TurnoCajaRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<TurnoCajaDto>> ObtenerTodosAsync()
        {
            return ConsultarTurnos()
                .OrderByDescending(t => t.FechaApertura)
                .ThenByDescending(t => t.Id)
                .Select(t => new TurnoCajaDto
                {
                    Id = t.Id,
                    SucursalId = t.SucursalId,
                    SucursalNombre = t.Sucursal.Nombre,
                    UsuarioAperturaId = t.UsuarioAperturaId,
                    UsuarioAperturaNombre = t.UsuarioApertura.UserName,
                    UsuarioCierreId = t.UsuarioCierreId,
                    UsuarioCierreNombre = t.UsuarioCierre == null
                        ? null : t.UsuarioCierre.UserName,
                    FechaApertura = t.FechaApertura,
                    FechaCierre = t.FechaCierre,
                    MontoInicial = t.MontoInicial,
                    MontoEsperado = t.MontoEsperado,
                    MontoContado = t.MontoContado,
                    Diferencia = t.Diferencia,
                    Estado = t.Estado,
                    Observaciones = t.Observaciones
                }).ToListAsync();
        }

        public Task<TurnoCajaDto?> ObtenerPorIdAsync(int id)
        {
            return ConsultarTurnos()
                .Where(t => t.Id == id)
                .Select(t => new TurnoCajaDto
                {
                    Id = t.Id,
                    SucursalId = t.SucursalId,
                    SucursalNombre = t.Sucursal.Nombre,
                    UsuarioAperturaId = t.UsuarioAperturaId,
                    UsuarioAperturaNombre = t.UsuarioApertura.UserName,
                    UsuarioCierreId = t.UsuarioCierreId,
                    UsuarioCierreNombre = t.UsuarioCierre == null
                        ? null : t.UsuarioCierre.UserName,
                    FechaApertura = t.FechaApertura,
                    FechaCierre = t.FechaCierre,
                    MontoInicial = t.MontoInicial,
                    MontoEsperado = t.MontoEsperado,
                    MontoContado = t.MontoContado,
                    Diferencia = t.Diferencia,
                    Estado = t.Estado,
                    Observaciones = t.Observaciones
                })
                .FirstOrDefaultAsync();
        }

        public Task<TurnoCajaDto?> ObtenerAbiertoPorUsuarioAsync(int usuarioId)
        {
            return ConsultarTurnos()
                .Where(t => t.UsuarioAperturaId == usuarioId && t.Estado == "Abierto")
                .Select(t => new TurnoCajaDto
                {
                    Id = t.Id,
                    SucursalId = t.SucursalId,
                    SucursalNombre = t.Sucursal.Nombre,
                    UsuarioAperturaId = t.UsuarioAperturaId,
                    UsuarioAperturaNombre = t.UsuarioApertura.UserName,
                    UsuarioCierreId = t.UsuarioCierreId,
                    UsuarioCierreNombre = t.UsuarioCierre == null
                        ? null : t.UsuarioCierre.UserName,
                    FechaApertura = t.FechaApertura,
                    FechaCierre = t.FechaCierre,
                    MontoInicial = t.MontoInicial,
                    MontoEsperado = t.MontoEsperado,
                    MontoContado = t.MontoContado,
                    Diferencia = t.Diferencia,
                    Estado = t.Estado,
                    Observaciones = t.Observaciones
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DataTablesResponseDto<TurnoCajaDto>> ObtenerPaginadasAsync(
            DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);
            IQueryable<TurnoCaja> query = ConsultarTurnos();
            int total = await query.CountAsync();

            string? buscar = request.Search?.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                bool esId = int.TryParse(buscar, out int idBuscado);
                query = query.Where(t =>
                    (esId && t.Id == idBuscado) ||
                    t.Estado.Contains(buscar) ||
                    t.Sucursal.Nombre.Contains(buscar) ||
                    t.UsuarioApertura.UserName.Contains(buscar) ||
                    (t.Observaciones != null && t.Observaciones.Contains(buscar)));
            }

            int filtrados = await query.CountAsync();
            var orden = request.Order?.FirstOrDefault();
            int indice = orden?.Column ?? -1;
            string? columna = indice >= 0
                ? request.Columns?.ElementAtOrDefault(indice)?.Data
                : null;
            bool descendente = string.Equals(
                orden?.Dir, "desc", StringComparison.OrdinalIgnoreCase);

            IOrderedQueryable<TurnoCaja> ordenados = columna?.ToLowerInvariant() switch
            {
                "id" => descendente ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id),
                "sucursalnombre" => descendente ? query.OrderByDescending(t => t.Sucursal.Nombre) : query.OrderBy(t => t.Sucursal.Nombre),
                "usuarioaperturanombre" => descendente ? query.OrderByDescending(t => t.UsuarioApertura.UserName) : query.OrderBy(t => t.UsuarioApertura.UserName),
                "fechaapertura" => descendente ? query.OrderByDescending(t => t.FechaApertura) : query.OrderBy(t => t.FechaApertura),
                "fechacierre" => descendente ? query.OrderByDescending(t => t.FechaCierre) : query.OrderBy(t => t.FechaCierre),
                "montoinicial" => descendente ? query.OrderByDescending(t => t.MontoInicial) : query.OrderBy(t => t.MontoInicial),
                "estado" => descendente ? query.OrderByDescending(t => t.Estado) : query.OrderBy(t => t.Estado),
                _ => query.OrderByDescending(t => t.FechaApertura)
            };

            int inicio = Math.Max(0, request.Start);
            int cantidad = request.Length == -1
                ? Math.Min(filtrados, 500)
                : request.Length > 0 ? Math.Min(request.Length, 100) : 10;

            var datos = await ordenados.ThenBy(t => t.Id)
                .Skip(inicio)
                .Take(cantidad)
                .Select(t => new TurnoCajaDto
                {
                    Id = t.Id,
                    SucursalId = t.SucursalId,
                    SucursalNombre = t.Sucursal.Nombre,
                    UsuarioAperturaId = t.UsuarioAperturaId,
                    UsuarioAperturaNombre = t.UsuarioApertura.UserName,
                    UsuarioCierreId = t.UsuarioCierreId,
                    UsuarioCierreNombre = t.UsuarioCierre == null
                        ? null : t.UsuarioCierre.UserName,
                    FechaApertura = t.FechaApertura,
                    FechaCierre = t.FechaCierre,
                    MontoInicial = t.MontoInicial,
                    MontoEsperado = t.MontoEsperado,
                    MontoContado = t.MontoContado,
                    Diferencia = t.Diferencia,
                    Estado = t.Estado,
                    Observaciones = t.Observaciones
                }).ToListAsync();

            return new DataTablesResponseDto<TurnoCajaDto>
            {
                Draw = Math.Max(0, request.Draw),
                RecordsTotal = total,
                RecordsFiltered = filtrados,
                Data = datos
            };
        }

        public Task<bool> SucursalActivaAsync(int sucursalId)
        {
            return _context.Sucursales.AsNoTracking()
                .AnyAsync(s => s.Id == sucursalId && s.Activo);
        }

        public Task<bool> UsuarioExisteAsync(int usuarioId)
        {
            return _context.Usuarios.AsNoTracking()
                .AnyAsync(u => u.Id == usuarioId);
        }

        public Task<bool> ExisteTurnoAbiertoAsync(int usuarioId)
        {
            return _context.TurnosCaja.AsNoTracking()
                .AnyAsync(t => t.UsuarioAperturaId == usuarioId && t.Estado == "Abierto");
        }

        // El bloqueo impide que se registren o anulen pagos del turno
        // mientras se calcula y confirma su cierre.
        public async Task<TurnoCaja?> ObtenerParaCerrarAsync(int id)
        {
            if (id <= 0)
                return null;

            if (_context.Database.CurrentTransaction == null)
                throw new InvalidOperationException(
                    "El cierre de caja requiere una transacción activa.");

            return await _context.TurnosCaja
                .FromSqlInterpolated(
                    $@"SELECT * FROM [TurnosCaja] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}")
                .FirstOrDefaultAsync();
        }

        // Solo cuentan pagos vigentes en efectivo asociados al turno.
        // Ventas son ingresos; compras son salidas de caja.
        public async Task<(decimal Ingresos, decimal Egresos)>
            ObtenerTotalesEfectivoAsync(int turnoId)
        {
            if (_context.Database.CurrentTransaction == null)
                throw new InvalidOperationException(
                    "El arqueo requiere una transacción activa.");

            var pagosEfectivo = _context.Pagos.AsNoTracking()
                .Where(p => p.TurnoCajaId == turnoId &&
                    p.Estado == "Registrado" &&
                    p.MetodoPago.Nombre.Trim().ToLower() == "efectivo");

            decimal ingresos = await pagosEfectivo
                .Where(p => p.VentaId != null)
                .SumAsync(p => (decimal?)p.Monto) ?? 0m;

            decimal egresos = await pagosEfectivo
                .Where(p => p.CompraId != null)
                .SumAsync(p => (decimal?)p.Monto) ?? 0m;

            return (ingresos, egresos);
        }

        public async Task AgregarAsync(TurnoCaja turno)
        {
            ArgumentNullException.ThrowIfNull(turno);
            await _context.TurnosCaja.AddAsync(turno);
        }

        public async Task EjecutarEnTransaccionAsync(Func<Task> operacion)
        {
            ArgumentNullException.ThrowIfNull(operacion);
            await using var transaccion = await _context.Database.BeginTransactionAsync(
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

        private IQueryable<TurnoCaja> ConsultarTurnos()
        {
            return _context.TurnosCaja.AsNoTracking();
        }
    }
}
