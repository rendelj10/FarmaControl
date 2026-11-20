
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class DetalleAjusteInventario
    {
        public int Id { get; set; }

        // AJUSTE AL QUE PERTENECE EL DETALLE

        [Range(1, int.MaxValue)]
        public int AjusteInventarioId { get; set; }

        public AjusteInventario AjusteInventario
        { get; set; } = null!;

        // LOTE QUE SE ESTÁ CONTANDO

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un lote válido.")]
        public int LoteId { get; set; }

        public Lote Lote { get; set; } = null!;

        // EXISTENCIA REGISTRADA EN EL SISTEMA

        [Range(0, int.MaxValue,
            ErrorMessage = "La existencia no puede ser negativa.")]
        public int ExistenciaSistema { get; set; }

        // CANTIDAD CONTADA FÍSICAMENTE

        [Range(0, int.MaxValue,
            ErrorMessage = "La cantidad física no puede ser negativa.")]
        public int CantidadFisica { get; set; }

        // DIFERENCIA CALCULADA POR EL SERVICIO
        // CantidadFisica - ExistenciaSistema

        public int Diferencia { get; set; }

        // OBSERVACIÓN DEL CONTEO

        [StringLength(250)]
        public string? Observacion { get; set; }
    }
}
