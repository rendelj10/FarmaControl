
using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IMedicamentoRepository
    {
        Task<DataTablesResponseDto<MedicamentoDto>>
            ObtenerPaginadosAsync(DataTablesRequestDto request);

        Task<IReadOnlyList<Medicamento>>
            ObtenerTodasAsync();

        Task<Medicamento?> ObtenerPorIdAsync(int id);

        Task<bool> ExisteCodigoAsync(
            string codigo, int? excluirId = null);

        Task<Medicamento> AgregarAsync(Medicamento medicamento);

        Task<bool> ActualizarAsync(Medicamento medicamento);
    }
}
