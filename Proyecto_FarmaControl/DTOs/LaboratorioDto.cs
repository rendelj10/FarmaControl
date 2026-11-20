namespace Proyecto_FarmaControl.DTOs
{
    public class LaboratorioDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Pais { get; set; }

        public string? Contacto { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public string? Direccion { get; set; }

        public bool Activo { get; set; }
    }
}
