
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; }

        public string? EmailConfirmToken { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public string? PasswordResedToken { get; set; }

        public DateTime? PasswordResetTokenExpirityTime
            { get; set; }

        public int RoleId { get; set; }

        public virtual Rol Role { get; set; } = null!;

        public ICollection<UsuarioPermiso> UsuarioPermisos
            { get; set; } = new List<UsuarioPermiso>();
    }
}
