
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class DetalleMovimiento
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int MovimientoInventarioId { get; set; }

        public MovimientoInventario MovimientoInventario
        { get; set; } = null!;

        public int? LoteOrigenId { get; set; }
        public Lote? LoteOrigen { get; set; }

        public int? LoteDestinoId { get; set; }
        public Lote? LoteDestino { get; set; }

        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [StringLength(250)]
        public string? Observacion { get; set; }
    }
}
