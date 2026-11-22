
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class CompraGuardarDto
    {
        // ========================================
        // PROVEEDOR
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un proveedor válido.")]
        public int ProveedorId { get; set; }

        // ========================================
        // ALMACEN DESTINO
        // ========================================

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un almacén válido.")]
        public int AlmacenId { get; set; }

        // ========================================
        // DOCUMENTO
        // ========================================

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [StringLength(50,
            ErrorMessage = "Máximo 50 caracteres.")]
        public string NumeroDocumento { get; set; }
            = string.Empty;

        // ========================================
        // FECHA DE COMPRA
        // ========================================

        [Required(ErrorMessage = "La fecha de compra es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaCompra { get; set; }
            = Proyecto_FarmaControl.Utilidades.FechaNicaragua.Hoy;

        // ========================================
        // OBSERVACIONES
        // ========================================

        [StringLength(500,
            ErrorMessage = "Máximo 500 caracteres.")]
        public string? Observaciones { get; set; }

        // ========================================
        // DETALLES DE LA COMPRA
        // ========================================

        [Required]
        [MinLength(1,
            ErrorMessage = "La compra debe contener al menos un medicamento.")]
        public List<DetalleCompraGuardarDto> Detalles
        { get; set; } = new();
    }
}
