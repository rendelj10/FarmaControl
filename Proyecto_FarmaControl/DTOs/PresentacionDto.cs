namespace Proyecto_FarmaControl.DTOs
{
    public class PresentacionDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal? CantidadContenido { get; set; }

        public string? UnidadContenido { get; set; }

        public bool Activo { get; set; }
    }
}
