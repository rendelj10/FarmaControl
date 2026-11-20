
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;

namespace Proyecto_FarmaControl.Services
{
    public class MedicamentoService : IMedicamentoService
    {
        private readonly IMedicamentoRepository _repository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ILaboratorioRepository _laboratorioRepository;
        private readonly IPresentacionRepository _presentacionRepository;

        public MedicamentoService(
            IMedicamentoRepository repository,
            ICategoriaRepository categoriaRepository,
            ILaboratorioRepository laboratorioRepository,
            IPresentacionRepository presentacionRepository)
        {
            _repository = repository;
            _categoriaRepository = categoriaRepository;
            _laboratorioRepository = laboratorioRepository;
            _presentacionRepository = presentacionRepository;
        }

        // =====================================
        // OBTENER TODOS
        // =====================================

        public async Task<IReadOnlyList<MedicamentoDto>>
            ObtenerTodasAsync()
        {
            var medicamentos = await _repository.ObtenerTodasAsync();

            return medicamentos.Select(m => new MedicamentoDto
            {
                Id = m.Id,
                Codigo = m.Codigo,
                Nombre = m.Nombre,
                PrincipioActivo = m.PrincipioActivo,
                Concentracion = m.Concentracion,
                FormaFarmaceutica = m.FormaFarmaceutica,
                Presentacion = m.Presentacion,
                CategoriaId = m.CategoriaId,
                CategoriaNombre = m.Categoria?.Nombre ?? string.Empty,
                LaboratorioId = m.LaboratorioId,
                LaboratorioNombre = m.Laboratorio?.Nombre,
                PresentacionId = m.PresentacionId,
                PresentacionNombre = m.PresentacionCatalogo?.Nombre,
                PrecioVenta = m.PrecioVenta,
                Activo = m.Activo
            }).ToList();
        }

        // =====================================
        // OBTENER POR ID
        // =====================================

        public async Task<MedicamentoGuardarDto?>
            ObtenerPorIdAsync(int id)
        {
            var medicamento = await _repository.ObtenerPorIdAsync(id);

            if (medicamento == null)
                return null;

            return new MedicamentoGuardarDto
            {
                Id = medicamento.Id,
                Codigo = medicamento.Codigo,
                Nombre = medicamento.Nombre,
                PrincipioActivo = medicamento.PrincipioActivo,
                Concentracion = medicamento.Concentracion,
                FormaFarmaceutica = medicamento.FormaFarmaceutica,
                Presentacion = medicamento.Presentacion,
                CategoriaId = medicamento.CategoriaId,
                LaboratorioId = medicamento.LaboratorioId,
                PresentacionId = medicamento.PresentacionId,
                PrecioVenta = medicamento.PrecioVenta
            };
        }

        // =====================================
        // DATATABLES
        // =====================================

        public Task<DataTablesResponseDto<MedicamentoDto>>
            ObtenerPaginadosAsync(DataTablesRequestDto request)
        {
            return _repository.ObtenerPaginadosAsync(request);
        }

        // =====================================
        // CREAR MEDICAMENTO
        // =====================================

        public async Task<(bool Success, string? ErrorMessage)>
            CrearAsync(MedicamentoGuardarDto dto)
        {
            var error = ValidarDatos(dto);

            if (error != null)
                return (false, error);

            var codigo = dto.Codigo.Trim();

            var categoriaError =
                await ValidarCategoriaAsync(dto.CategoriaId);

            if (categoriaError != null)
                return (false, categoriaError);

            var laboratorioError =
                await ValidarLaboratorioAsync(dto.LaboratorioId);

            if (laboratorioError != null)
                return (false, laboratorioError);

            var presentacionError =
                await ValidarPresentacionAsync(dto.PresentacionId);

            if (presentacionError != null)
                return (false, presentacionError);

            if (await _repository.ExisteCodigoAsync(codigo))
                return (false,
                    "Ya existe un medicamento con ese código.");

            var medicamento = new Medicamento
            {
                Codigo = codigo,
                Nombre = dto.Nombre.Trim(),
                PrincipioActivo = NormalizarTexto(dto.PrincipioActivo),
                Concentracion = NormalizarTexto(dto.Concentracion),
                FormaFarmaceutica = NormalizarTexto(dto.FormaFarmaceutica),

                // Campo antiguo conservado temporalmente.
                Presentacion = NormalizarTexto(dto.Presentacion),

                CategoriaId = dto.CategoriaId,
                LaboratorioId = NormalizarId(dto.LaboratorioId),
                PresentacionId = NormalizarId(dto.PresentacionId),
                PrecioVenta = dto.PrecioVenta,
                Activo = true
            };

            await _repository.AgregarAsync(medicamento);

            return (true, null);
        }

        // =====================================
        // ACTUALIZAR MEDICAMENTO
        // =====================================

        public async Task<(bool Success, string? ErrorMessage)>
            ActualizarAsync(MedicamentoGuardarDto dto)
        {
            if (!dto.Id.HasValue || dto.Id.Value <= 0)
                return (false,
                    "El identificador del medicamento no es válido.");

            var existente = await _repository
                .ObtenerPorIdAsync(dto.Id.Value);

            if (existente == null)
                return (false, "El medicamento no existe.");

            var error = ValidarDatos(dto);

            if (error != null)
                return (false, error);

            var codigo = dto.Codigo.Trim();

            var categoriaError =
                await ValidarCategoriaAsync(dto.CategoriaId);

            if (categoriaError != null)
                return (false, categoriaError);

            var laboratorioError =
                await ValidarLaboratorioAsync(dto.LaboratorioId);

            if (laboratorioError != null)
                return (false, laboratorioError);

            var presentacionError =
                await ValidarPresentacionAsync(dto.PresentacionId);

            if (presentacionError != null)
                return (false, presentacionError);

            if (await _repository.ExisteCodigoAsync(
                codigo, dto.Id.Value))
            {
                return (false,
                    "Ya existe otro medicamento con ese código.");
            }

            existente.Codigo = codigo;
            existente.Nombre = dto.Nombre.Trim();

            existente.PrincipioActivo =
                NormalizarTexto(dto.PrincipioActivo);

            existente.Concentracion =
                NormalizarTexto(dto.Concentracion);

            existente.FormaFarmaceutica =
                NormalizarTexto(dto.FormaFarmaceutica);

            // No eliminar el dato antiguo si no se envía uno nuevo.
            existente.Presentacion =
                NormalizarTexto(dto.Presentacion) ?? existente.Presentacion;

            existente.CategoriaId = dto.CategoriaId;
            existente.LaboratorioId = NormalizarId(dto.LaboratorioId);
            existente.PresentacionId = NormalizarId(dto.PresentacionId);
            existente.PrecioVenta = dto.PrecioVenta;

            var actualizado =
                await _repository.ActualizarAsync(existente);

            return actualizado
                ? (true, null)
                : (false, "No se pudo actualizar el medicamento.");
        }

        // =====================================
        // DESACTIVAR MEDICAMENTO
        // =====================================

        public async Task<(bool Success, string? ErrorMessage)>
            DesactivarAsync(int id)
        {
            if (id <= 0)
                return (false,
                    "El identificador del medicamento no es válido.");

            var medicamento =
                await _repository.ObtenerPorIdAsync(id);

            if (medicamento == null)
                return (false, "El medicamento no existe.");

            if (!medicamento.Activo)
                return (false,
                    "El medicamento ya está desactivado.");

            medicamento.Activo = false;

            var actualizado =
                await _repository.ActualizarAsync(medicamento);

            return actualizado
                ? (true, null)
                : (false, "No se pudo desactivar el medicamento.");
        }

        // =====================================
        // VALIDACIONES
        // =====================================

        private static string? ValidarDatos(
            MedicamentoGuardarDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Codigo))
                return "El código del medicamento es obligatorio.";

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return "El nombre del medicamento es obligatorio.";

            if (dto.PrecioVenta < 0)
                return "El precio de venta no puede ser negativo.";

            return null;
        }

        private async Task<string?> ValidarCategoriaAsync(
            int categoriaId)
        {
            var categoria = await _categoriaRepository
                .ObtenerPorIdAsync(categoriaId);

            if (categoria == null)
                return "La categoría seleccionada no existe.";

            if (!categoria.Activo)
                return "La categoría seleccionada está inactiva.";

            return null;
        }

        private async Task<string?> ValidarLaboratorioAsync(
            int? laboratorioId)
        {
            if (!laboratorioId.HasValue || laboratorioId.Value <= 0)
                return null;

            var laboratorio = await _laboratorioRepository
                .ObtenerPorIdAsync(laboratorioId.Value);

            if (laboratorio == null)
                return "El laboratorio seleccionado no existe.";

            if (!laboratorio.Activo)
                return "El laboratorio seleccionado está inactivo.";

            return null;
        }

        private async Task<string?> ValidarPresentacionAsync(
            int? presentacionId)
        {
            if (!presentacionId.HasValue || presentacionId.Value <= 0)
                return null;

            var presentacion = await _presentacionRepository
                .ObtenerPorIdAsync(presentacionId.Value);

            if (presentacion == null)
                return "La presentación seleccionada no existe.";

            if (!presentacion.Activo)
                return "La presentación seleccionada está inactiva.";

            return null;
        }

        // =====================================
        // NORMALIZAR CAMPOS
        // =====================================

        private static int? NormalizarId(int? id)
        {
            return id.HasValue && id.Value > 0
                ? id.Value
                : null;
        }

        private static string? NormalizarTexto(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }
    }
}
