
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class LoteGuardarDto
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un medicamento.")]
        public int MedicamentoId { get; set; }

        [Required(ErrorMessage = "El número de lote es obligatorio.")]
        [StringLength(50,
            ErrorMessage = "Máximo 50 caracteres.")]
        public string NumeroLote { get; set; } = string.Empty;

        [Required(ErrorMessage = "El almacén es obligatorio.")]
        [StringLength(100,
            ErrorMessage = "Máximo 100 caracteres.")]
        public string Almacen { get; set; } = string.Empty;

        [Range(1, int.MaxValue,
            ErrorMessage = "La cantidad inicial debe ser mayor que cero.")]
        public int CantidadInicial { get; set; }

        [Range(typeof(decimal), "0.01", "999999999.99",
            ErrorMessage = "El costo unitario debe ser mayor que cero.")]
        public decimal CostoUnitario { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
        public DateTime FechaIngreso { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
        public DateTime FechaVencimiento { get; set; }
    }
}
