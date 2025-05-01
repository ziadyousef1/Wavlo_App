using System.ComponentModel.DataAnnotations;

namespace Wavlo.DTOs
{
    public class CreateStoryDto
    {
        [Required]
        public IFormFile MediaFile { get; set; }
    }
}
