namespace Wavlo.Models
{
    public class StoryView
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StoryId { get; set; }
        public Story Story { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
