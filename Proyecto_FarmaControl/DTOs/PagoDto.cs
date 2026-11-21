namespace Proyecto_FarmaControl.DTOs
{
    public class PagoDto
    {
        public int Id { get; set; }
        public int? CompraId { get; set; }
        public int? VentaId { get; set; }
        public string Documento { get; set; } = string.Empty;
        public int MetodoPagoId { get; set; }
        public string MetodoPagoNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public int? TurnoCajaId { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
        public string? Referencia { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }
}
