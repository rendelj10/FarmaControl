
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Medicamento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El codigo es obligatorio")]
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

        // Campo antiguo: se conserva temporalmente
        // para migrar los datos existentes.
        [StringLength(100)]
        public string? Presentacion { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        public Categoria Categoria { get; set; } = null!;

        // Relacion con laboratorio fabricante.
        public int? LaboratorioId { get; set; }

        public Laboratorio? Laboratorio { get; set; }

        // Relacion con el catalogo de presentaciones.
        public int? PresentacionId { get; set; }

        public Presentacion? PresentacionCatalogo { get; set; }

        // Precio de venta vigente.
        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal PrecioVenta { get; set; } = 0m;

        public bool Activo { get; set; } = true;
    }
}
