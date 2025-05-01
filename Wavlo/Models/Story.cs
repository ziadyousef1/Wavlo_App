namespace Wavlo.Models
{
    public class Story
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public User User { get; set; }

        public string MediaUrl { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } 
    }
}
