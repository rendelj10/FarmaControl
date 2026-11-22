using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class TurnoCaja
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int SucursalId { get; set; }
        public Sucursal Sucursal { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int UsuarioAperturaId { get; set; }
        public Usuario UsuarioApertura { get; set; } = null!;

        public int? UsuarioCierreId { get; set; }
        public Usuario? UsuarioCierre { get; set; }

        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal MontoInicial { get; set; }

        public decimal? MontoEsperado { get; set; }
        public decimal? MontoContado { get; set; }
        public decimal? Diferencia { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Abierto";

        [StringLength(250)]
        public string? Observaciones { get; set; }
    }
}
