
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Services
{
    public class MedicamentoService : IMedicamentoService
    {
        private readonly IMedicamentoRepository _repository;
        private readonly ICategoriaRepository _categoriaRepository;

        public MedicamentoService(
            IMedicamentoRepository repository,
            ICategoriaRepository categoriaRepository)
        {
            _repository = repository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<List<Medicamento>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        public async Task<Medicamento?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Medicamento medicamento)
        {
            ValidarDatos(medicamento);

medicamento.Codigo = medicamento.Codigo.Trim();
medicamento.Nombre = medicamento.Nombre.Trim();

            await ValidarCategoriaAsync(medicamento.CategoriaId);

            if (await _repository.ExisteCodigoAsync(medicamento.Codigo))
                throw new InvalidOperationException(
                    "Ya existe un medicamento con ese código.");

            NormalizarCamposOpcionales(medicamento);

            medicamento.Activo = true;

            await _repository.AgregarAsync(medicamento);
        }

        public async Task ActualizarAsync(Medicamento medicamento)
        {
            var existente = await _repository
                .ObtenerPorIdAsync(medicamento.Id);

            if (existente == null)
                throw new KeyNotFoundException(
                    "El medicamento no existe.");

            ValidarDatos(medicamento);

medicamento.Codigo = medicamento.Codigo.Trim();
medicamento.Nombre = medicamento.Nombre.Trim();

            await ValidarCategoriaAsync(medicamento.CategoriaId);

            if (await _repository.ExisteCodigoAsync(
                medicamento.Codigo, medicamento.Id))
                throw new InvalidOperationException(
                    "Ya existe un medicamento con ese código.");

            existente.Codigo = medicamento.Codigo;
            existente.Nombre = medicamento.Nombre;
            existente.PrincipioActivo = medicamento.PrincipioActivo;
            existente.Concentracion = medicamento.Concentracion;
            existente.FormaFarmaceutica = medicamento.FormaFarmaceutica;
            existente.Presentacion = medicamento.Presentacion;
            existente.CategoriaId = medicamento.CategoriaId;

            NormalizarCamposOpcionales(existente);

            await _repository.ActualizarAsync(existente);
        }

        public async Task DesactivarAsync(int id)
        {
            var medicamento = await _repository.ObtenerPorIdAsync(id);

            if (medicamento == null)
                throw new KeyNotFoundException(
                    "El medicamento no existe.");

            medicamento.Activo = false;

            await _repository.ActualizarAsync(medicamento);
        }

        private static void ValidarDatos(Medicamento medicamento)
        {
            if (string.IsNullOrWhiteSpace(medicamento.Codigo))
                throw new ArgumentException(
                    "El código del medicamento es obligatorio.");

            if (string.IsNullOrWhiteSpace(medicamento.Nombre))
                throw new ArgumentException(
                    "El nombre del medicamento es obligatorio.");
        }

        private async Task ValidarCategoriaAsync(int categoriaId)
        {
            var categoria = await _categoriaRepository
                .ObtenerPorIdAsync(categoriaId);

            if (categoria == null)
                throw new ArgumentException(
                    "La categoría seleccionada no existe.");

            if (!categoria.Activo)
                throw new ArgumentException(
                    "La categoría seleccionada está inactiva.");
        }

        private static void NormalizarCamposOpcionales(
            Medicamento medicamento)
        {
            medicamento.PrincipioActivo =
                NormalizarTexto(medicamento.PrincipioActivo);

            medicamento.Concentracion =
                NormalizarTexto(medicamento.Concentracion);

            medicamento.FormaFarmaceutica =
                NormalizarTexto(medicamento.FormaFarmaceutica);

            medicamento.Presentacion =
                NormalizarTexto(medicamento.Presentacion);
        }

        private static string? NormalizarTexto(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }
    }
}
