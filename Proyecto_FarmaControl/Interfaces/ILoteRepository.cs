
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface ILoteRepository
    {
        Task<List<Lote>> ObtenerTodasAsync();

        Task<Lote?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Lote lote);

        Task ActualizarAsync(Lote lote);
    }
}
