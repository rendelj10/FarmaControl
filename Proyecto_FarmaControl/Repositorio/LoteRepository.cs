
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class LoteRepository : ILoteRepository
    {
        private readonly AppDbContext _context;

        public LoteRepository(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // CONSULTAR TODOS LOS LOTES
        // ========================================

        public async Task<List<Lote>> ObtenerTodasAsync()
        {
            return await _context.Lotes
                .Include(l => l.Medicamento)
                .Include(l => l.Almacen)
                .AsNoTracking()
                .OrderBy(l => l.FechaVencimiento)
                .ThenBy(l => l.NumeroLote)
                .ToListAsync();
        }

        // ========================================
        // CONSULTAR LOTE POR ID
        // ========================================

        public async Task<Lote?> ObtenerPorIdAsync(int id)
        {
            return await _context.Lotes
                .Include(l => l.Medicamento)
                .Include(l => l.Almacen)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        // ========================================
        // AGREGAR LOTE
        // ========================================

        public async Task AgregarAsync(Lote lote)
        {
            await _context.Lotes.AddAsync(lote);

            await _context.SaveChangesAsync();
        }

        // ========================================
        // ACTUALIZAR LOTE
        // ========================================

        public async Task ActualizarAsync(Lote lote)
        {
            // El servicio obtiene la entidad mediante
            // ObtenerPorIdAsync. EF Core rastrea
            // los cambios de esa entidad.

            await _context.SaveChangesAsync();
        }

        // ========================================
        // VALIDAR ALMACEN EXISTENTE Y ACTIVO
        // ========================================

        public async Task<bool> AlmacenDisponibleAsync(
            int almacenId)
        {
            return await _context.Almacenes
                .AsNoTracking()
                .AnyAsync(a =>
                    a.Id == almacenId &&
                    a.Activo &&
                    a.Sucursal.Activo);
        }

        // ========================================
        // VERIFICAR NUMERO DE LOTE DUPLICADO
        // ========================================

        public async Task<bool> ExisteLoteAsync(
            int medicamentoId,
            string numeroLote,
            int almacenId,
            int? excluirId = null)
        {
            var numeroNormalizado =
                numeroLote.Trim().ToUpperInvariant();

            return await _context.Lotes
                .AsNoTracking()
                .AnyAsync(l =>
                    l.MedicamentoId == medicamentoId &&
                    l.AlmacenId == almacenId &&
                    l.NumeroLote.Trim().ToUpper()
                        == numeroNormalizado &&
                    (!excluirId.HasValue ||
                     l.Id != excluirId.Value));
        }

        // ========================================
        // DATATABLES CON PAGINACION EN SQL SERVER
        // ========================================

        public async Task<DataTablesResponseDto<LoteDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            var query = _context.Lotes
                .AsNoTracking()
                .AsQueryable();

            // Total de registros
            int recordsTotal = await query.CountAsync();

            // =====================================
            // BUSQUEDA
            // =====================================

            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l =>
                    l.NumeroLote.Contains(search) ||
                    l.Medicamento.Nombre.Contains(search) ||
                    l.Almacen.Nombre.Contains(search) ||
                    l.Almacen.Codigo.Contains(search));
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
                    ? query.OrderByDescending(l => l.Id)
                    : query.OrderBy(l => l.Id),

                "numerolote" => descendente
                    ? query.OrderByDescending(l => l.NumeroLote)
                    : query.OrderBy(l => l.NumeroLote),

                "medicamentonombre" => descendente
                    ? query.OrderByDescending(
                        l => l.Medicamento.Nombre)
                    : query.OrderBy(
                        l => l.Medicamento.Nombre),

                "almacennombre" or "almacen" => descendente
                    ? query.OrderByDescending(
                        l => l.Almacen.Nombre)
                    : query.OrderBy(
                        l => l.Almacen.Nombre),

                "cantidadinicial" => descendente
                    ? query.OrderByDescending(
                        l => l.CantidadInicial)
                    : query.OrderBy(l => l.CantidadInicial),

                "existencias" => descendente
                    ? query.OrderByDescending(l => l.Existencias)
                    : query.OrderBy(l => l.Existencias),

                "costounitario" => descendente
                    ? query.OrderByDescending(l => l.CostoUnitario)
                    : query.OrderBy(l => l.CostoUnitario),

                "fechaingreso" => descendente
                    ? query.OrderByDescending(l => l.FechaIngreso)
                    : query.OrderBy(l => l.FechaIngreso),

                "fechavencimiento" => descendente
                    ? query.OrderByDescending(
                        l => l.FechaVencimiento)
                    : query.OrderBy(l => l.FechaVencimiento),

                "activo" => descendente
                    ? query.OrderByDescending(l => l.Activo)
                    : query.OrderBy(l => l.Activo),

                _ => query.OrderBy(l => l.Id)
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
                .Select(l => new LoteDto
                {
                    Id = l.Id,

                    MedicamentoId = l.MedicamentoId,
                    MedicamentoNombre =
                        l.Medicamento.Nombre,

                    NumeroLote = l.NumeroLote,

                    AlmacenId = l.AlmacenId,
                    AlmacenNombre = l.Almacen.Nombre,

                    CantidadInicial = l.CantidadInicial,
                    Existencias = l.Existencias,

                    CostoUnitario = l.CostoUnitario,

                    FechaIngreso = l.FechaIngreso,
                    FechaVencimiento = l.FechaVencimiento,

                    Activo = l.Activo
                })
                .ToListAsync();

            return new DataTablesResponseDto<LoteDto>
            {
                Draw = Math.Max(0, request.Draw),
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }
    }
}
