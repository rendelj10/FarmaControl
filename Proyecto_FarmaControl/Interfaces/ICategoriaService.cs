
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> ObtenerTodasAsync();

        Task<Categoria?> ObtenerPorIdAsync(int id);

        Task CrearAsync(Categoria categoria);

        Task ActualizarAsync(Categoria categoria);

        Task DesactivarAsync(int id);
    }
}
