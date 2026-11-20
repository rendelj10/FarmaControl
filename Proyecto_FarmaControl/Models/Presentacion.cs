using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Presentacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la presentación es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        public string? Descripcion { get; set; }

        public decimal? CantidadContenido { get; set; }

        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        public string? UnidadContenido { get; set; }

        public bool Activo { get; set; } = true;
    }
}
