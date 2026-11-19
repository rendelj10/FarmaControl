
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly AppDbContext _context;

        public ProveedorRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // 1. CONSULTAR TODOS LOS PROVEEDORES
        // ========================================

        public async Task<List<Proveedor>> ObtenerTodosAsync()
        {
            return await _context.Set<Proveedor>()
                .AsNoTracking()
                .OrderBy(p => p.RazonSocial)
                .ThenBy(p => p.Id)
                .ToListAsync();
        }

        // ========================================
        // 2. CONSULTAR PROVEEDOR POR ID
        // ========================================

        public async Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            return await _context.Set<Proveedor>()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // ========================================
        // 3. AGREGAR PROVEEDOR
        // ========================================

        public async Task AgregarAsync(Proveedor proveedor)
        {
            await _context.Set<Proveedor>()
                .AddAsync(proveedor);

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 4. ACTUALIZAR PROVEEDOR
        // ========================================

        public async Task ActualizarAsync(Proveedor proveedor)
        {
            // El servicio recupera previamente
            // la entidad mediante ObtenerPorIdAsync.
            // Entity Framework rastrea sus cambios.

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 5. VALIDAR CODIGO DUPLICADO
        // ========================================

        public async Task<bool> ExisteCodigoAsync(
            string codigo,
            int? excluirId = null)
        {
            var codigoNormalizado =
                codigo.Trim().ToUpperInvariant();

            return await _context.Set<Proveedor>()
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Codigo.Trim().ToUpper()
                        == codigoNormalizado &&
                    (!excluirId.HasValue ||
                     p.Id != excluirId.Value));
        }

        // ========================================
        // 6. NUEVO:
        // VALIDAR COMPRAS PENDIENTES
        // ========================================

        public async Task<bool> TieneComprasPendientesAsync(
            int proveedorId)
        {
            if (proveedorId <= 0)
            {
                return false;
            }

            // No se permite desactivar un proveedor
            // mientras tenga compras pendientes
            // de recepcion.
            //
            // Las compras recibidas o anuladas
            // no impiden la desactivacion.

            return await _context.Set<Compra>()
                .AsNoTracking()
                .AnyAsync(c =>
                    c.ProveedorId == proveedorId &&
                    c.Estado == "Borrador");
        }

        // ========================================
        // 7. DATATABLES - PAGINACION SQL SERVER
        // ========================================

        public async Task<DataTablesResponseDto<ProveedorDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _context.Set<Proveedor>()
                .AsNoTracking()
                .AsQueryable();

            // =====================================
            // TOTAL DE REGISTROS
            // =====================================

            int recordsTotal = await query.CountAsync();

            // =====================================
            // BUSQUEDA GENERAL
            // =====================================

            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Codigo.Contains(search) ||
                    p.RazonSocial.Contains(search) ||
                    (p.NombreComercial != null &&
                     p.NombreComercial.Contains(search)) ||
                    (p.Ruc != null &&
                     p.Ruc.Contains(search)) ||
                    (p.Contacto != null &&
                     p.Contacto.Contains(search)) ||
                    (p.Telefono != null &&
                     p.Telefono.Contains(search)) ||
                    (p.Email != null &&
                     p.Email.Contains(search)) ||
                    (p.Direccion != null &&
                     p.Direccion.Contains(search)));
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
                    ? query.OrderByDescending(p => p.Id)
                    : query.OrderBy(p => p.Id),

                "codigo" => descendente
                    ? query.OrderByDescending(p => p.Codigo)
                    : query.OrderBy(p => p.Codigo),

                "razonsocial" => descendente
                    ? query.OrderByDescending(p => p.RazonSocial)
                    : query.OrderBy(p => p.RazonSocial),

                "nombrecomercial" => descendente
                    ? query.OrderByDescending(p => p.NombreComercial)
                    : query.OrderBy(p => p.NombreComercial),

                "ruc" => descendente
                    ? query.OrderByDescending(p => p.Ruc)
                    : query.OrderBy(p => p.Ruc),

                "contacto" => descendente
                    ? query.OrderByDescending(p => p.Contacto)
                    : query.OrderBy(p => p.Contacto),

                "telefono" => descendente
                    ? query.OrderByDescending(p => p.Telefono)
                    : query.OrderBy(p => p.Telefono),

                "email" => descendente
                    ? query.OrderByDescending(p => p.Email)
                    : query.OrderBy(p => p.Email),

                "direccion" => descendente
                    ? query.OrderByDescending(p => p.Direccion)
                    : query.OrderBy(p => p.Direccion),

                "activo" => descendente
                    ? query.OrderByDescending(p => p.Activo)
                    : query.OrderBy(p => p.Activo),

                _ => query.OrderBy(p => p.RazonSocial)
            };

            // Orden secundario para mantener
            // la paginacion estable.

            query = ((IOrderedQueryable<Proveedor>)query)
                .ThenBy(p => p.Id);

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
                .Select(p => new ProveedorDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    RazonSocial = p.RazonSocial,
                    NombreComercial = p.NombreComercial,
                    Ruc = p.Ruc,
                    Contacto = p.Contacto,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    Direccion = p.Direccion,
                    Activo = p.Activo
                })
                .ToListAsync();

            return new DataTablesResponseDto<ProveedorDto>
            {
                Draw = Math.Max(0, request.Draw),
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
