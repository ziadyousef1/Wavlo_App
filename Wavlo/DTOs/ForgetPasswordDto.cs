using System.ComponentModel.DataAnnotations;

namespace Wavlo.DTOs
{
    public class ForgetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
