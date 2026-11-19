
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.Repositorio;


namespace Proyecto_FarmaControl.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repository;

        public CategoriaService(ICategoriaRepository repository)
        {
            _repository = repository;
        }

        // =====================================
        // OBTENER TODAS LAS CATEGORIAS
        // =====================================

        public async Task<IReadOnlyList<CategoriaTablaDto>>
            ObtenerTodasAsync()
        {
            var categorias = await _repository.ObtenerTodasAsync();

            return categorias.Select(c => new CategoriaTablaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo
            }).ToList();
        }

        // =====================================
        // OBTENER CATEGORIA POR ID
        // =====================================

        public async Task<CategoriaGuardarDto?>
            ObtenerPorIdAsync(int id)
        {
            var categoria = await _repository.ObtenerPorIdAsync(id);

            if (categoria == null)
                return null;

            return new CategoriaGuardarDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };
        }

        // =====================================
        // OBTENER CATEGORIAS PAGINADAS
        // =====================================

        public Task<DataTablesResponseDto<CategoriaTablaDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request)
        {
            return _repository.ObtenerPaginadasAsync(request);
        }

        // =====================================
        // CREAR CATEGORIA
        // =====================================

        public async Task<(bool Success, string? ErrorMessage)>
            CrearAsync(CategoriaGuardarDto dto)
        {
            var nombre = dto.Nombre?.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre de la categoría es obligatorio.");

            if (await _repository.ExisteNombreAsync(nombre))
                return (false, "Ya existe una categoría con ese nombre.");

            var categoria = new Categoria
            {
                Nombre = nombre,
                Descripcion = dto.Descripcion?.Trim(),
                Activo = true
            };

            var categoriaCreada =
                await _repository.AgregarAsync(categoria);

            if (categoriaCreada.Id <= 0)
                return (false, "No se pudo registrar la categoría.");

            return (true, null);
        }

        // =====================================
        // ACTUALIZAR CATEGORIA
        // =====================================

        public async Task<(bool Success, string? ErrorMessage)>
            ActualizarAsync(CategoriaGuardarDto dto)
        {
            if (!dto.Id.HasValue || dto.Id.Value <= 0)
                return (false, "El identificador de categoría no es válido.");

            var categoria =
                await _repository.ObtenerPorIdAsync(dto.Id.Value);

            if (categoria == null)
                return (false, "La categoría no existe.");

            var nombre = dto.Nombre?.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre de la categoría es obligatorio.");

            if (await _repository.ExisteNombreAsync(nombre, dto.Id))
                return (false, "Ya existe otra categoría con ese nombre.");

            categoria.Nombre = nombre;
            categoria.Descripcion = dto.Descripcion?.Trim();

            var actualizado =
                await _repository.ActualizarAsync(categoria);

            if (!actualizado)
                return (false, "No se pudo actualizar la categoría.");

            return (true, null);
        }

        // =====================================
        // DESACTIVAR CATEGORIA
        // =====================================

        public async Task<(bool Success, string? ErrorMessage)>
            DesactivarAsync(int id)
        {
            var categoria = await _repository.ObtenerPorIdAsync(id);

            if (categoria == null)
                return (false, "La categoría no existe.");

            if (!categoria.Activo)
                return (false, "La categoría ya está desactivada.");

            categoria.Activo = false;

            var actualizado =
                await _repository.ActualizarAsync(categoria);

            if (!actualizado)
                return (false, "No se pudo desactivar la categoría.");

            return (true, null);
        }
    }
}
