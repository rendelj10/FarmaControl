
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
        // 1. CONSULTAR TODOS
        // ========================================

        public async Task<List<Almacen>> ObtenerTodosAsync()
        {
            return await _repository.ObtenerTodosAsync();
        }

        // ========================================
        // 2. CONSULTAR POR ID
        // ========================================

        public async Task<Almacen?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        // ========================================
        // 3. PAGINACION
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
        // 4. CREAR ALMACEN
        // ========================================

        public async Task CrearAsync(Almacen almacen)
        {
            ArgumentNullException.ThrowIfNull(almacen);

            NormalizarDatos(almacen);

            ValidarDatos(almacen);

            await ValidarSucursalAsync(
                almacen.SucursalId);

            bool duplicado =
                await _repository.ExisteCodigoAsync(
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
        // 5. ACTUALIZAR ALMACEN
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

            await ValidarSucursalAsync(
                almacen.SucursalId);

            // ====================================
            // CAMBIO DE SUCURSAL
            // ====================================

            if (existente.SucursalId != almacen.SucursalId)
            {
                // Validacion existente:
                // no mover almacenes con lotes.

                bool tieneLotes =
                    await _repository.TieneLotesAsync(
                        almacen.Id);

                if (tieneLotes)
                {
                    throw new InvalidOperationException(
                        "No se puede cambiar de sucursal " +
                        "un almacén que tiene lotes asociados.");
                }

                // NUEVA VALIDACION:
                // no mover almacenes con compras
                // que aun no se han recibido.

                bool comprasPendientes =
                    await _repository
                        .TieneComprasPendientesAsync(
                            almacen.Id);

                if (comprasPendientes)
                {
                    throw new InvalidOperationException(
                        "No se puede cambiar de sucursal " +
                        "un almacén que tiene compras " +
                        "pendientes de recepción.");
                }
            }

            // ====================================
            // CODIGO DUPLICADO
            // ====================================

            bool duplicado =
                await _repository.ExisteCodigoAsync(
                    almacen.SucursalId,
                    almacen.Codigo,
                    excluirId: almacen.Id);

            if (duplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe otro almacén con ese código " +
                    "dentro de la sucursal seleccionada.");
            }

            // ====================================
            // ACTUALIZAR CAMPOS PERMITIDOS
            // ====================================

            existente.SucursalId = almacen.SucursalId;
            existente.Codigo = almacen.Codigo;
            existente.Nombre = almacen.Nombre;
            existente.Ubicacion = almacen.Ubicacion;

            await _repository.ActualizarAsync(existente);
        }

        // ========================================
        // 6. DESACTIVAR ALMACEN
        // ========================================

        public async Task DesactivarAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del almacén no es válido.");
            }

            var almacen = await _repository
                .ObtenerPorIdAsync(id);

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

            // ====================================
            // NUEVA VALIDACION:
            // COMPRAS PENDIENTES
            // ====================================

            bool comprasPendientes =
                await _repository
                    .TieneComprasPendientesAsync(id);

            if (comprasPendientes)
            {
                throw new InvalidOperationException(
                    "No se puede desactivar el almacén " +
                    "porque tiene compras pendientes " +
                    "de recepción.");
            }

            // ====================================
            // VALIDACION EXISTENTE:
            // LOTES ASOCIADOS
            // ====================================

            bool tieneLotes =
                await _repository.TieneLotesAsync(id);

            if (tieneLotes)
            {
                throw new InvalidOperationException(
                    "No se puede desactivar un almacén " +
                    "que tiene lotes asociados.");
            }

            // ====================================
            // DESACTIVACION LOGICA
            // ====================================

            almacen.Activo = false;

            await _repository.ActualizarAsync(almacen);
        }

        // ========================================
        // 7. VALIDAR SUCURSAL
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
        // 8. NORMALIZAR DATOS
        // ========================================

        private static void NormalizarDatos(
            Almacen almacen)
        {
            almacen.Codigo =
                (almacen.Codigo ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();

            almacen.Nombre =
                (almacen.Nombre ?? string.Empty)
                    .Trim();

            almacen.Ubicacion =
                string.IsNullOrWhiteSpace(
                    almacen.Ubicacion)
                    ? null
                    : almacen.Ubicacion.Trim();
        }

        // ========================================
        // 9. VALIDAR DATOS
        // ========================================

        private static void ValidarDatos(
            Almacen almacen)
        {
            var resultados =
                new List<ValidationResult>();

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
