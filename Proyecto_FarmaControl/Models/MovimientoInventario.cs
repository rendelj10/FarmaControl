
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class MovimientoInventario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string TipoMovimiento { get; set; } = string.Empty;

        public int? AlmacenOrigenId { get; set; }
        public Almacen? AlmacenOrigen { get; set; }

        public int? AlmacenDestinoId { get; set; }
        public Almacen? AlmacenDestino { get; set; }

        [Range(1, int.MaxValue)]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int? CompraId { get; set; }
        public Compra? Compra { get; set; }

        public int? VentaId { get; set; }
        public Venta? Venta { get; set; }

        public int? AjusteInventarioId { get; set; }
        public AjusteInventario? AjusteInventario { get; set; }

        public DateTime FechaMovimiento { get; set; }

        [StringLength(150)]
        public string? Motivo { get; set; }

        [StringLength(100)]
        public string? Referencia { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Borrador";

        [StringLength(250)]
        public string? Observaciones { get; set; }

        public ICollection<DetalleMovimiento> Detalles { get; set; }
            = new List<DetalleMovimiento>();
    }
}
