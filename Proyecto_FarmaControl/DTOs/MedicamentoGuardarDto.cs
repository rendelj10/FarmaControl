
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class MedicamentoGuardarDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(150)]
        public string? PrincipioActivo { get; set; }

        [StringLength(100)]
        public string? Concentracion { get; set; }

        [StringLength(100)]
        public string? FormaFarmaceutica { get; set; }

        // Campo antiguo conservado temporalmente.
        [StringLength(100)]
        public string? Presentacion { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar una categoría válida.")]
        public int CategoriaId { get; set; }

        // Nuevas relaciones opcionales.
        public int? LaboratorioId { get; set; }

        public int? PresentacionId { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999.99",
            ErrorMessage = "El precio de venta debe ser mayor o igual a cero.")]
        public decimal PrecioVenta { get; set; } = 0m;
    }
}
