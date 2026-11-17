
namespace Proyecto_FarmaControl.DTOs
{
    public class MedicamentoDto
    {
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? PrincipioActivo { get; set; }

        public string? Concentracion { get; set; }

        public string? FormaFarmaceutica { get; set; }

        public string? Presentacion { get; set; }

        public int CategoriaId { get; set; }

        public string CategoriaNombre { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }
}
