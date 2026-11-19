
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface ILoteService
    {
        Task<List<Lote>> ObtenerTodasAsync();

        Task<Lote?> ObtenerPorIdAsync(int id);

        Task CrearAsync(Lote lote);

        Task ActualizarAsync(Lote lote);

        Task DesactivarAsync(int id);
    }
}
