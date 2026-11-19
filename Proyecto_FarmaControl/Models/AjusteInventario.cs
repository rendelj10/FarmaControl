
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class AjusteInventario
    {
        public int Id { get; set; }

        // ========================================
        // ALMACEN
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un almacén válido.")]
        public int AlmacenId { get; set; }

        public Almacen Almacen { get; set; } = null!;

        // ========================================
        // DOCUMENTO DEL AJUSTE
        // ========================================

        [Required(ErrorMessage =
            "El número de documento es obligatorio.")]
        [StringLength(50)]
        public string NumeroDocumento { get; set; }
            = string.Empty;

        // ========================================
        // FECHAS
        // ========================================

        public DateTime FechaAjuste { get; set; }
            = DateTime.Today;

        public DateTime? FechaAplicacion { get; set; }

        // ========================================
        // ESTADO
        // Borrador, Aplicado, Anulado
        // ========================================

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Borrador";

        // ========================================
        // MOTIVO OBLIGATORIO
        // ========================================

        [Required(ErrorMessage =
            "El motivo del ajuste es obligatorio.")]
        [StringLength(200)]
        public string Motivo { get; set; }
            = string.Empty;

        // ========================================
        // OBSERVACIONES
        // ========================================

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // ========================================
        // DETALLES DEL AJUSTE
        // ========================================

        public ICollection<DetalleAjusteInventario> Detalles
        { get; set; }
            = new List<DetalleAjusteInventario>();
    }
}
