using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface ICategoriaService
    {
        Task<DataTablesResponseDto<CategoriaTablaDto>>
            ObtenerPaginadasAsync(DataTablesRequestDto request);

        Task<IReadOnlyList<CategoriaTablaDto>>
            ObtenerTodasAsync();
        Task<CategoriaGuardarDto?> ObtenerPorIdAsync(int id);

        Task<(bool Success, string? ErrorMessage)>
            CrearAsync(CategoriaGuardarDto dto);

        Task<(bool Success, string? ErrorMessage)>
            ActualizarAsync(CategoriaGuardarDto dto);

        Task<(bool Success, string? ErrorMessage)>
            DesactivarAsync(int id);
    }
}
