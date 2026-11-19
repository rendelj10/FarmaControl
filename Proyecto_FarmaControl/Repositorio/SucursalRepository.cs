
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class SucursalRepository : ISucursalRepository
    {
        private readonly AppDbContext _context;

        public SucursalRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // CONSULTAR TODAS LAS SUCURSALES
        // ========================================

        public async Task<List<Sucursal>> ObtenerTodasAsync()
        {
            return await _context.Sucursales
                .AsNoTracking()
                .OrderBy(s => s.Nombre)
                .ToListAsync();
        }

        // ========================================
        // CONSULTAR SUCURSAL POR ID
        // ========================================

        public async Task<Sucursal?> ObtenerPorIdAsync(int id)
        {
            return await _context.Sucursales
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // ========================================
        // AGREGAR SUCURSAL
        // ========================================

        public async Task AgregarAsync(Sucursal sucursal)
        {
            await _context.Sucursales.AddAsync(sucursal);

            await _context.SaveChangesAsync();
        }

        // ========================================
        // ACTUALIZAR SUCURSAL
        // ========================================

        public async Task ActualizarAsync(Sucursal sucursal)
        {
            // El servicio debe recuperar previamente
            // la entidad mediante ObtenerPorIdAsync.
            // Entity Framework rastrea sus cambios.

            await _context.SaveChangesAsync();
        }

        // ========================================
        // COMPROBAR CODIGO DUPLICADO
        // ========================================

        public async Task<bool> ExisteCodigoAsync(
            string codigo,
            int? excluirId = null)
        {
            var codigoNormalizado =
                codigo.Trim().ToUpperInvariant();

            return await _context.Sucursales
                .AsNoTracking()
                .AnyAsync(s =>
                    s.Codigo.Trim().ToUpper()
                        == codigoNormalizado &&
                    (!excluirId.HasValue ||
                     s.Id != excluirId.Value));
        }

        // ========================================
        // VERIFICAR ALMACENES RELACIONADOS
        // ========================================

        public async Task<bool> TieneAlmacenesAsync(
            int sucursalId)
        {
            return await _context.Almacenes
                .AsNoTracking()
                .AnyAsync(a => a.SucursalId == sucursalId);
        }

        // ========================================
        // DATATABLES - PAGINACION SQL SERVER
        // ========================================

        public async Task<DataTablesResponseDto<SucursalDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            var query = _context.Sucursales
                .AsNoTracking()
                .AsQueryable();

            // =====================================
            // TOTAL DE REGISTROS
            // =====================================

            int recordsTotal = await query.CountAsync();

            // =====================================
            // BUSQUEDA
            // =====================================

            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.Codigo.Contains(search) ||
                    s.Nombre.Contains(search) ||
                    (s.Direccion != null &&
                     s.Direccion.Contains(search)) ||
                    (s.Telefono != null &&
                     s.Telefono.Contains(search)) ||
                    (s.Email != null &&
                     s.Email.Contains(search)));
            }

            int recordsFiltered = await query.CountAsync();

            // =====================================
            // ORDENAMIENTO
            // =====================================

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
                    ? query.OrderByDescending(s => s.Id)
                    : query.OrderBy(s => s.Id),

                "codigo" => descendente
                    ? query.OrderByDescending(s => s.Codigo)
                    : query.OrderBy(s => s.Codigo),

                "direccion" => descendente
                    ? query.OrderByDescending(s => s.Direccion)
                    : query.OrderBy(s => s.Direccion),

                "telefono" => descendente
                    ? query.OrderByDescending(s => s.Telefono)
                    : query.OrderBy(s => s.Telefono),

                "email" => descendente
                    ? query.OrderByDescending(s => s.Email)
                    : query.OrderBy(s => s.Email),

                "activo" => descendente
                    ? query.OrderByDescending(s => s.Activo)
                    : query.OrderBy(s => s.Activo),

                _ => descendente
                    ? query.OrderByDescending(s => s.Nombre)
                    : query.OrderBy(s => s.Nombre)
            };

            // =====================================
            // PAGINACION
            // =====================================

            int start = Math.Max(0, request.Start);

            int length = request.Length == -1
                ? Math.Min(recordsFiltered, 500)
                : request.Length > 0
                    ? Math.Min(request.Length, 100)
                    : 10;

            // =====================================
            // PROYECCION A DTO
            // =====================================

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(s => new SucursalDto
                {
                    Id = s.Id,
                    Codigo = s.Codigo,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion,
                    Telefono = s.Telefono,
                    Email = s.Email,
                    Activo = s.Activo
                })
                .ToListAsync();

            return new DataTablesResponseDto<SucursalDto>
            {
                Draw = Math.Max(0, request.Draw),
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
