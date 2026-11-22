using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class MetodoPagoRepository : IMetodoPagoRepository
    {
        private readonly AppDbContext _context;

        public MetodoPagoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MetodoPago>> ObtenerTodosAsync()
        {
            return await _context.MetodosPago
                .AsNoTracking()
                .OrderBy(m => m.Nombre)
                .ThenBy(m => m.Id)
                .ToListAsync();
        }

        public async Task<MetodoPago?> ObtenerPorIdAsync(int id)
        {
            return await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> ExisteNombreAsync(
            string nombre,
            int? excluirId = null)
        {
            return await _context.MetodosPago.AnyAsync(m =>
                m.Nombre == nombre &&
                (!excluirId.HasValue || m.Id != excluirId.Value));
        }

        public async Task AgregarAsync(MetodoPago metodoPago)
        {
            await _context.MetodosPago.AddAsync(metodoPago);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(MetodoPago metodoPago)
        {
            await _context.SaveChangesAsync();
        }

        public async Task<DataTablesResponseDto<MetodoPagoDto>> ObtenerPaginadosAsync(
            DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _context.MetodosPago
                .AsNoTracking()
                .AsQueryable();

            int recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m =>
                    m.Nombre.Contains(search) ||
                    (m.Descripcion != null && m.Descripcion.Contains(search)));
            }

            int recordsFiltered = await query.CountAsync();
            var order = request.Order?.FirstOrDefault();
            int columnIndex = order?.Column ?? -1;
            string? column = columnIndex >= 0
                ? request.Columns?.ElementAtOrDefault(columnIndex)?.Data
                : null;
            bool descending = string.Equals(
                order?.Dir,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            query = column?.ToLowerInvariant() switch
            {
                "id" => descending
                    ? query.OrderByDescending(m => m.Id)
                    : query.OrderBy(m => m.Id),
                "descripcion" => descending
                    ? query.OrderByDescending(m => m.Descripcion)
                    : query.OrderBy(m => m.Descripcion),
                "requiereReferencia" => descending
                    ? query.OrderByDescending(m => m.RequiereReferencia)
                    : query.OrderBy(m => m.RequiereReferencia),
                "activo" => descending
                    ? query.OrderByDescending(m => m.Activo)
                    : query.OrderBy(m => m.Activo),
                _ => descending
                    ? query.OrderByDescending(m => m.Nombre)
                    : query.OrderBy(m => m.Nombre)
            };

            query = ((IOrderedQueryable<MetodoPago>)query).ThenBy(m => m.Id);

            int start = Math.Max(0, request.Start);
            int length = request.Length == -1
                ? Math.Min(recordsFiltered, 500)
                : request.Length > 0
                    ? Math.Min(request.Length, 100)
                    : 10;

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(m => new MetodoPagoDto
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Descripcion = m.Descripcion,
                    RequiereReferencia = m.RequiereReferencia,
                    Activo = m.Activo
                })
                .ToListAsync();

            return new DataTablesResponseDto<MetodoPagoDto>
            {
                Draw = Math.Max(0, request.Draw),
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
