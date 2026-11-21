using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class LaboratorioRepository : ILaboratorioRepository
    {
        private readonly AppDbContext _context;

        public LaboratorioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Laboratorio>> ObtenerTodosAsync()
        {
            return await _context.Laboratorios
                .AsNoTracking()
                .OrderBy(l => l.Nombre)
                .ThenBy(l => l.Id)
                .ToListAsync();
        }

        public async Task<Laboratorio?> ObtenerPorIdAsync(int id)
        {
            return await _context.Laboratorios
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AgregarAsync(Laboratorio laboratorio)
        {
            await _context.Laboratorios.AddAsync(laboratorio);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Laboratorio laboratorio)
        {
            await _context.SaveChangesAsync();
        }

        public async Task<DataTablesResponseDto<LaboratorioDto>> ObtenerPaginadosAsync(
            DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _context.Laboratorios
                .AsNoTracking()
                .AsQueryable();

            int recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l =>
                    l.Nombre.Contains(search) ||
                    (l.Pais != null && l.Pais.Contains(search)) ||
                    (l.Contacto != null && l.Contacto.Contains(search)) ||
                    (l.Telefono != null && l.Telefono.Contains(search)) ||
                    (l.Email != null && l.Email.Contains(search)) ||
                    (l.Direccion != null && l.Direccion.Contains(search)));
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
                "id" => descending ? query.OrderByDescending(l => l.Id) : query.OrderBy(l => l.Id),
                "pais" => descending ? query.OrderByDescending(l => l.Pais) : query.OrderBy(l => l.Pais),
                "contacto" => descending ? query.OrderByDescending(l => l.Contacto) : query.OrderBy(l => l.Contacto),
                "telefono" => descending ? query.OrderByDescending(l => l.Telefono) : query.OrderBy(l => l.Telefono),
                "email" => descending ? query.OrderByDescending(l => l.Email) : query.OrderBy(l => l.Email),
                "direccion" => descending ? query.OrderByDescending(l => l.Direccion) : query.OrderBy(l => l.Direccion),
                "activo" => descending ? query.OrderByDescending(l => l.Activo) : query.OrderBy(l => l.Activo),
                _ => descending ? query.OrderByDescending(l => l.Nombre) : query.OrderBy(l => l.Nombre)
            };

            query = ((IOrderedQueryable<Laboratorio>)query).ThenBy(l => l.Id);

            int start = Math.Max(0, request.Start);
            int length = request.Length == -1
                ? Math.Min(recordsFiltered, 500)
                : request.Length > 0 ? Math.Min(request.Length, 100) : 10;

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(l => new LaboratorioDto
                {
                    Id = l.Id,
                    Nombre = l.Nombre,
                    Pais = l.Pais,
                    Contacto = l.Contacto,
                    Telefono = l.Telefono,
                    Email = l.Email,
                    Direccion = l.Direccion,
                    Activo = l.Activo
                })
                .ToListAsync();

            return new DataTablesResponseDto<LaboratorioDto>
            {
                Draw = Math.Max(0, request.Draw),
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
