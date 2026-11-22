using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class PresentacionRepository : IPresentacionRepository
    {
        private readonly AppDbContext _context;

        public PresentacionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Presentacion>> ObtenerTodasAsync()
        {
            return await _context.Presentaciones
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.Id)
                .ToListAsync();
        }

        public async Task<Presentacion?> ObtenerPorIdAsync(int id)
        {
            return await _context.Presentaciones
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AgregarAsync(Presentacion presentacion)
        {
            await _context.Presentaciones.AddAsync(presentacion);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Presentacion presentacion)
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null)
        {
            string normalizado = nombre.Trim().ToUpper();

            return await _context.Presentaciones
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Nombre.ToUpper() == normalizado &&
                    (!excluirId.HasValue || p.Id != excluirId.Value));
        }

        public async Task<DataTablesResponseDto<PresentacionDto>> ObtenerPaginadasAsync(
            DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _context.Presentaciones
                .AsNoTracking()
                .AsQueryable();

            int recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(search) ||
                    (p.Descripcion != null && p.Descripcion.Contains(search)) ||
                    (p.UnidadContenido != null && p.UnidadContenido.Contains(search)));
            }

            int recordsFiltered = await query.CountAsync();
            var order = request.Order?.FirstOrDefault();
            int columnIndex = order?.Column ?? -1;
            string? column = columnIndex >= 0
                ? request.Columns?.ElementAtOrDefault(columnIndex)?.Data
                : null;
            bool descending = string.Equals(order?.Dir, "desc", StringComparison.OrdinalIgnoreCase);

            query = column?.ToLowerInvariant() switch
            {
                "id" => descending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
                "descripcion" => descending ? query.OrderByDescending(p => p.Descripcion) : query.OrderBy(p => p.Descripcion),
                "cantidadcontenido" => descending ? query.OrderByDescending(p => p.CantidadContenido) : query.OrderBy(p => p.CantidadContenido),
                "unidadcontenido" => descending ? query.OrderByDescending(p => p.UnidadContenido) : query.OrderBy(p => p.UnidadContenido),
                "activo" => descending ? query.OrderByDescending(p => p.Activo) : query.OrderBy(p => p.Activo),
                _ => descending ? query.OrderByDescending(p => p.Nombre) : query.OrderBy(p => p.Nombre)
            };

            query = ((IOrderedQueryable<Presentacion>)query).ThenBy(p => p.Id);

            int start = Math.Max(0, request.Start);
            int length = request.Length == -1
                ? Math.Min(recordsFiltered, 500)
                : request.Length > 0 ? Math.Min(request.Length, 100) : 10;

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(p => new PresentacionDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    CantidadContenido = p.CantidadContenido,
                    UnidadContenido = p.UnidadContenido,
                    Activo = p.Activo
                })
                .ToListAsync();

            return new DataTablesResponseDto<PresentacionDto>
            {
                Draw = Math.Max(0, request.Draw),
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
