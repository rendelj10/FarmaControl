using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class PresentacionGuardarDto
    {
        [Required(ErrorMessage = "El nombre de la presentación es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descripcion { get; set; }

        public decimal? CantidadContenido { get; set; }

        [StringLength(30)]
        public string? UnidadContenido { get; set; }
    }
}
