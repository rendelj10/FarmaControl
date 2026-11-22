
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class DetalleAjusteInventarioGuardarDto
    {
        // LOTE QUE SE VA A CONTAR

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un lote válido.")]
        public int LoteId { get; set; }

        // CANTIDAD CONTADA FÍSICAMENTE

        [Range(0, int.MaxValue,
            ErrorMessage = "La cantidad física no puede ser negativa.")]
        public int CantidadFisica { get; set; }

        // OBSERVACIÓN DEL CONTEO

        [StringLength(250,
            ErrorMessage = "La observación no puede superar 250 caracteres.")]
        public string? Observacion { get; set; }
    }
}
