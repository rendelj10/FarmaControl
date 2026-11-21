
using System.ComponentModel.DataAnnotations;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Service
{
    public class PresentacionService : IPresentacionService
    {
        private readonly IPresentacionRepository _repository;

        public PresentacionService(IPresentacionRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Presentacion>> ObtenerTodasAsync() =>
            _repository.ObtenerTodasAsync();

        public Task<Presentacion?> ObtenerPorIdAsync(int id) =>
            id > 0
                ? _repository.ObtenerPorIdAsync(id)
                : Task.FromResult<Presentacion?>(null);

        public Task<DataTablesResponseDto<PresentacionDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return _repository.ObtenerPaginadasAsync(request);
        }

        public async Task CrearAsync(Presentacion presentacion)
        {
            ArgumentNullException.ThrowIfNull(presentacion);

            Normalizar(presentacion);
            Validar(presentacion);

            if (await _repository.ExisteNombreAsync(presentacion.Nombre))
                throw new InvalidOperationException(
                    "Ya existe una presentación con ese nombre.");

            presentacion.Activo = true;

            await _repository.AgregarAsync(presentacion);
        }

        public async Task ActualizarAsync(Presentacion presentacion)
        {
            ArgumentNullException.ThrowIfNull(presentacion);

            if (presentacion.Id <= 0)
                throw new ArgumentException(
                    "El identificador de la presentación no es válido.");

            var existente = await _repository.ObtenerPorIdAsync(
                presentacion.Id);

            if (existente == null)
                throw new KeyNotFoundException(
                    "La presentación no existe.");

            if (!existente.Activo)
                throw new InvalidOperationException(
                    "No se puede actualizar una presentación inactiva.");

            Normalizar(presentacion);
            Validar(presentacion);

            if (await _repository.ExisteNombreAsync(
                presentacion.Nombre, presentacion.Id))
            {
                throw new InvalidOperationException(
                    "Ya existe otra presentación con ese nombre.");
            }

            existente.Nombre = presentacion.Nombre;
            existente.Descripcion = presentacion.Descripcion;
            existente.CantidadContenido = presentacion.CantidadContenido;
            existente.UnidadContenido = presentacion.UnidadContenido;

            await _repository.ActualizarAsync(existente);
        }

        public async Task DesactivarAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException(
                    "El identificador de la presentación no es válido.");

            var presentacion = await _repository.ObtenerPorIdAsync(id);

            if (presentacion == null)
                throw new KeyNotFoundException(
                    "La presentación no existe.");

            if (!presentacion.Activo)
                throw new InvalidOperationException(
                    "La presentación ya se encuentra inactiva.");

            presentacion.Activo = false;

            await _repository.ActualizarAsync(presentacion);
        }

        private static void Normalizar(Presentacion presentacion)
        {
            presentacion.Nombre =
                (presentacion.Nombre ?? string.Empty).Trim();

            presentacion.Descripcion =
                NormalizarOpcional(presentacion.Descripcion);

            presentacion.UnidadContenido =
                NormalizarOpcional(presentacion.UnidadContenido);
        }

        private static string? NormalizarOpcional(string? valor) =>
            string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();

        private static void Validar(Presentacion presentacion)
        {
            var resultados = new List<ValidationResult>();

            bool valido = Validator.TryValidateObject(
                presentacion,
                new ValidationContext(presentacion),
                resultados,
                validateAllProperties: true);

            if (!valido)
            {
                throw new ArgumentException(
                    resultados.FirstOrDefault()?.ErrorMessage
                    ?? "Los datos de la presentación no son válidos.");
            }

            if (presentacion.CantidadContenido.HasValue &&
                presentacion.CantidadContenido.Value <= 0)
            {
                throw new ArgumentException(
                    "La cantidad de contenido debe ser mayor que cero.");
            }
        }
    }
}
