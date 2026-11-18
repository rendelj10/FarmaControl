
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> ObtenerTodasAsync()
        {
            return await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AgregarAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Categoria categoria)
        {
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteNombreAsync(
            string nombre, int? excluirId = null)
        {
            return await _context.Categorias.AnyAsync(c =>
                c.Nombre == nombre &&
                (!excluirId.HasValue || c.Id != excluirId.Value));
        }

        // =====================================
        // DATATABLES - PAGINACION Y BUSQUEDA
        // =====================================

        public async Task<DataTablesResponseDto<CategoriaTablaDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request)
        {
            var query = _context.Categorias
                .AsNoTracking()
                .AsQueryable();

            var recordsTotal = await query.CountAsync();

            // BUSQUEDA
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Nombre.Contains(search) ||
                    (c.Descripcion != null &&
                     c.Descripcion.Contains(search)));
            }

            var recordsFiltered = await query.CountAsync();

            // ORDENAMIENTO
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "asc";

            var columnName = request.Columns
                .ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            query = columnName.ToLowerInvariant() switch
            {
                "id" => orderDir == "desc"
                    ? query.OrderByDescending(c => c.Id)
                    : query.OrderBy(c => c.Id),

                "descripcion" => orderDir == "desc"
                    ? query.OrderByDescending(c => c.Descripcion)
                    : query.OrderBy(c => c.Descripcion),

                "activo" => orderDir == "desc"
                    ? query.OrderByDescending(c => c.Activo)
                    : query.OrderBy(c => c.Activo),

                _ => orderDir == "desc"
                    ? query.OrderByDescending(c => c.Nombre)
                    : query.OrderBy(c => c.Nombre)
            };

            // PAGINACION
            var start = Math.Max(0, request.Start);
            var length = request.Length > 0
                ? Math.Min(request.Length, 100)
                : 10;

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(c => new CategoriaTablaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Activo = c.Activo
                })
                .ToListAsync();

            return new DataTablesResponseDto<CategoriaTablaDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
