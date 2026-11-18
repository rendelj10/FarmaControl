
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

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

        public async Task<List<Lote>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        public async Task<Lote?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Lote lote)
        {
            ValidarDatos(lote);

            await ValidarMedicamentoAsync(lote.MedicamentoId);

            lote.NumeroLote = lote.NumeroLote.Trim();
            lote.Almacen = lote.Almacen.Trim();

            // Las existencias iniciales parten
            // de la cantidad recibida.
            lote.Existencias = lote.CantidadInicial;

            lote.Activo = true;

            await _repository.AgregarAsync(lote);
        }

        public async Task ActualizarAsync(Lote lote)
        {
            var existente = await _repository
                .ObtenerPorIdAsync(lote.Id);

            if (existente == null)
                throw new KeyNotFoundException(
                    "El lote no existe.");

            if (!existente.Activo)
                throw new InvalidOperationException(
                    "No se puede actualizar un lote inactivo.");

            ValidarDatos(lote);

            await ValidarMedicamentoAsync(lote.MedicamentoId);

            existente.MedicamentoId = lote.MedicamentoId;
            existente.NumeroLote = lote.NumeroLote.Trim();
            existente.Almacen = lote.Almacen.Trim();
            existente.CostoUnitario = lote.CostoUnitario;
            existente.FechaIngreso = lote.FechaIngreso;
            existente.FechaVencimiento = lote.FechaVencimiento;

            // Se conservan las cantidades existentes
            // para no alterar el historial de inventario.

            await _repository.ActualizarAsync(existente);
        }

        public async Task DesactivarAsync(int id)
        {
            var lote = await _repository.ObtenerPorIdAsync(id);

            if (lote == null)
                throw new KeyNotFoundException(
                    "El lote no existe.");

            if (!lote.Activo)
                throw new InvalidOperationException(
                    "El lote ya se encuentra inactivo.");

            if (lote.Existencias > 0)
                throw new InvalidOperationException(
                    "No se puede desactivar un lote que todavía tiene existencias.");

            lote.Activo = false;

            await _repository.ActualizarAsync(lote);
        }

        private static void ValidarDatos(Lote lote)
        {
            if (lote.MedicamentoId <= 0)
                throw new ArgumentException(
                    "Debe seleccionar un medicamento.");

            if (string.IsNullOrWhiteSpace(lote.NumeroLote))
                throw new ArgumentException(
                    "El número de lote es obligatorio.");

            if (string.IsNullOrWhiteSpace(lote.Almacen))
                throw new ArgumentException(
                    "El almacén es obligatorio.");

            if (lote.CantidadInicial <= 0)
                throw new ArgumentException(
                    "La cantidad inicial debe ser mayor que cero.");

            if (lote.CostoUnitario <= 0)
                throw new ArgumentException(
                    "El costo unitario debe ser mayor que cero.");

            if (lote.FechaIngreso == default)
                throw new ArgumentException(
                    "La fecha de ingreso es obligatoria.");

            if (lote.FechaVencimiento.Date <= lote.FechaIngreso.Date)
                throw new ArgumentException(
                    "La fecha de vencimiento debe ser posterior a la fecha de ingreso.");
        }

        private async Task ValidarMedicamentoAsync(int medicamentoId)
        {
            var medicamento = await _medicamentoRepository
                .ObtenerPorIdAsync(medicamentoId);

            if (medicamento == null)
                throw new ArgumentException(
                    "El medicamento seleccionado no existe.");

            if (!medicamento.Activo)
                throw new ArgumentException(
                    "El medicamento seleccionado está inactivo.");
        }
    }
}
