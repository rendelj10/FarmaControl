
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class VentaGuardarDto
    {
        // ========================================
        // CLIENTE
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un cliente válido.")]
        public int ClienteId { get; set; }

        // ========================================
        // ALMACEN
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un almacén válido.")]
        public int AlmacenId { get; set; }

        // ========================================
        // DOCUMENTO
        // ========================================

        [Required(ErrorMessage = "El documento es obligatorio.")]
        [StringLength(50)]
        public string NumeroDocumento { get; set; }
            = string.Empty;

        // ========================================
        // FECHA
        // ========================================

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaVenta { get; set; }
            = Proyecto_FarmaControl.Utilidades.FechaNicaragua.Hoy;

        // ========================================
        // OBSERVACIONES
        // ========================================

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // ========================================
        // DETALLES
        // ========================================

        [Required]
        [MinLength(1,
            ErrorMessage = "La venta debe contener al menos un detalle.")]
        public List<DetalleVentaGuardarDto> Detalles
        { get; set; } = new();
    }
}
