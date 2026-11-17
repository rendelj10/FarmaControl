
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface IMedicamentoService
    {
        Task<List<Medicamento>> ObtenerTodasAsync();

        Task<Medicamento?> ObtenerPorIdAsync(int id);

        Task CrearAsync(Medicamento medicamento);

        Task ActualizarAsync(Medicamento medicamento);

        Task DesactivarAsync(int id);
    }
}
