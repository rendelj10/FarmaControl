
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _context;

        public ClienteRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // 1. CONSULTAR TODOS LOS CLIENTES
        // ========================================

        public async Task<List<Cliente>> ObtenerTodosAsync()
        {
            return await _context.Set<Cliente>()
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Id)
                .ToListAsync();
        }

        // ========================================
        // 2. CONSULTAR CLIENTE POR ID
        // ========================================

        public async Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _context.Set<Cliente>()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // ========================================
        // 3. AGREGAR CLIENTE
        // ========================================

        public async Task AgregarAsync(Cliente cliente)
        {
            ArgumentNullException.ThrowIfNull(cliente);

            await _context.Set<Cliente>()
                .AddAsync(cliente);

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 4. ACTUALIZAR CLIENTE
        // ========================================

        public async Task ActualizarAsync(Cliente cliente)
        {
            ArgumentNullException.ThrowIfNull(cliente);

            // El servicio obtiene previamente
            // una entidad rastreada mediante
            // ObtenerPorIdAsync.
            //
            // EF Core guarda los campos modificados.

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 5. VERIFICAR CODIGO DUPLICADO
        // ========================================

        public async Task<bool> ExisteCodigoAsync(
            string codigo,
            int? excluirId = null)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return false;
            }

            string codigoNormalizado = codigo
                .Trim()
                .ToUpperInvariant();

            return await _context.Set<Cliente>()
                .AsNoTracking()
                .AnyAsync(c =>
                    c.Codigo.Trim().ToUpper()
                        == codigoNormalizado &&
                    (!excluirId.HasValue ||
                     c.Id != excluirId.Value));

        }


        // ========================================
        // VERIFICAR VENTAS PENDIENTES
        // ========================================

        public async Task<bool> TieneVentasPendientesAsync(
            int clienteId)
        {
            if (clienteId <= 0)
            {
                return false;
            }

            return await _context.Set<Venta>()
                .AsNoTracking()
                .AnyAsync(v =>
                    v.ClienteId == clienteId &&
                    v.Estado == "Borrador");
        }


        // ========================================
        // 6. DATATABLES - PAGINACION SQL
        // ========================================

        public async Task<DataTablesResponseDto<ClienteDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _context.Set<Cliente>()
                .AsNoTracking()
                .AsQueryable();

            // ====================================
            // TOTAL DE REGISTROS
            // ====================================

            int recordsTotal =
                await query.CountAsync();

            // ====================================
            // BUSQUEDA GENERAL
            // ====================================

            string? search =
                request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Codigo.Contains(search) ||
                    c.Nombre.Contains(search) ||
                    (c.DocumentoIdentidad != null &&
                     c.DocumentoIdentidad.Contains(search)) ||
                    (c.Telefono != null &&
                     c.Telefono.Contains(search)) ||
                    (c.Email != null &&
                     c.Email.Contains(search)) ||
                    (c.Direccion != null &&
                     c.Direccion.Contains(search)));
            }

            int recordsFiltered =
                await query.CountAsync();

            // ====================================
            // ORDENAMIENTO
            // ====================================

            var orden =
                request.Order?.FirstOrDefault();

            int indiceColumna =
                orden?.Column ?? -1;

            string? columna = indiceColumna >= 0
                ? request.Columns?
                    .ElementAtOrDefault(indiceColumna)?.Data
                : null;

            bool descendente = string.Equals(
                orden?.Dir,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            IOrderedQueryable<Cliente> ordenada =
                columna?.ToLowerInvariant() switch
                {
                    "id" => descendente
                        ? query.OrderByDescending(c => c.Id)
                        : query.OrderBy(c => c.Id),

                    "codigo" => descendente
                        ? query.OrderByDescending(c => c.Codigo)
                        : query.OrderBy(c => c.Codigo),

                    "nombre" => descendente
                        ? query.OrderByDescending(c => c.Nombre)
                        : query.OrderBy(c => c.Nombre),

                    "documentoidentidad" => descendente
                        ? query.OrderByDescending(
                            c => c.DocumentoIdentidad)
                        : query.OrderBy(
                            c => c.DocumentoIdentidad),

                    "telefono" => descendente
                        ? query.OrderByDescending(c => c.Telefono)
                        : query.OrderBy(c => c.Telefono),

                    "email" => descendente
                        ? query.OrderByDescending(c => c.Email)
                        : query.OrderBy(c => c.Email),

                    "direccion" => descendente
                        ? query.OrderByDescending(c => c.Direccion)
                        : query.OrderBy(c => c.Direccion),

                    "activo" => descendente
                        ? query.OrderByDescending(c => c.Activo)
                        : query.OrderBy(c => c.Activo),

                    _ => query.OrderBy(c => c.Nombre)
                };

            // Orden adicional para que
            // la paginacion sea estable.

            ordenada = ordenada.ThenBy(c => c.Id);

            // ====================================
            // PAGINACION
            // ====================================

            int start =
                Math.Max(0, request.Start);

            int length = request.Length == -1
                ? Math.Min(recordsFiltered, 500)
                : request.Length > 0
                    ? Math.Min(request.Length, 100)
                    : 10;

            // ====================================
            // CONVERTIR A DTO
            // ====================================

            var data = await ordenada
                .Skip(start)
                .Take(length)
                .Select(c => new ClienteDto
                {
                    Id = c.Id,

                    Codigo = c.Codigo,

                    Nombre = c.Nombre,

                    DocumentoIdentidad =
                        c.DocumentoIdentidad,

                    Telefono = c.Telefono,

                    Email = c.Email,

                    Direccion = c.Direccion,

                    Activo = c.Activo
                })
                .ToListAsync();

            return new DataTablesResponseDto<ClienteDto>
            {
                Draw = Math.Max(0, request.Draw),

                RecordsTotal = recordsTotal,

                RecordsFiltered = recordsFiltered,

                Data = data
            };
        }
    }
}
