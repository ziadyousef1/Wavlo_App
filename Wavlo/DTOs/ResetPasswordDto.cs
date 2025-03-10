using System.ComponentModel.DataAnnotations;

namespace Wavlo.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Password does not match")]
        public string confirmPassword { get; set; }

    }
}
