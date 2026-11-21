
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Lote
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un medicamento.")]
        public int MedicamentoId { get; set; }

        public Medicamento Medicamento { get; set; } = null!;

        [Required(ErrorMessage = "El número de lote es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string NumeroLote { get; set; } = string.Empty;

        // Relación con la tabla Almacenes
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un almacén válido.")]
        public int AlmacenId { get; set; }

        public Almacen Almacen { get; set; } = null!;

        [Range(1, int.MaxValue,
            ErrorMessage = "La cantidad inicial debe ser mayor que cero.")]
        public int CantidadInicial { get; set; }

        [Range(0, int.MaxValue,
            ErrorMessage = "Las existencias no pueden ser negativas.")]
        public int Existencias { get; set; }

        [Range(typeof(decimal), "0.01", "999999999.99",
            ErrorMessage = "El costo unitario debe ser mayor que cero.")]
        public decimal CostoUnitario { get; set; }

        public DateTime FechaIngreso { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public bool Activo { get; set; } = true;
    }
}
