
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Repositorio
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        // =====================================
        // OBTENER TODAS LAS CATEGORIAS
        // =====================================

        public async Task<IReadOnlyList<Categoria>> ObtenerTodasAsync()
        {
            return await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        // =====================================
        // OBTENER CATEGORIA POR ID
        // =====================================

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // =====================================
        // AGREGAR CATEGORIA
        // =====================================

        public async Task<Categoria> AgregarAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);

            await _context.SaveChangesAsync();

            return categoria;
        }

        // =====================================
        // ACTUALIZAR CATEGORIA
        // =====================================

        public async Task<bool> ActualizarAsync(Categoria categoria)
        {
            _context.Categorias.Update(categoria);

            var filasAfectadas = await _context.SaveChangesAsync();

            return filasAfectadas > 0;
        }

        // =====================================
        // VERIFICAR NOMBRE DUPLICADO
        // =====================================

        public async Task<bool> ExisteNombreAsync(
            string nombre,
            int? excluirId = null)
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

            // TOTAL DE REGISTROS
            var recordsTotal = await query.CountAsync();

            // =====================================
            // BUSQUEDA
            // =====================================

            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Nombre.Contains(search) ||
                    (c.Descripcion != null &&
                     c.Descripcion.Contains(search)));
            }

            var recordsFiltered = await query.CountAsync();

            // =====================================
            // ORDENAMIENTO
            // =====================================

            var order = request.Order?.FirstOrDefault();

            var orderColumn = order?.Column ?? 0;

            var orderDir = order?.Dir ?? "asc";

            var descending = string.Equals(
                orderDir,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            var columnName = request.Columns?
                .ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            query = columnName.ToLowerInvariant() switch
            {
                "id" => descending
                    ? query.OrderByDescending(c => c.Id)
                    : query.OrderBy(c => c.Id),

                "descripcion" => descending
                    ? query.OrderByDescending(c => c.Descripcion)
                    : query.OrderBy(c => c.Descripcion),

                "activo" => descending
                    ? query.OrderByDescending(c => c.Activo)
                    : query.OrderBy(c => c.Activo),

                _ => descending
                    ? query.OrderByDescending(c => c.Nombre)
                    : query.OrderBy(c => c.Nombre)
            };

            // =====================================
            // PAGINACION
            // =====================================

            var start = Math.Max(0, request.Start);

            var length = request.Length > 0
                ? Math.Min(request.Length, 100)
                : 10;

            // =====================================
            // PROYECCION A DTO
            // =====================================

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

            // =====================================
            // RESPUESTA DATATABLES
            // =====================================

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
