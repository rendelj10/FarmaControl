using System.ComponentModel.DataAnnotations;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Service
{
    public class MetodoPagoService : IMetodoPagoService
    {
        private readonly IMetodoPagoRepository _repository;

        public MetodoPagoService(IMetodoPagoRepository repository)
        {
            _repository = repository;
        }

        public Task<List<MetodoPago>> ObtenerTodosAsync() =>
            _repository.ObtenerTodosAsync();

        public Task<MetodoPago?> ObtenerPorIdAsync(int id) =>
            id > 0
                ? _repository.ObtenerPorIdAsync(id)
                : Task.FromResult<MetodoPago?>(null);

        public Task<DataTablesResponseDto<MetodoPagoDto>> ObtenerPaginadosAsync(
            DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return _repository.ObtenerPaginadosAsync(request);
        }

        public async Task CrearAsync(MetodoPago metodoPago)
        {
            ArgumentNullException.ThrowIfNull(metodoPago);

            Normalizar(metodoPago);
            Validar(metodoPago);

            if (await _repository.ExisteNombreAsync(metodoPago.Nombre))
                throw new InvalidOperationException(
                    "Ya existe un método de pago con ese nombre.");

            metodoPago.Activo = true;
            await _repository.AgregarAsync(metodoPago);
        }

        public async Task ActualizarAsync(MetodoPago metodoPago)
        {
            ArgumentNullException.ThrowIfNull(metodoPago);

            if (metodoPago.Id <= 0)
                throw new ArgumentException(
                    "El identificador del método de pago no es válido.");

            var existente = await _repository.ObtenerPorIdAsync(metodoPago.Id);

            if (existente == null)
                throw new KeyNotFoundException(
                    "El método de pago no existe.");

            if (!existente.Activo)
                throw new InvalidOperationException(
                    "No se puede actualizar un método de pago inactivo.");

            Normalizar(metodoPago);
            Validar(metodoPago);

            if (await _repository.ExisteNombreAsync(
                metodoPago.Nombre,
                metodoPago.Id))
            {
                throw new InvalidOperationException(
                    "Ya existe otro método de pago con ese nombre.");
            }

            existente.Nombre = metodoPago.Nombre;
            existente.Descripcion = metodoPago.Descripcion;
            existente.RequiereReferencia = metodoPago.RequiereReferencia;

            await _repository.ActualizarAsync(existente);
        }

        public async Task DesactivarAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException(
                    "El identificador del método de pago no es válido.");

            var metodoPago = await _repository.ObtenerPorIdAsync(id);

            if (metodoPago == null)
                throw new KeyNotFoundException(
                    "El método de pago no existe.");

            if (!metodoPago.Activo)
                throw new InvalidOperationException(
                    "El método de pago ya se encuentra inactivo.");

            metodoPago.Activo = false;
            await _repository.ActualizarAsync(metodoPago);
        }

        private static void Normalizar(MetodoPago metodoPago)
        {
            metodoPago.Nombre = (metodoPago.Nombre ?? string.Empty).Trim();
            metodoPago.Descripcion = string.IsNullOrWhiteSpace(metodoPago.Descripcion)
                ? null
                : metodoPago.Descripcion.Trim();
        }

        private static void Validar(MetodoPago metodoPago)
        {
            var resultados = new List<ValidationResult>();

            bool valido = Validator.TryValidateObject(
                metodoPago,
                new ValidationContext(metodoPago),
                resultados,
                validateAllProperties: true);

            if (!valido)
            {
                throw new ArgumentException(
                    resultados.FirstOrDefault()?.ErrorMessage ??
                    "Los datos del método de pago no son válidos.");
            }
        }
    }
}
