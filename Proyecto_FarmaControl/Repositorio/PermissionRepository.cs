
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        // =====================================
        // OBTENER TODOS LOS PERMISOS
        // =====================================

        public async Task<IReadOnlyList<Permiso>> GetAllAsync()
        {
            return await _context.Permissions
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        // =====================================
        // OBTENER PERMISO POR ID
        // =====================================

        public async Task<Permiso?> GetByIdAsync(int id)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // =====================================
        // OBTENER PERMISO POR NOMBRE
        // =====================================

        public async Task<Permiso?> GetByNameAsync(string name)
        {
            var nombre = name.Trim().ToLowerInvariant();

            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == nombre);
        }

        // =====================================
        // CREAR PERMISO
        // =====================================

        public async Task<Permiso> CreateAsync(
            string name,
            string? description)
        {
            var permission = new Permiso
            {
                Name = name.Trim().ToLowerInvariant(),
                Description = description?.Trim()
            };

            await _context.Permissions.AddAsync(permission);

            await _context.SaveChangesAsync();

            return permission;
        }

        // =====================================
        // ACTUALIZAR PERMISO
        // =====================================

        public async Task<bool> UpdateAsync(Permiso permission)
        {
            _context.Permissions.Update(permission);

            var filasAfectadas =
                await _context.SaveChangesAsync();

            return filasAfectadas > 0;
        }

        // =====================================
        // VERIFICAR ASIGNACIONES
        // =====================================

        public async Task<bool> IsAssignedAsync(int permissionId)
        {
            var asignadoARol = await _context.RolePermissions
                .AnyAsync(rp => rp.PermissionId == permissionId);

            if (asignadoARol)
                return true;

            return await _context.UsuarioPermisos
                .AnyAsync(up => up.PermissionId == permissionId);
        }

        // =====================================
        // ELIMINAR PERMISO
        // =====================================

        public async Task<bool> DeleteAsync(int id)
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null)
                return false;

            if (await IsAssignedAsync(id))
                return false;

            _context.Permissions.Remove(permission);

            var filasAfectadas =
                await _context.SaveChangesAsync();

            return filasAfectadas > 0;
        }

        // =====================================
        // DATATABLES - PAGINACION Y BUSQUEDA
        // =====================================

        public async Task<DataTablesResponseDto<PermissionDto>>
            GetPagedAsync(DataTablesRequestDto request)
        {
            var query = _context.Permissions
                .AsNoTracking()
                .AsQueryable();

            // TOTAL DE REGISTROS

            var recordsTotal = await query.CountAsync();

            // BUSQUEDA

            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Description != null &&
                     p.Description.Contains(search)));
            }

            var recordsFiltered = await query.CountAsync();

            // ORDENAMIENTO

            var order = request.Order?.FirstOrDefault();

            var orderColumn = order?.Column ?? 0;

            var orderDir = order?.Dir ?? "asc";

            var descending = string.Equals(
                orderDir,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            var columnName = request.Columns?
                .ElementAtOrDefault(orderColumn)?.Data ?? "name";

            query = columnName.ToLowerInvariant() switch
            {
                "id" => descending
                    ? query.OrderByDescending(p => p.Id)
                    : query.OrderBy(p => p.Id),

                "description" or "descripcion" => descending
                    ? query.OrderByDescending(p => p.Description)
                    : query.OrderBy(p => p.Description),

                _ => descending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name)
            };

            // PAGINACION

            var start = Math.Max(0, request.Start);

            var length = request.Length > 0
                ? Math.Min(request.Length, 100)
                : 10;

            // PROYECCION A DTO

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                })
                .ToListAsync();

            // RESPUESTA DATATABLES

            return new DataTablesResponseDto<PermissionDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
