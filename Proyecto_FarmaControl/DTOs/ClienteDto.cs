
namespace Proyecto_FarmaControl.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? DocumentoIdentidad { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public string? Direccion { get; set; }

        public bool Activo { get; set; }
    }
}
