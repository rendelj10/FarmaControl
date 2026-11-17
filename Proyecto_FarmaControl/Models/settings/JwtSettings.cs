
namespace Proyecto_FarmaControl.Models.settings
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int TokenValidateInMinutos { get; set; } = 10;
    }
}
