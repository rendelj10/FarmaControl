using Proyecto_FarmaControl.DTOs;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Repositorio
{
    public interface IMetodoPagoRepository
    {
        Task<List<MetodoPago>> ObtenerTodosAsync();

        Task<MetodoPago?> ObtenerPorIdAsync(int id);

        Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null);

        Task AgregarAsync(MetodoPago metodoPago);

        Task ActualizarAsync(MetodoPago metodoPago);

        Task<DataTablesResponseDto<MetodoPagoDto>> ObtenerPaginadosAsync(
            DataTablesRequestDto request);
    }
}
