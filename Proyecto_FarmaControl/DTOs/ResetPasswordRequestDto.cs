
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
