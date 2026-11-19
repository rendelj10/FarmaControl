
using Proyecto_FarmaControl.DTOs;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface IMedicamentoService
    {
        Task<DataTablesResponseDto<MedicamentoDto>>
            ObtenerPaginadosAsync(DataTablesRequestDto request);

        Task<IReadOnlyList<MedicamentoDto>>
            ObtenerTodasAsync();

        Task<MedicamentoGuardarDto?>
            ObtenerPorIdAsync(int id);

        Task<(bool Success, string? ErrorMessage)>
            CrearAsync(MedicamentoGuardarDto dto);

        Task<(bool Success, string? ErrorMessage)>
            ActualizarAsync(MedicamentoGuardarDto dto);

        Task<(bool Success, string? ErrorMessage)>
            DesactivarAsync(int id);
    }
}
