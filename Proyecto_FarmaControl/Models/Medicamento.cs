
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Medicamento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(30)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(150)]
        public string? PrincipioActivo { get; set; }

        [StringLength(100)]
        public string? Concentracion { get; set; }

        [StringLength(100)]
        public string? FormaFarmaceutica { get; set; }

        [StringLength(100)]
        public string? Presentacion { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        public Categoria Categoria { get; set; } = null!;

        public bool Activo { get; set; } = true;
    }
}
