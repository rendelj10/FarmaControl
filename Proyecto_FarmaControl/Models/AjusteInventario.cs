
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class AjusteInventario
    {
        public int Id { get; set; }

        // Almacén donde se realiza el ajuste
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un almacén válido.")]
        public int AlmacenId { get; set; }

        public Almacen Almacen { get; set; } = null!;

        // Usuario que registra el ajuste
        [Range(1, int.MaxValue)]
        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        // Usuario que confirma el ajuste
        public int? ConfirmadoPorUsuarioId { get; set; }

        public Usuario? ConfirmadoPorUsuario { get; set; }

        // Fechas
        public DateTime FechaAjuste { get; set; }
            = DateTime.UtcNow;

        public DateTime? FechaConfirmacion { get; set; }

        // Conteo o Correccion
        [Required]
        [StringLength(30)]
        public string TipoAjuste { get; set; } = "Conteo";

        // Motivo del ajuste
        [Required(ErrorMessage =
            "El motivo del ajuste es obligatorio.")]
        [StringLength(150)]
        public string Motivo { get; set; } = string.Empty;

        // Borrador, Confirmado o Anulado
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Borrador";

        // Observaciones generales
        [StringLength(250)]
        public string? Observaciones { get; set; }

        // Detalles del conteo físico
        public ICollection<DetalleAjusteInventario> Detalles
        { get; set; } =
            new List<DetalleAjusteInventario>();

        // Movimiento generado al confirmar
        public MovimientoInventario? MovimientoInventario
        { get; set; }
    }
}
