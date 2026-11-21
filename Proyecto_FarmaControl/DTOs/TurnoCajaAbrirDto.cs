using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class TurnoCajaAbrirDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una sucursal válida.")]
        public int SucursalId { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999.99",
            ErrorMessage = "El monto inicial debe ser mayor o igual a cero.")]
        public decimal MontoInicial { get; set; }

        [StringLength(250, ErrorMessage = "Las observaciones no pueden exceder 250 caracteres.")]
        public string? Observaciones { get; set; }
    }
}
