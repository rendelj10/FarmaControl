
using System.ComponentModel.DataAnnotations;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Service
{
    public class SucursalService : ISucursalService
    {
        private readonly ISucursalRepository _repository;

        public SucursalService(
            ISucursalRepository repository)
        {
            _repository = repository;
        }

        // ========================================
        // CONSULTAR TODAS LAS SUCURSALES
        // ========================================

        public async Task<List<Sucursal>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        // ========================================
        // CONSULTAR SUCURSAL POR ID
        // ========================================

        public async Task<Sucursal?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        // ========================================
        // PAGINACION SQL PARA DATATABLES
        // ========================================

        public async Task<DataTablesResponseDto<SucursalDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await _repository
                .ObtenerPaginadasAsync(request);
        }

        // ========================================
        // CREAR SUCURSAL
        // ========================================

        public async Task CrearAsync(Sucursal sucursal)
        {
            ArgumentNullException.ThrowIfNull(sucursal);

            NormalizarDatos(sucursal);
            ValidarDatos(sucursal);

            bool duplicado =
                await _repository.ExisteCodigoAsync(
                    sucursal.Codigo);

            if (duplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe una sucursal con ese código.");
            }

            sucursal.Activo = true;

            await _repository.AgregarAsync(sucursal);
        }

        // ========================================
        // ACTUALIZAR SUCURSAL
        // ========================================

        public async Task ActualizarAsync(Sucursal sucursal)
        {
            ArgumentNullException.ThrowIfNull(sucursal);

            if (sucursal.Id <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la sucursal no es válido.");
            }

            var existente =
                await _repository.ObtenerPorIdAsync(
                    sucursal.Id);

            if (existente == null)
            {
                throw new KeyNotFoundException(
                    "La sucursal no existe.");
            }

            if (!existente.Activo)
            {
                throw new InvalidOperationException(
                    "No se puede actualizar una sucursal inactiva.");
            }

            NormalizarDatos(sucursal);
            ValidarDatos(sucursal);

            bool duplicado =
                await _repository.ExisteCodigoAsync(
                    sucursal.Codigo,
                    excluirId: sucursal.Id);

            if (duplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe otra sucursal con ese código.");
            }

            existente.Codigo = sucursal.Codigo;
            existente.Nombre = sucursal.Nombre;
            existente.Direccion = sucursal.Direccion;
            existente.Telefono = sucursal.Telefono;
            existente.Email = sucursal.Email;

            await _repository.ActualizarAsync(existente);
        }

        // ========================================
        // DESACTIVAR SUCURSAL
        // ========================================

        public async Task DesactivarAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la sucursal no es válido.");
            }

            var sucursal =
                await _repository.ObtenerPorIdAsync(id);

            if (sucursal == null)
            {
                throw new KeyNotFoundException(
                    "La sucursal no existe.");
            }

            if (!sucursal.Activo)
            {
                throw new InvalidOperationException(
                    "La sucursal ya se encuentra inactiva.");
            }

            bool tieneAlmacenes =
                await _repository.TieneAlmacenesAsync(id);

            if (tieneAlmacenes)
            {
                throw new InvalidOperationException(
                    "No se puede desactivar una sucursal " +
                    "que tiene almacenes asociados.");
            }

            sucursal.Activo = false;

            await _repository.ActualizarAsync(sucursal);
        }

        // ========================================
        // NORMALIZAR DATOS
        // ========================================

        private static void NormalizarDatos(
            Sucursal sucursal)
        {
            sucursal.Codigo =
                (sucursal.Codigo ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();

            sucursal.Nombre =
                (sucursal.Nombre ?? string.Empty)
                    .Trim();

            sucursal.Direccion =
                NormalizarOpcional(sucursal.Direccion);

            sucursal.Telefono =
                NormalizarOpcional(sucursal.Telefono);

            sucursal.Email =
                NormalizarOpcional(sucursal.Email);
        }

        private static string? NormalizarOpcional(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        // ========================================
        // VALIDAR DATOS
        // ========================================

        private static void ValidarDatos(
            Sucursal sucursal)
        {
            var resultados =
                new List<ValidationResult>();

            bool valido = Validator.TryValidateObject(
                sucursal,
                new ValidationContext(sucursal),
                resultados,
                validateAllProperties: true);

            if (!valido)
            {
                throw new ArgumentException(
                    resultados.FirstOrDefault()?.ErrorMessage
                    ?? "Los datos de la sucursal no son válidos.");
            }
        }
    }
}
