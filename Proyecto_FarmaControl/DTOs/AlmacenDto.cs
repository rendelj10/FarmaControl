
namespace Proyecto_FarmaControl.DTOs
{
    public class AlmacenDto
    {
        public int Id { get; set; }

        public int SucursalId { get; set; }

        public string SucursalNombre { get; set; } = string.Empty;

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Ubicacion { get; set; }

        public bool Activo { get; set; }
    }
}
