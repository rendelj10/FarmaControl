
using System.Data;
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public class AjusteInventarioRepository
        : IAjusteInventarioRepository
    {
        private readonly AppDbContext _context;

        public AjusteInventarioRepository(
            AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // 1. CONSULTAR TODOS LOS AJUSTES
        // ========================================


        public async Task<List<AjusteInventario>>
            ObtenerTodosAsync()
        {
            return await _context
                .Set<AjusteInventario>()
                .Include(a => a.Almacen)
                .Include(a => a.Detalles)
                    .ThenInclude(d => d.Medicamento)
                .Include(a => a.Detalles)
                    .ThenInclude(d => d.Lote)
                .AsNoTracking()
                .AsSplitQuery()
                .OrderByDescending(a => a.FechaAjuste)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }


        // ========================================
        // 2. CONSULTAR AJUSTE POR ID
        // ========================================

        public async Task<AjusteInventario?>
            ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _context
                .Set<AjusteInventario>()
                .Include(a => a.Almacen)
                .Include(a => a.Detalles)
                    .ThenInclude(d => d.Medicamento)
                .Include(a => a.Detalles)
                    .ThenInclude(d => d.Lote)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // ========================================
        // 3. OBTENER AJUSTE CON BLOQUEO
        // ========================================

        public async Task<AjusteInventario?>
            ObtenerParaProcesarAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            if (_context.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException(
                    "Se requiere una transacción activa " +
                    "para procesar el ajuste.");
            }

            // El ajuste se bloquea mientras
            // se comprueba su estado y se aplica
            // o anula.
            //
            // No cargamos los lotes con Include.
            // Cada lote se bloqueara individualmente
            // al procesar los detalles.

            return await _context
                .Set<AjusteInventario>()
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM [AjustesInventario]
                            WITH (UPDLOCK, HOLDLOCK)
                       WHERE [Id] = {id}")
                .Include(a => a.Detalles)
                .FirstOrDefaultAsync();
        }

        // ========================================
        // 4. REGISTRAR AJUSTE EN BORRADOR
        // ========================================

        public async Task AgregarAsync(
            AjusteInventario ajuste)
        {
            ArgumentNullException.ThrowIfNull(ajuste);

            // EF Core registrara el encabezado
            // junto con los detalles asociados.
            //
            // Crear un borrador no modifica
            // las existencias de los lotes.

            await _context
                .Set<AjusteInventario>()
                .AddAsync(ajuste);

            await _context.SaveChangesAsync();
        }

        // ========================================
        // 5. VALIDAR DOCUMENTO DUPLICADO
        // ========================================

        public async Task<bool> ExisteDocumentoAsync(
            string numeroDocumento)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
            {
                return false;
            }

            string documento = numeroDocumento
                .Trim()
                .ToUpperInvariant();

            return await _context
                .Set<AjusteInventario>()
                .AsNoTracking()
                .AnyAsync(a =>
                    a.NumeroDocumento.Trim().ToUpper()
                    == documento);
        }

        // ========================================
        // 6. VALIDAR ALMACEN ACTIVO
        // ========================================

        public async Task<bool> AlmacenDisponibleAsync(
            int almacenId)
        {
            if (almacenId <= 0)
            {
                return false;
            }

            return await _context
                .Set<Almacen>()
                .AsNoTracking()
                .AnyAsync(a =>
                    a.Id == almacenId &&
                    a.Activo &&
                    a.Sucursal.Activo);
        }

        // ========================================
        // 7. OBTENER MEDICAMENTOS ACTIVOS
        // ========================================

        public async Task<List<int>>
            ObtenerMedicamentosActivosAsync(
                IEnumerable<int> ids)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var identificadores = ids
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (identificadores.Length == 0)
            {
                return new List<int>();
            }

            return await _context
                .Set<Medicamento>()
                .AsNoTracking()
                .Where(m =>
                    identificadores.Contains(m.Id) &&
                    m.Activo)
                .Select(m => m.Id)
                .ToListAsync();
        }

        // ========================================
        // 8. CONSULTAR LOTE SIN MODIFICARLO
        // ========================================

        public async Task<Lote?>
            ObtenerLoteParaConsultaAsync(int loteId)
        {
            if (loteId <= 0)
            {
                return null;
            }

            return await _context
                .Set<Lote>()
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == loteId);
        }

        // ========================================
        // 9. CONSULTAR LOTE CON BLOQUEO
        // ========================================

        public async Task<Lote?>
            ObtenerLoteParaAjustarAsync(int loteId)
        {
            if (loteId <= 0)
            {
                return null;
            }

            if (_context.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException(
                    "El ajuste de existencias requiere " +
                    "una transacción activa.");
            }

            // UPDLOCK:
            // solicita un bloqueo de actualizacion.
            //
            // HOLDLOCK:
            // mantiene la proteccion hasta
            // finalizar la transaccion.
            //
            // El servicio consultara los lotes
            // en orden ascendente por Id.

            return await _context
                .Set<Lote>()
                .FromSqlInterpolated(
                    $@"SELECT *
                       FROM [Lotes] WITH (UPDLOCK, HOLDLOCK)
                       WHERE [Id] = {loteId}")
                .FirstOrDefaultAsync();
        }

        // ========================================
        // 10. EJECUTAR TRANSACCION SQL
        // ========================================

        public async Task EjecutarEnTransaccionAsync(
            Func<Task> operacion)
        {
            ArgumentNullException.ThrowIfNull(operacion);

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                // El servicio realiza las
                // validaciones y modificaciones.

                await operacion();

                // Guardar conjuntamente el estado
                // del ajuste, los detalles y
                // las nuevas existencias.

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();

                throw;
            }
        }

        // ========================================
        // 11. DATATABLES - PAGINACION SQL
        // ========================================

        public async Task<
            DataTablesResponseDto<AjusteInventarioDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _context
                .Set<AjusteInventario>()
                .AsNoTracking()
                .AsQueryable();

            // ====================================
            // TOTAL DE REGISTROS
            // ====================================

            int recordsTotal =
                await query.CountAsync();

            // ====================================
            // FILTRO DE BUSQUEDA
            // ====================================

            string? search =
                request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.NumeroDocumento.Contains(search) ||
                    a.Estado.Contains(search) ||
                    a.Motivo.Contains(search) ||
                    a.Almacen.Nombre.Contains(search) ||
                    (a.Observaciones != null &&
                     a.Observaciones.Contains(search)));
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

            IOrderedQueryable<AjusteInventario> ordenada =
                columna?.ToLowerInvariant() switch
                {
                    "id" => descendente
                        ? query.OrderByDescending(a => a.Id)
                        : query.OrderBy(a => a.Id),

                    "almacennombre" => descendente
                        ? query.OrderByDescending(
                            a => a.Almacen.Nombre)
                        : query.OrderBy(
                            a => a.Almacen.Nombre),

                    "numerodocumento" => descendente
                        ? query.OrderByDescending(
                            a => a.NumeroDocumento)
                        : query.OrderBy(
                            a => a.NumeroDocumento),

                    "fechaajuste" => descendente
                        ? query.OrderByDescending(
                            a => a.FechaAjuste)
                        : query.OrderBy(
                            a => a.FechaAjuste),

                    "fechaaplicacion" => descendente
                        ? query.OrderByDescending(
                            a => a.FechaAplicacion)
                        : query.OrderBy(
                            a => a.FechaAplicacion),

                    "estado" => descendente
                        ? query.OrderByDescending(a => a.Estado)
                        : query.OrderBy(a => a.Estado),

                    "motivo" => descendente
                        ? query.OrderByDescending(a => a.Motivo)
                        : query.OrderBy(a => a.Motivo),

                    _ => query.OrderByDescending(
                        a => a.FechaAjuste)
                };

            // Orden secundario estable.

            ordenada = ordenada.ThenBy(a => a.Id);

            // ====================================
            // PAGINACION
            // ====================================

            int start = Math.Max(0, request.Start);

            int length = request.Length == -1
                ? Math.Min(recordsFiltered, 500)
                : request.Length > 0
                    ? Math.Min(request.Length, 100)
                    : 10;

            // ====================================
            // PROYECCION A DTO
            // ====================================

            var data = await ordenada
                .Skip(start)
                .Take(length)
                .Select(a => new AjusteInventarioDto
                {
                    Id = a.Id,

                    AlmacenId = a.AlmacenId,

                    AlmacenNombre = a.Almacen.Nombre,

                    NumeroDocumento = a.NumeroDocumento,

                    FechaAjuste = a.FechaAjuste,

                    FechaAplicacion = a.FechaAplicacion,

                    Estado = a.Estado,

                    Motivo = a.Motivo,

                    Observaciones = a.Observaciones
                })
                .ToListAsync();

            return new DataTablesResponseDto<AjusteInventarioDto>
            {
                Draw = Math.Max(0, request.Draw),

                RecordsTotal = recordsTotal,

                RecordsFiltered = recordsFiltered,

                Data = data
            };
        }
    }
}
