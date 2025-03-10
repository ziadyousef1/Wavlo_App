using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Wavlo.Models
{
    public class UserImage
    {
        public int Id { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
