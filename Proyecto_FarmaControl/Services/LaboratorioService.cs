using System.ComponentModel.DataAnnotations;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Services
{
    public class LaboratorioService : ILaboratorioService
    {
        private readonly ILaboratorioRepository _repository;

        public LaboratorioService(ILaboratorioRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Laboratorio>> ObtenerTodosAsync() =>
            _repository.ObtenerTodosAsync();

        public Task<Laboratorio?> ObtenerPorIdAsync(int id) =>
            id > 0 ? _repository.ObtenerPorIdAsync(id) : Task.FromResult<Laboratorio?>(null);

        public Task<DataTablesResponseDto<LaboratorioDto>> ObtenerPaginadosAsync(DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return _repository.ObtenerPaginadosAsync(request);
        }

        public async Task CrearAsync(Laboratorio laboratorio)
        {
            ArgumentNullException.ThrowIfNull(laboratorio);
            Normalizar(laboratorio);
            Validar(laboratorio);
            laboratorio.Activo = true;
            await _repository.AgregarAsync(laboratorio);
        }

        public async Task ActualizarAsync(Laboratorio laboratorio)
        {
            ArgumentNullException.ThrowIfNull(laboratorio);
            if (laboratorio.Id <= 0)
                throw new ArgumentException("El identificador del laboratorio no es válido.");

            var existente = await _repository.ObtenerPorIdAsync(laboratorio.Id);
            if (existente == null)
                throw new KeyNotFoundException("El laboratorio no existe.");
            if (!existente.Activo)
                throw new InvalidOperationException("No se puede actualizar un laboratorio inactivo.");

            Normalizar(laboratorio);
            Validar(laboratorio);

            existente.Nombre = laboratorio.Nombre;
            existente.Pais = laboratorio.Pais;
            existente.Contacto = laboratorio.Contacto;
            existente.Telefono = laboratorio.Telefono;
            existente.Email = laboratorio.Email;
            existente.Direccion = laboratorio.Direccion;

            await _repository.ActualizarAsync(existente);
        }

        public async Task DesactivarAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El identificador del laboratorio no es válido.");

            var laboratorio = await _repository.ObtenerPorIdAsync(id);
            if (laboratorio == null)
                throw new KeyNotFoundException("El laboratorio no existe.");
            if (!laboratorio.Activo)
                throw new InvalidOperationException("El laboratorio ya se encuentra inactivo.");

            laboratorio.Activo = false;
            await _repository.ActualizarAsync(laboratorio);
        }

        private static void Normalizar(Laboratorio laboratorio)
        {
            laboratorio.Nombre = (laboratorio.Nombre ?? string.Empty).Trim();
            laboratorio.Pais = NormalizarOpcional(laboratorio.Pais);
            laboratorio.Contacto = NormalizarOpcional(laboratorio.Contacto);
            laboratorio.Telefono = NormalizarOpcional(laboratorio.Telefono);
            laboratorio.Email = NormalizarOpcional(laboratorio.Email)?.ToLowerInvariant();
            laboratorio.Direccion = NormalizarOpcional(laboratorio.Direccion);
        }

        private static string? NormalizarOpcional(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private static void Validar(Laboratorio laboratorio)
        {
            var resultados = new List<ValidationResult>();
            bool valido = Validator.TryValidateObject(
                laboratorio,
                new ValidationContext(laboratorio),
                resultados,
                validateAllProperties: true);

            if (!valido)
                throw new ArgumentException(resultados.FirstOrDefault()?.ErrorMessage
                    ?? "Los datos del laboratorio no son válidos.");
        }
    }
}
