
using System.ComponentModel.DataAnnotations;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IProveedorRepository _repository;

        public ProveedorService(
            IProveedorRepository repository)
        {
            _repository = repository;
        }

        // ========================================
        // 1. CONSULTAR TODOS LOS PROVEEDORES
        // ========================================

        public async Task<List<Proveedor>> ObtenerTodosAsync()
        {
            return await _repository.ObtenerTodosAsync();
        }

        // ========================================
        // 2. CONSULTAR PROVEEDOR POR ID
        // ========================================

        public async Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _repository.ObtenerPorIdAsync(id);
        }

        // ========================================
        // 3. PAGINACION SQL PARA DATATABLES
        // ========================================

        public async Task<DataTablesResponseDto<ProveedorDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await _repository
                .ObtenerPaginadasAsync(request);
        }

        // ========================================
        // 4. CREAR PROVEEDOR
        // ========================================

        public async Task CrearAsync(Proveedor proveedor)
        {
            ArgumentNullException.ThrowIfNull(proveedor);

            NormalizarDatos(proveedor);

            ValidarDatos(proveedor);

            bool codigoDuplicado =
                await _repository.ExisteCodigoAsync(
                    proveedor.Codigo);

            if (codigoDuplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe un proveedor con ese código.");
            }

            proveedor.Activo = true;

            await _repository.AgregarAsync(proveedor);
        }

        // ========================================
        // 5. ACTUALIZAR PROVEEDOR
        // ========================================

        public async Task ActualizarAsync(Proveedor proveedor)
        {
            ArgumentNullException.ThrowIfNull(proveedor);

            if (proveedor.Id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del proveedor no es válido.");
            }

            // Recuperar el registro existente.
            // Entity Framework realiza el seguimiento
            // de los cambios sobre esta entidad.

            var existente = await _repository
                .ObtenerPorIdAsync(proveedor.Id);

            if (existente == null)
            {
                throw new KeyNotFoundException(
                    "El proveedor no existe.");
            }

            if (!existente.Activo)
            {
                throw new InvalidOperationException(
                    "No se puede actualizar un proveedor inactivo.");
            }

            NormalizarDatos(proveedor);

            ValidarDatos(proveedor);

            // Comprobar que el código no esté
            // asignado a otro proveedor.

            bool codigoDuplicado =
                await _repository.ExisteCodigoAsync(
                    proveedor.Codigo,
                    excluirId: proveedor.Id);

            if (codigoDuplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe otro proveedor con ese código.");
            }

            // Actualizar únicamente los campos
            // permitidos para este módulo.

            existente.Codigo = proveedor.Codigo;

            existente.RazonSocial = proveedor.RazonSocial;

            existente.NombreComercial =
                proveedor.NombreComercial;

            existente.Ruc = proveedor.Ruc;

            existente.Contacto = proveedor.Contacto;

            existente.Telefono = proveedor.Telefono;

            existente.Email = proveedor.Email;

            existente.Direccion = proveedor.Direccion;

            // No modificar Id ni Activo desde
            // la operación de actualización.

            await _repository.ActualizarAsync(existente);
        }

        // ========================================
        // 6. DESACTIVAR PROVEEDOR
        // ========================================

        public async Task DesactivarAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del proveedor no es válido.");
            }

            // Recuperar el proveedor existente.

            var proveedor = await _repository
                .ObtenerPorIdAsync(id);

            if (proveedor == null)
            {
                throw new KeyNotFoundException(
                    "El proveedor no existe.");
            }

            // Verificar que esté activo.

            if (!proveedor.Activo)
            {
                throw new InvalidOperationException(
                    "El proveedor ya se encuentra inactivo.");
            }

            // ====================================
            // NUEVA VALIDACION:
            // COMPRAS PENDIENTES DE RECEPCION
            // ====================================

            bool tieneComprasPendientes =
                await _repository
                    .TieneComprasPendientesAsync(id);

            if (tieneComprasPendientes)
            {
                throw new InvalidOperationException(
                    "No se puede desactivar el proveedor " +
                    "porque tiene compras pendientes " +
                    "de recepción.");
            }

            // ====================================
            // DESACTIVACION LOGICA
            // ====================================

            // Si no tiene compras en Borrador,
            // se permite desactivarlo.
            //
            // No se elimina físicamente.
            // Se conservan las compras históricas.

            proveedor.Activo = false;

            await _repository.ActualizarAsync(proveedor);
        }

        // ========================================
        // 7. NORMALIZAR DATOS
        // ========================================

        private static void NormalizarDatos(
            Proveedor proveedor)
        {
            proveedor.Codigo =
                (proveedor.Codigo ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();

            proveedor.RazonSocial =
                (proveedor.RazonSocial ?? string.Empty)
                    .Trim();

            proveedor.NombreComercial =
                NormalizarOpcional(
                    proveedor.NombreComercial);

            proveedor.Ruc =
                NormalizarOpcional(
                    proveedor.Ruc);

            proveedor.Contacto =
                NormalizarOpcional(
                    proveedor.Contacto);

            proveedor.Telefono =
                NormalizarOpcional(
                    proveedor.Telefono);

            proveedor.Email =
                NormalizarOpcional(
                    proveedor.Email)?.ToLowerInvariant();

            proveedor.Direccion =
                NormalizarOpcional(
                    proveedor.Direccion);
        }

        // ========================================
        // 8. NORMALIZAR CAMPOS OPCIONALES
        // ========================================

        private static string? NormalizarOpcional(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }

        // ========================================
        // 9. VALIDAR DATOS DEL PROVEEDOR
        // ========================================

        private static void ValidarDatos(
            Proveedor proveedor)
        {
            var resultados =
                new List<ValidationResult>();

            bool valido = Validator.TryValidateObject(
                proveedor,
                new ValidationContext(proveedor),
                resultados,
                validateAllProperties: true);

            if (!valido)
            {
                string mensaje =
                    resultados.FirstOrDefault()?.ErrorMessage
                    ?? "Los datos del proveedor no son válidos.";

                throw new ArgumentException(mensaje);
            }

            if (string.IsNullOrWhiteSpace(proveedor.Codigo))
            {
                throw new ArgumentException(
                    "El código del proveedor es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(
                proveedor.RazonSocial))
            {
                throw new ArgumentException(
                    "La razón social del proveedor es obligatoria.");
            }
        }
    }
}
