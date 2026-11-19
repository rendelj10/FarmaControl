
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class AjusteInventarioGuardarDto
    {
        // ========================================
        // ALMACEN
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un almacén válido.")]
        public int AlmacenId { get; set; }

        // ========================================
        // DOCUMENTO
        // ========================================

        [Required(ErrorMessage =
            "El número de documento es obligatorio.")]
        [StringLength(50)]
        public string NumeroDocumento { get; set; }
            = string.Empty;

        // ========================================
        // FECHA
        // ========================================

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaAjuste { get; set; }
            = DateTime.Today;

        // ========================================
        // MOTIVO
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
        // DETALLES
        // ========================================

        [Required]
        [MinLength(1, ErrorMessage =
            "Debe registrar al menos un detalle.")]
        public List<DetalleAjusteInventarioGuardarDto>
            Detalles
        { get; set; } = new();
    }
}
