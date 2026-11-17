
namespace Proyecto_FarmaControl.DTOs
{
    public class AuthRespuestaDto
    {
        public string Token { get; set; } = string.Empty;

        public DateTime Expira { get; set; }

        public int UsuarioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;
    }
}
