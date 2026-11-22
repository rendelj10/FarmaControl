
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Almacen
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar una sucursal válida.")]
        public int SucursalId { get; set; }

        [Required(ErrorMessage = "El código del almacén es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del almacén es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string? Ubicacion { get; set; }

        public bool Activo { get; set; } = true;

        // Cada almacén pertenece a una sucursal.
        public Sucursal Sucursal { get; set; } = null!;
    }
}
