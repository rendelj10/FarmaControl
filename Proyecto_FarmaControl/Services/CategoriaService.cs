
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Models;
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repository;

        public CategoriaService(ICategoriaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Categoria>> ObtenerTodasAsync()
        {
            return await _repository.ObtenerTodasAsync();
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return await _repository.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Categoria categoria)
        {
            var nombre = categoria.Nombre?.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre de la categoría es obligatorio.");

            categoria.Nombre = nombre;

            if (await _repository.ExisteNombreAsync(categoria.Nombre))
                throw new InvalidOperationException(
                    "Ya existe una categoría con ese nombre.");

            categoria.Activo = true;

            await _repository.AgregarAsync(categoria);
        }

        public async Task ActualizarAsync(Categoria categoria)
        {
            var existente = await _repository
                .ObtenerPorIdAsync(categoria.Id);

            if (existente == null)
                throw new KeyNotFoundException(
                    "La categoría no existe.");

            var nombre = categoria.Nombre?.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre de la categoría es obligatorio.");

            categoria.Nombre = nombre;

            if (await _repository.ExisteNombreAsync(
                categoria.Nombre, categoria.Id))
                throw new InvalidOperationException(
                    "Ya existe una categoría con ese nombre.");

            existente.Nombre = categoria.Nombre;
            existente.Descripcion = categoria.Descripcion;

            await _repository.ActualizarAsync(existente);
        }

        public async Task DesactivarAsync(int id)
        {
            var categoria = await _repository
                .ObtenerPorIdAsync(id);

            if (categoria == null)
                throw new KeyNotFoundException(
                    "La categoría no existe.");

            categoria.Activo = false;

            await _repository.ActualizarAsync(categoria);
        }

        // =====================================
        // DATATABLES - CATEGORIAS PAGINADAS
        // =====================================

        public async Task<DataTablesResponseDto<CategoriaTablaDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request)
        {
            return await _repository.ObtenerPaginadasAsync(request);
        }
    }
}
