namespace Proyecto_FarmaControl.DTOs
{
    public class MetodoPagoDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool RequiereReferencia { get; set; }

        public bool Activo { get; set; }
    }
}
