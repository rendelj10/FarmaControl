
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class AlmacenGuardarDto
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar una sucursal válida.")]
        public int SucursalId { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Ubicacion { get; set; }
    }
}
