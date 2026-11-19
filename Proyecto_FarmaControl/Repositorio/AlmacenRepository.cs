
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class AlmacenRepository : IAlmacenRepository
    {
        private readonly AppDbContext _context;

        public AlmacenRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // 1. CONSULTAR TODOS LOS ALMACENES
        // ========================================

        public async Task<List<Almacen>> ObtenerTodosAsync()
        {
            return await _context.Almacenes
                .Include(a => a.Sucursal)
                .AsNoTracking()
                .OrderBy(a => a.Sucursal.Nombre)
                .ThenBy(a => a.Nombre)
                .ToListAsync();
        }

        // ========================================
        // 2. CONSULTAR ALMACEN POR ID
        // ========================================

        public async Task<Almacen?> ObtenerPorIdAsync(int id)
        {
            return await _context.Almacenes
                .Include(a => a.Sucursal)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // ========================================
        // 3. AGREGAR ALMACEN
        // ========================================

        public async Task AgregarAsync(Almacen almacen)
        {
            await _context.Almacenes.AddAsync(almacen);

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 4. ACTUALIZAR ALMACEN
        // ========================================

        public async Task ActualizarAsync(Almacen almacen)
        {
            // El servicio recupera previamente
            // la entidad con ObtenerPorIdAsync.
            // EF Core mantiene el seguimiento
            // de sus modificaciones.

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 5. COMPROBAR CODIGO DUPLICADO
        // ========================================

        public async Task<bool> ExisteCodigoAsync(
            int sucursalId,
            string codigo,
            int? excluirId = null)
        {
            var codigoNormalizado =
                codigo.Trim().ToUpperInvariant();

            return await _context.Almacenes
                .AsNoTracking()
                .AnyAsync(a =>
                    a.SucursalId == sucursalId &&
                    a.Codigo.Trim().ToUpper()
                        == codigoNormalizado &&
                    (!excluirId.HasValue ||
                     a.Id != excluirId.Value));
        }

        // ========================================
        // 6. COMPROBAR SUCURSAL ACTIVA
        // ========================================

        public async Task<bool> SucursalDisponibleAsync(
            int sucursalId)
        {
            return await _context.Sucursales
                .AsNoTracking()
                .AnyAsync(s =>
                    s.Id == sucursalId &&
                    s.Activo);
        }

        // ========================================
        // 7. VERIFICAR LOTES RELACIONADOS
        // ========================================

        public async Task<bool> TieneLotesAsync(
            int almacenId)
        {
            return await _context.Lotes
                .AsNoTracking()
                .AnyAsync(l => l.AlmacenId == almacenId);
        }

        // ========================================
        // 8. NUEVO:
        // VERIFICAR COMPRAS PENDIENTES
        // ========================================

        public async Task<bool> TieneComprasPendientesAsync(
            int almacenId)
        {
            if (almacenId <= 0)
            {
                return false;
            }

            // Solo las compras en Borrador
            // impiden desactivar o trasladar
            // el almacen.
            //
            // Las compras Recibidas y Anuladas
            // permanecen como historial.

            return await _context.Compras
                .AsNoTracking()
                .AnyAsync(c =>
                    c.AlmacenId == almacenId &&
                    c.Estado == "Borrador");
        }

        // ========================================
        // 9. DATATABLES - PAGINACION SQL
        // ========================================

        public async Task<DataTablesResponseDto<AlmacenDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _context.Almacenes
                .AsNoTracking()
                .AsQueryable();

            // TOTAL DE REGISTROS

            int recordsTotal = await query.CountAsync();

            // BUSQUEDA

            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.Codigo.Contains(search) ||
                    a.Nombre.Contains(search) ||
                    a.Sucursal.Nombre.Contains(search) ||
                    (a.Ubicacion != null &&
                     a.Ubicacion.Contains(search)));
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

            query = columna?.ToLowerInvariant() switch
            {
                "id" => descendente
                    ? query.OrderByDescending(a => a.Id)
                    : query.OrderBy(a => a.Id),

                "sucursalid" => descendente
                    ? query.OrderByDescending(a => a.SucursalId)
                    : query.OrderBy(a => a.SucursalId),

                "sucursalnombre" => descendente
                    ? query.OrderByDescending(
                        a => a.Sucursal.Nombre)
                    : query.OrderBy(
                        a => a.Sucursal.Nombre),

                "codigo" => descendente
                    ? query.OrderByDescending(a => a.Codigo)
                    : query.OrderBy(a => a.Codigo),

                "ubicacion" => descendente
                    ? query.OrderByDescending(a => a.Ubicacion)
                    : query.OrderBy(a => a.Ubicacion),

                "activo" => descendente
                    ? query.OrderByDescending(a => a.Activo)
                    : query.OrderBy(a => a.Activo),

                _ => descendente
                    ? query.OrderByDescending(a => a.Nombre)
                    : query.OrderBy(a => a.Nombre)
            };

            // PAGINACION

            int start = Math.Max(0, request.Start);

            int length = request.Length == -1
                ? Math.Min(recordsFiltered, 500)
                : request.Length > 0
                    ? Math.Min(request.Length, 100)
                    : 10;

            // PROYECCION A DTO

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(a => new AlmacenDto
                {
                    Id = a.Id,
                    SucursalId = a.SucursalId,
                    SucursalNombre = a.Sucursal.Nombre,
                    Codigo = a.Codigo,
                    Nombre = a.Nombre,
                    Ubicacion = a.Ubicacion,
                    Activo = a.Activo
                })
                .ToListAsync();

            return new DataTablesResponseDto<AlmacenDto>
            {
                Draw = Math.Max(0, request.Draw),
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
