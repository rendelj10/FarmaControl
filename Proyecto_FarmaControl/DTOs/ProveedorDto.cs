
namespace Proyecto_FarmaControl.DTOs
{
    public class ProveedorDto
    {
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string RazonSocial { get; set; } = string.Empty;

        public string? NombreComercial { get; set; }

        public string? Ruc { get; set; }

        public string? Contacto { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public string? Direccion { get; set; }

        public bool Activo { get; set; }
    }
}
