
using System.ComponentModel.DataAnnotations;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Services
{
    public class AlmacenService : IAlmacenService
    {
        private readonly IAlmacenRepository _repository;

        public AlmacenService(
            IAlmacenRepository repository)
        {
            _repository = repository;
        }

        // ========================================
        // CONSULTAR TODOS LOS ALMACENES
        // ========================================

        public async Task<List<Almacen>> ObtenerTodosAsync()
        {
            return await _repository.ObtenerTodosAsync();
        }

        // ========================================
        // CONSULTAR ALMACEN POR ID
        // ========================================

        public async Task<Almacen?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        // ========================================
        // PAGINACION SQL SERVER
        // ========================================

        public async Task<DataTablesResponseDto<AlmacenDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await _repository
                .ObtenerPaginadasAsync(request);
        }

        // ========================================
        // CREAR ALMACEN
        // ========================================

        public async Task CrearAsync(Almacen almacen)
        {
            ArgumentNullException.ThrowIfNull(almacen);

            NormalizarDatos(almacen);
            ValidarDatos(almacen);

            await ValidarSucursalAsync(almacen.SucursalId);

            bool duplicado = await _repository.ExisteCodigoAsync(
                almacen.SucursalId,
                almacen.Codigo);

            if (duplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe un almacén con ese código " +
                    "dentro de la sucursal seleccionada.");
            }

            almacen.Activo = true;

            await _repository.AgregarAsync(almacen);
        }

        // ========================================
        // ACTUALIZAR ALMACEN
        // ========================================

        public async Task ActualizarAsync(Almacen almacen)
        {
            ArgumentNullException.ThrowIfNull(almacen);

            if (almacen.Id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del almacén no es válido.");
            }

            var existente = await _repository
                .ObtenerPorIdAsync(almacen.Id);

            if (existente == null)
            {
                throw new KeyNotFoundException(
                    "El almacén no existe.");
            }

            if (!existente.Activo)
            {
                throw new InvalidOperationException(
                    "No se puede actualizar un almacén inactivo.");
            }

            NormalizarDatos(almacen);
            ValidarDatos(almacen);

            await ValidarSucursalAsync(almacen.SucursalId);

            // Impedir trasladar un almacén con lotes
            // a otra sucursal, para conservar
            // la integridad del historial.

            if (existente.SucursalId != almacen.SucursalId)
            {
                bool tieneLotes =
                    await _repository.TieneLotesAsync(almacen.Id);

                if (tieneLotes)
                {
                    throw new InvalidOperationException(
                        "No se puede cambiar de sucursal " +
                        "un almacén que tiene lotes asociados.");
                }
            }

            bool duplicado = await _repository.ExisteCodigoAsync(
                almacen.SucursalId,
                almacen.Codigo,
                excluirId: almacen.Id);

            if (duplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe otro almacén con ese código " +
                    "dentro de la sucursal seleccionada.");
            }

            existente.SucursalId = almacen.SucursalId;
            existente.Codigo = almacen.Codigo;
            existente.Nombre = almacen.Nombre;
            existente.Ubicacion = almacen.Ubicacion;

            await _repository.ActualizarAsync(existente);
        }

        // ========================================
        // DESACTIVAR ALMACEN
        // ========================================

        public async Task DesactivarAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del almacén no es válido.");
            }

            var almacen = await _repository.ObtenerPorIdAsync(id);

            if (almacen == null)
            {
                throw new KeyNotFoundException(
                    "El almacén no existe.");
            }

            if (!almacen.Activo)
            {
                throw new InvalidOperationException(
                    "El almacén ya se encuentra inactivo.");
            }

            // Por ahora se impide la desactivación
            // si existe cualquier lote asociado.

            bool tieneLotes =
                await _repository.TieneLotesAsync(id);

            if (tieneLotes)
            {
                throw new InvalidOperationException(
                    "No se puede desactivar un almacén " +
                    "que tiene lotes asociados.");
            }

            almacen.Activo = false;

            await _repository.ActualizarAsync(almacen);
        }

        // ========================================
        // VALIDAR SUCURSAL
        // ========================================

        private async Task ValidarSucursalAsync(
            int sucursalId)
        {
            bool disponible =
                await _repository.SucursalDisponibleAsync(
                    sucursalId);

            if (!disponible)
            {
                throw new ArgumentException(
                    "La sucursal seleccionada no existe " +
                    "o se encuentra inactiva.");
            }
        }

        // ========================================
        // NORMALIZAR DATOS
        // ========================================

        private static void NormalizarDatos(
            Almacen almacen)
        {
            almacen.Codigo =
                (almacen.Codigo ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();

            almacen.Nombre =
                (almacen.Nombre ?? string.Empty).Trim();

            almacen.Ubicacion =
                string.IsNullOrWhiteSpace(almacen.Ubicacion)
                    ? null
                    : almacen.Ubicacion.Trim();
        }

        // ========================================
        // VALIDAR DATOS
        // ========================================

        private static void ValidarDatos(
            Almacen almacen)
        {
            var resultados = new List<ValidationResult>();

            bool valido = Validator.TryValidateObject(
                almacen,
                new ValidationContext(almacen),
                resultados,
                validateAllProperties: true);

            if (!valido)
            {
                throw new ArgumentException(
                    resultados.FirstOrDefault()?.ErrorMessage
                    ?? "Los datos del almacén no son válidos.");
            }
        }
    }
}
