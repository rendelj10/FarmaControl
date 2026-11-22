namespace Proyecto_FarmaControl.DTOs
{
    public class ReporteFilaDto
    {
        public int Id { get; set; }
        // Identificadores opcionales para enlazar los movimientos con su operación origen.
        public int? VentaId { get; set; }
        public int? CompraId { get; set; }
        public int? AjusteInventarioId { get; set; }
        public DateTime Fecha { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string Relacionado { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal? Monto { get; set; }
        public int? Cantidad { get; set; }
        public DateTime? Vencimiento { get; set; }
    }

    public class ReporteResultadoDto
    {
        public int TotalRegistros { get; set; }
        public decimal? TotalMonetario { get; set; }
        public int? TotalCantidad { get; set; }
        public int Limite { get; set; }
        public List<ReporteFilaDto> Filas { get; set; } = new();
        public bool Truncado => TotalRegistros > Filas.Count;
    }
}
