
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class MedicamentoRepository : IMedicamentoRepository
    {
        private readonly AppDbContext _context;

        public MedicamentoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Medicamento>>
            ObtenerTodasAsync()
        {
            return await _context.Medicamentos
                .Include(m => m.Categoria)
                .Include(m => m.Laboratorio)
                .Include(m => m.PresentacionCatalogo)
                .AsNoTracking()
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<Medicamento?> ObtenerPorIdAsync(int id)
        {
            return await _context.Medicamentos
                .Include(m => m.Categoria)
                .Include(m => m.Laboratorio)
                .Include(m => m.PresentacionCatalogo)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Medicamento> AgregarAsync(
            Medicamento medicamento)
        {
            var resultado = await _context.Medicamentos
                .AddAsync(medicamento);

            await _context.SaveChangesAsync();

            return resultado.Entity;
        }

        public async Task<bool> ActualizarAsync(
            Medicamento medicamento)
        {
            _context.Medicamentos.Update(medicamento);

            var filasAfectadas =
                await _context.SaveChangesAsync();

            return filasAfectadas > 0;
        }

        public async Task<bool> ExisteCodigoAsync(
            string codigo, int? excluirId = null)
        {
            return await _context.Medicamentos.AnyAsync(m =>
                m.Codigo == codigo &&
                (!excluirId.HasValue ||
                 m.Id != excluirId.Value));
        }

        // =====================================
        // DATATABLES - PAGINACION Y BUSQUEDA
        // =====================================

        public async Task<DataTablesResponseDto<MedicamentoDto>>
            ObtenerPaginadosAsync(DataTablesRequestDto request)
        {
            var query = _context.Medicamentos
                .Include(m => m.Categoria)
                .Include(m => m.Laboratorio)
                .Include(m => m.PresentacionCatalogo)
                .AsNoTracking()
                .AsQueryable();

            var recordsTotal = await query.CountAsync();

            // BUSQUEDA
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m =>
                    m.Codigo.Contains(search) ||
                    m.Nombre.Contains(search) ||
                    (m.PrincipioActivo != null &&
                     m.PrincipioActivo.Contains(search)) ||
                    (m.Concentracion != null &&
                     m.Concentracion.Contains(search)) ||
                    (m.FormaFarmaceutica != null &&
                     m.FormaFarmaceutica.Contains(search)) ||
                    (m.Presentacion != null &&
                     m.Presentacion.Contains(search)) ||
                    (m.Laboratorio != null &&
                     m.Laboratorio.Nombre.Contains(search)) ||
                    (m.PresentacionCatalogo != null &&
                     m.PresentacionCatalogo.Nombre.Contains(search)) ||
                    m.Categoria.Nombre.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();

            // ORDENAMIENTO
            var order =
                request.Order.FirstOrDefault();

            var orderColumn = order?.Column ?? 0;

            var orderDir = order?.Dir ?? "asc";

            var columnName = request.Columns
                .ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            var descending = string.Equals(
                orderDir,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            query = columnName.ToLowerInvariant() switch
            {
                "id" => descending
                    ? query.OrderByDescending(m => m.Id)
                    : query.OrderBy(m => m.Id),

                "codigo" => descending
                    ? query.OrderByDescending(m => m.Codigo)
                    : query.OrderBy(m => m.Codigo),

                "nombre" => descending
                    ? query.OrderByDescending(m => m.Nombre)
                    : query.OrderBy(m => m.Nombre),

                "principioactivo" => descending
                    ? query.OrderByDescending(m => m.PrincipioActivo)
                    : query.OrderBy(m => m.PrincipioActivo),

                "concentracion" => descending
                    ? query.OrderByDescending(m => m.Concentracion)
                    : query.OrderBy(m => m.Concentracion),

                "formafarmaceutica" => descending
                    ? query.OrderByDescending(m => m.FormaFarmaceutica)
                    : query.OrderBy(m => m.FormaFarmaceutica),

                "presentacion" => descending
                    ? query.OrderByDescending(m => m.Presentacion)
                    : query.OrderBy(m => m.Presentacion),

                "laboratorionombre" => descending
                    ? query.OrderByDescending(m => m.Laboratorio!.Nombre)
                    : query.OrderBy(m => m.Laboratorio!.Nombre),

                "presentacionnombre" => descending
                    ? query.OrderByDescending(m => m.PresentacionCatalogo!.Nombre)
                    : query.OrderBy(m => m.PresentacionCatalogo!.Nombre),

                "precioventa" => descending
                    ? query.OrderByDescending(m => m.PrecioVenta)
                    : query.OrderBy(m => m.PrecioVenta),

                "categorianombre" => descending
                    ? query.OrderByDescending(m => m.Categoria.Nombre)
                    : query.OrderBy(m => m.Categoria.Nombre),

                "activo" => descending
                    ? query.OrderByDescending(m => m.Activo)
                    : query.OrderBy(m => m.Activo),

                _ => query.OrderBy(m => m.Nombre)
            };

            // PAGINACION
            var start = Math.Max(0, request.Start);

            var length = request.Length > 0
                ? Math.Min(request.Length, 100)
                : 10;

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(m => new MedicamentoDto
                {
                    Id = m.Id,
                    Codigo = m.Codigo,
                    Nombre = m.Nombre,
                    PrincipioActivo = m.PrincipioActivo,
                    Concentracion = m.Concentracion,
                    FormaFarmaceutica = m.FormaFarmaceutica,

                    // Campo antiguo
                    Presentacion = m.Presentacion,

                    CategoriaId = m.CategoriaId,
                    CategoriaNombre = m.Categoria.Nombre,

                    // Nuevas relaciones
                    LaboratorioId = m.LaboratorioId,
                    LaboratorioNombre = m.Laboratorio != null
                        ? m.Laboratorio.Nombre
                        : null,

                    PresentacionId = m.PresentacionId,
                    PresentacionNombre = m.PresentacionCatalogo != null
                        ? m.PresentacionCatalogo.Nombre
                        : null,

                    PrecioVenta = m.PrecioVenta,

                    Activo = m.Activo
                })
                .ToListAsync();

            return new DataTablesResponseDto<MedicamentoDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
