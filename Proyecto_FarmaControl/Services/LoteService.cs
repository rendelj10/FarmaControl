
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Services
{
    public class LoteService : ILoteService
    {
        private readonly ILoteRepository _repository;
        private readonly IMedicamentoRepository _medicamentoRepository;

        public LoteService(
            ILoteRepository repository,
            IMedicamentoRepository medicamentoRepository)
        {
            _repository = repository;
            _medicamentoRepository = medicamentoRepository;
        }

        // ========================================
        // CONSULTAR TODOS LOS LOTES
        // ========================================

        public async Task<List<Lote>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        // ========================================
        // CONSULTAR LOTE POR ID
        // ========================================

        public async Task<Lote?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        // ========================================
        // DATATABLES - PAGINACION SQL
        // ========================================

        public async Task<DataTablesResponseDto<LoteDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            return await _repository.ObtenerPaginadasAsync(request);
        }

        // ========================================
        // CREAR LOTE
        // ========================================

        public async Task CrearAsync(Lote lote)
        {
            ValidarDatos(lote, esCreacion: true);

            await ValidarMedicamentoAsync(lote.MedicamentoId);
            await ValidarAlmacenAsync(lote.AlmacenId);

            lote.NumeroLote = lote.NumeroLote.Trim();

            bool duplicado = await _repository.ExisteLoteAsync(
                lote.MedicamentoId,
                lote.NumeroLote,
                lote.AlmacenId);

            if (duplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe un lote con ese número " +
                    "para el medicamento y almacén seleccionados.");
            }

            lote.Existencias = lote.CantidadInicial;
            lote.Activo = true;

            await _repository.AgregarAsync(lote);
        }

        // ========================================
        // ACTUALIZAR LOTE
        // ========================================

        public async Task ActualizarAsync(Lote lote)
        {
            var existente =
                await _repository.ObtenerPorIdAsync(lote.Id);

            if (existente == null)
            {
                throw new KeyNotFoundException(
                    "El lote no existe.");
            }

            if (!existente.Activo)
            {
                throw new InvalidOperationException(
                    "No se puede actualizar un lote inactivo.");
            }

            ValidarDatos(lote, esCreacion: false);

            await ValidarMedicamentoAsync(lote.MedicamentoId);
            await ValidarAlmacenAsync(lote.AlmacenId);

            // No permitir trasladar o reasignar lotes
            // que todavía tengan existencias.
            if (existente.Existencias > 0 &&
                (existente.MedicamentoId != lote.MedicamentoId ||
                 existente.AlmacenId != lote.AlmacenId))
            {
                throw new InvalidOperationException(
                    "No se puede cambiar el medicamento " +
                    "ni el almacén de un lote con existencias. " +
                    "Debe utilizarse un movimiento de inventario.");
            }

            lote.NumeroLote = lote.NumeroLote.Trim();

            bool duplicado = await _repository.ExisteLoteAsync(
                lote.MedicamentoId,
                lote.NumeroLote,
                lote.AlmacenId,
                excluirId: lote.Id);

            if (duplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe otro lote con ese número " +
                    "para el medicamento y almacén seleccionados.");
            }

            existente.MedicamentoId = lote.MedicamentoId;
            existente.NumeroLote = lote.NumeroLote;
            existente.AlmacenId = lote.AlmacenId;

            existente.CostoUnitario = lote.CostoUnitario;
            existente.FechaIngreso = lote.FechaIngreso;
            existente.FechaVencimiento = lote.FechaVencimiento;

            // Se conservan CantidadInicial y Existencias.
            // Los ajustes de stock se implementarán
            // en el módulo de movimientos de inventario.

            await _repository.ActualizarAsync(existente);
        }

        // ========================================
        // DESACTIVAR LOTE
        // ========================================

        public async Task DesactivarAsync(int id)
        {
            var lote = await _repository.ObtenerPorIdAsync(id);

            if (lote == null)
            {
                throw new KeyNotFoundException(
                    "El lote no existe.");
            }

            if (!lote.Activo)
            {
                throw new InvalidOperationException(
                    "El lote ya se encuentra inactivo.");
            }

            if (lote.Existencias > 0)
            {
                throw new InvalidOperationException(
                    "No se puede desactivar un lote " +
                    "que todavía tiene existencias.");
            }

            lote.Activo = false;

            await _repository.ActualizarAsync(lote);
        }

        // ========================================
        // VALIDAR DATOS
        // ========================================

        private static void ValidarDatos(
            Lote lote,
            bool esCreacion)
        {
            if (lote.MedicamentoId <= 0)
            {
                throw new ArgumentException(
                    "Debe seleccionar un medicamento.");
            }

            if (lote.AlmacenId <= 0)
            {
                throw new ArgumentException(
                    "Debe seleccionar un almacén válido.");
            }

            if (string.IsNullOrWhiteSpace(lote.NumeroLote))
            {
                throw new ArgumentException(
                    "El número de lote es obligatorio.");
            }

            if (lote.NumeroLote.Trim().Length > 50)
            {
                throw new ArgumentException(
                    "El número de lote no puede superar 50 caracteres.");
            }

            if (esCreacion && lote.CantidadInicial <= 0)
            {
                throw new ArgumentException(
                    "La cantidad inicial debe ser mayor que cero.");
            }

            if (lote.CostoUnitario <= 0)
            {
                throw new ArgumentException(
                    "El costo unitario debe ser mayor que cero.");
            }

            if (lote.FechaIngreso == default)
            {
                throw new ArgumentException(
                    "La fecha de ingreso es obligatoria.");
            }

            if (lote.FechaVencimiento == default)
            {
                throw new ArgumentException(
                    "La fecha de vencimiento es obligatoria.");
            }

            if (lote.FechaVencimiento.Date <=
                lote.FechaIngreso.Date)
            {
                throw new ArgumentException(
                    "La fecha de vencimiento debe ser " +
                    "posterior a la fecha de ingreso.");
            }
        }

        // ========================================
        // VALIDAR MEDICAMENTO
        // ========================================

        private async Task ValidarMedicamentoAsync(int medicamentoId)
        {
            var medicamento = await _medicamentoRepository
                .ObtenerPorIdAsync(medicamentoId);

            if (medicamento == null)
            {
                throw new ArgumentException(
                    "El medicamento seleccionado no existe.");
            }

            if (!medicamento.Activo)
            {
                throw new ArgumentException(
                    "El medicamento seleccionado está inactivo.");
            }
        }

        // ========================================
        // VALIDAR ALMACEN Y SUCURSAL
        // ========================================

        private async Task ValidarAlmacenAsync(int almacenId)
        {
            bool disponible =
                await _repository.AlmacenDisponibleAsync(almacenId);

            if (!disponible)
            {
                throw new ArgumentException(
                    "El almacén seleccionado no existe, " +
                    "está inactivo o pertenece a una " +
                    "sucursal inactiva.");
            }
        }
    }
}
