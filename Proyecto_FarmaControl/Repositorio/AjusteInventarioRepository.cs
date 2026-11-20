
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class AjusteInventarioRepository
        : IAjusteInventarioRepository
    {
        private readonly AppDbContext _context;

        public AjusteInventarioRepository(
            AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // 1. CONSULTAR TODOS LOS AJUSTES
        // ========================================

        public async Task<List<AjusteInventario>>
            ObtenerTodosAsync()
        {
            return await _context
                .Set<AjusteInventario>()
                .Include(a => a.Almacen)
                .Include(a => a.Usuario)
                .Include(a => a.ConfirmadoPorUsuario)
                .Include(a => a.MovimientoInventario)
                .Include(a => a.Detalles)
                    .ThenInclude(d => d.Lote)
                        .ThenInclude(l => l.Medicamento)
                .AsNoTracking()
                .AsSplitQuery()
                .OrderByDescending(a => a.FechaAjuste)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }

        // ========================================
        // 2. CONSULTAR AJUSTE POR ID
        // ========================================

        public async Task<AjusteInventario?>
            ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _context
                .Set<AjusteInventario>()
                .Include(a => a.Almacen)
                .Include(a => a.Usuario)
                .Include(a => a.ConfirmadoPorUsuario)
                .Include(a => a.MovimientoInventario)
                .Include(a => a.Detalles)
                    .ThenInclude(d => d.Lote)
                        .ThenInclude(l => l.Medicamento)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // ========================================
        // 3. CONSULTAR AJUSTES PAGINADOS
        // ========================================

        public async Task<
            DataTablesResponseDto<AjusteInventarioDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _context
                .Set<AjusteInventario>()
                .AsNoTracking()
                .AsQueryable();

            int recordsTotal = await query.CountAsync();

            // BUSQUEDA

            string? search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                bool esId = int.TryParse(
                    search, out int idBuscado);

                query = query.Where(a =>
                    (esId && a.Id == idBuscado) ||
                    a.TipoAjuste.Contains(search) ||
                    a.Estado.Contains(search) ||
                    a.Motivo.Contains(search) ||
                    a.Almacen.Nombre.Contains(search) ||
                    a.Usuario.UserName.Contains(search) ||
                    (a.Observaciones != null &&
                     a.Observaciones.Contains(search)));
            }

            int recordsFiltered = await query.CountAsync();

            // ORDENAMIENTO

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

            IOrderedQueryable<AjusteInventario> ordenada =
                columna?.ToLowerInvariant() switch
                {
                    "id" => descendente
                        ? query.OrderByDescending(a => a.Id)
                        : query.OrderBy(a => a.Id),

                    "almacennombre" => descendente
                        ? query.OrderByDescending(
                            a => a.Almacen.Nombre)
                        : query.OrderBy(
                            a => a.Almacen.Nombre),

                    "usuarionombre" => descendente
                        ? query.OrderByDescending(
                            a => a.Usuario.UserName)
                        : query.OrderBy(
                            a => a.Usuario.UserName),

                    "fechaajuste" => descendente
                        ? query.OrderByDescending(
                            a => a.FechaAjuste)
                        : query.OrderBy(
                            a => a.FechaAjuste),

                    "fechaconfirmacion" => descendente
                        ? query.OrderByDescending(
                            a => a.FechaConfirmacion)
                        : query.OrderBy(
                            a => a.FechaConfirmacion),

                    "tipoajuste" => descendente
                        ? query.OrderByDescending(
                            a => a.TipoAjuste)
                        : query.OrderBy(
                            a => a.TipoAjuste),

                    "estado" => descendente
                        ? query.OrderByDescending(a => a.Estado)
                        : query.OrderBy(a => a.Estado),

                    "motivo" => descendente
                        ? query.OrderByDescending(a => a.Motivo)
                        : query.OrderBy(a => a.Motivo),

                    _ => query.OrderByDescending(
                        a => a.FechaAjuste)
                };

            ordenada = ordenada.ThenBy(a => a.Id);

            // PAGINACION

            int start = Math.Max(0, request.Start);

            int length = request.Length == -1
                ? Math.Min(recordsFiltered, 500)
                : request.Length > 0
                    ? Math.Min(request.Length, 100)
                    : 10;

            // CONVERTIR A DTO

            var data = await ordenada
                .Skip(start)
                .Take(length)
                .Select(a => new AjusteInventarioDto
                {
                    Id = a.Id,

                    AlmacenId = a.AlmacenId,

                    AlmacenNombre = a.Almacen.Nombre,

                    UsuarioId = a.UsuarioId,

                    UsuarioNombre = a.Usuario.UserName,

                    ConfirmadoPorUsuarioId =
                        a.ConfirmadoPorUsuarioId,

                    ConfirmadoPorUsuarioNombre =
                        a.ConfirmadoPorUsuario == null
                            ? null
                            : a.ConfirmadoPorUsuario.UserName,

                    FechaAjuste = a.FechaAjuste,

                    FechaConfirmacion =
                        a.FechaConfirmacion,

                    TipoAjuste = a.TipoAjuste,

                    Estado = a.Estado,

                    Motivo = a.Motivo,

                    Observaciones = a.Observaciones,

                    MovimientoInventarioId =
                        a.MovimientoInventario == null
                            ? (int?)null
                            : a.MovimientoInventario.Id
                })
                .ToListAsync();

            return new DataTablesResponseDto<AjusteInventarioDto>
            {
                Draw = Math.Max(0, request.Draw),

                RecordsTotal = recordsTotal,

                RecordsFiltered = recordsFiltered,

                Data = data
            };
        }

        // ========================================
        // 4. OBTENER AJUSTE PARA PROCESAR
        // ========================================

        public async Task<AjusteInventario?>
            ObtenerParaProcesarAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            if (_context.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException(
                    "Se requiere una transacción activa " +
                    "para procesar el ajuste.");
            }

            return await _context
                .Set<AjusteInventario>()
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM [AjustesInventario]
                       WITH (UPDLOCK, HOLDLOCK)
                       WHERE [Id] = {id}")
                .Include(a => a.Detalles)
                .FirstOrDefaultAsync();
        }

        // ========================================
        // 5. REGISTRAR AJUSTE EN BORRADOR
        // ========================================

        public async Task AgregarAsync(
            AjusteInventario ajuste)
        {
            ArgumentNullException.ThrowIfNull(ajuste);

            await _context
                .Set<AjusteInventario>()
                .AddAsync(ajuste);

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 6. REGISTRAR MOVIMIENTO DE INVENTARIO
        // ========================================

        public async Task AgregarMovimientoAsync(
            MovimientoInventario movimiento)
        {
            ArgumentNullException.ThrowIfNull(movimiento);

            await _context
                .Set<MovimientoInventario>()
                .AddAsync(movimiento);

            // Se guardará dentro de la transacción
            // al confirmar el ajuste.
        }

        // ========================================
        // 7. VALIDAR ALMACEN DISPONIBLE
        // ========================================

        public async Task<bool> AlmacenDisponibleAsync(
            int almacenId)
        {
            if (almacenId <= 0)
            {
                return false;
            }

            return await _context
                .Set<Almacen>()
                .AsNoTracking()
                .AnyAsync(a =>
                    a.Id == almacenId &&
                    a.Activo &&
                    a.Sucursal.Activo);
        }

        // ========================================
        // 8. CONSULTAR LOTE
        // ========================================

        public async Task<Lote?>
            ObtenerLoteParaConsultaAsync(int loteId)
        {
            if (loteId <= 0)
            {
                return null;
            }

            return await _context
                .Set<Lote>()
                .Include(l => l.Medicamento)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == loteId);
        }

        // ========================================
        // 9. OBTENER LOTE PARA AJUSTAR
        // ========================================

        public async Task<Lote?>
            ObtenerLoteParaAjustarAsync(int loteId)
        {
            if (loteId <= 0)
            {
                return null;
            }

            if (_context.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException(
                    "El ajuste de existencias requiere " +
                    "una transacción activa.");
            }

            return await _context
                .Set<Lote>()
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM [Lotes]
                       WITH (UPDLOCK, HOLDLOCK)
                       WHERE [Id] = {loteId}")
                .Include(l => l.Medicamento)
                .FirstOrDefaultAsync();
        }

        // ========================================
        // 10. EJECUTAR TRANSACCION SQL
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
    }
}
