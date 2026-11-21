
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Sucursal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código de la sucursal es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la sucursal es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        public string? Direccion { get; set; }

        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        // Una sucursal puede tener varios almacenes.
        public ICollection<Almacen> Almacenes { get; set; }
            = new List<Almacen>();
    }
}
