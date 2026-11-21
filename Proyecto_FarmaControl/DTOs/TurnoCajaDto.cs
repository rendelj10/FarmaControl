namespace Proyecto_FarmaControl.DTOs
{
    public class TurnoCajaDto
    {
        public int Id { get; set; }
        public int SucursalId { get; set; }
        public string SucursalNombre { get; set; } = string.Empty;
        public int UsuarioAperturaId { get; set; }
        public string UsuarioAperturaNombre { get; set; } = string.Empty;
        public int? UsuarioCierreId { get; set; }
        public string? UsuarioCierreNombre { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal? MontoEsperado { get; set; }
        public decimal? MontoContado { get; set; }
        public decimal? Diferencia { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }
}
