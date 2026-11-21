
using System.ComponentModel.DataAnnotations;
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Service
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repository;

        public ClienteService(
            IClienteRepository repository)
        {
            _repository = repository;
        }

        // ========================================
        // 1. CONSULTAR TODOS LOS CLIENTES
        // ========================================

        public async Task<List<Cliente>> ObtenerTodosAsync()
        {
            return await _repository.ObtenerTodosAsync();
        }

        // ========================================
        // 2. CONSULTAR CLIENTE POR ID
        // ========================================

        public async Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _repository.ObtenerPorIdAsync(id);
        }

        // ========================================
        // 3. PAGINACION DATATABLES
        // ========================================

        public async Task<DataTablesResponseDto<ClienteDto>>
            ObtenerPaginadasAsync(
                DataTablesRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return await _repository
                .ObtenerPaginadasAsync(request);
        }

        // ========================================
        // 4. CREAR CLIENTE
        // ========================================

        public async Task CrearAsync(Cliente cliente)
        {
            ArgumentNullException.ThrowIfNull(cliente);

            // Normalizar los datos recibidos.

            NormalizarDatos(cliente);

            // Validar el modelo.

            ValidarDatos(cliente);

            // Verificar codigo duplicado.

            bool codigoDuplicado =
                await _repository.ExisteCodigoAsync(
                    cliente.Codigo);

            if (codigoDuplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe un cliente con ese código.");
            }

            // Registrar cliente activo.

            cliente.Activo = true;

            await _repository.AgregarAsync(cliente);
        }

        // ========================================
        // 5. ACTUALIZAR CLIENTE
        // ========================================

        public async Task ActualizarAsync(Cliente cliente)
        {
            ArgumentNullException.ThrowIfNull(cliente);

            if (cliente.Id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del cliente no es válido.");
            }

            // ====================================
            // CONSULTAR CLIENTE EXISTENTE
            // ====================================

            var existente = await _repository
                .ObtenerPorIdAsync(cliente.Id);

            if (existente == null)
            {
                throw new KeyNotFoundException(
                    "El cliente no existe.");
            }

            // ====================================
            // VERIFICAR CLIENTE ACTIVO
            // ====================================

            if (!existente.Activo)
            {
                throw new InvalidOperationException(
                    "No se puede actualizar un cliente inactivo.");
            }

            // ====================================
            // NORMALIZAR Y VALIDAR
            // ====================================

            NormalizarDatos(cliente);

            ValidarDatos(cliente);

            // ====================================
            // VERIFICAR CODIGO DUPLICADO
            // ====================================

            bool codigoDuplicado =
                await _repository.ExisteCodigoAsync(
                    cliente.Codigo,
                    excluirId: cliente.Id);

            if (codigoDuplicado)
            {
                throw new InvalidOperationException(
                    "Ya existe otro cliente con ese código.");
            }

            // ====================================
            // ACTUALIZAR CAMPOS PERMITIDOS
            // ====================================

            existente.Codigo = cliente.Codigo;

            existente.Nombre = cliente.Nombre;

            existente.DocumentoIdentidad =
                cliente.DocumentoIdentidad;

            existente.Telefono = cliente.Telefono;

            existente.Email = cliente.Email;

            existente.Direccion = cliente.Direccion;

            // No modificar Id ni Activo.

            await _repository.ActualizarAsync(existente);
        }

        // ========================================
        // 6. DESACTIVAR CLIENTE
        // ========================================

        public async Task DesactivarAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador del cliente no es válido.");
            }

            // ====================================
            // CONSULTAR CLIENTE
            // ====================================

            var cliente = await _repository
                .ObtenerPorIdAsync(id);

            if (cliente == null)
            {
                throw new KeyNotFoundException(
                    "El cliente no existe.");
            }

            // ====================================
            // VERIFICAR ESTADO ACTUAL
            // ====================================

            if (!cliente.Activo)
            {
                throw new InvalidOperationException(
                    "El cliente ya se encuentra inactivo.");
            }

            // ====================================
            // PROTEGER VENTAS PENDIENTES
            // ====================================

            bool tieneVentasPendientes =
                await _repository
                    .TieneVentasPendientesAsync(id);

            if (tieneVentasPendientes)
            {
                throw new InvalidOperationException(
                    "No se puede desactivar el cliente " +
                    "porque tiene ventas pendientes " +
                    "en estado Borrador.");
            }

            // ====================================
            // DESACTIVACION LOGICA
            // ====================================

            // No se elimina el cliente.
            // Se conserva su historial de ventas.
            //
            // Esta regla comprueba las ventas
            // pendientes antes de desactivar.
            // La concurrencia entre crear ventas
            // y desactivar clientes se revisara
            // por separado.

            cliente.Activo = false;

            await _repository.ActualizarAsync(cliente);
        }

        // ========================================
        // 7. NORMALIZAR DATOS
        // ========================================

        private static void NormalizarDatos(
            Cliente cliente)
        {
            cliente.Codigo =
                (cliente.Codigo ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();

            cliente.Nombre =
                (cliente.Nombre ?? string.Empty)
                    .Trim();

            cliente.DocumentoIdentidad =
                NormalizarOpcional(
                    cliente.DocumentoIdentidad);

            cliente.Telefono =
                NormalizarOpcional(
                    cliente.Telefono);

            cliente.Email =
                NormalizarOpcional(
                    cliente.Email)?.ToLowerInvariant();

            cliente.Direccion =
                NormalizarOpcional(
                    cliente.Direccion);
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
        // 9. VALIDAR DATOS
        // ========================================

        private static void ValidarDatos(
            Cliente cliente)
        {
            var resultados = new List<ValidationResult>();

            bool valido = Validator.TryValidateObject(
                cliente,
                new ValidationContext(cliente),
                resultados,
                validateAllProperties: true);

            if (!valido)
            {
                string mensaje =
                    resultados.FirstOrDefault()?.ErrorMessage
                    ?? "Los datos del cliente no son válidos.";

                throw new ArgumentException(mensaje);
            }

            if (string.IsNullOrWhiteSpace(cliente.Codigo))
            {
                throw new ArgumentException(
                    "El código del cliente es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                throw new ArgumentException(
                    "El nombre del cliente es obligatorio.");
            }
        }
    }
}
