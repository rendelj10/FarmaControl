
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface IMedicamentoRepository
    {
        Task<List<Medicamento>> ObtenerTodasAsync();

        Task<Medicamento?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Medicamento medicamento);

        Task ActualizarAsync(Medicamento medicamento);

        Task<bool> ExisteCodigoAsync(
            string codigo, int? excluirId = null);
    }
}
