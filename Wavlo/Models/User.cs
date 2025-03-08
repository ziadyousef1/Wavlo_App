namespace Wavlo.Models
{
    public class User
    {
        public User() : base()
        {
            Chats = new List<ChatUser>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? VerificationCode { get; set; }
        public DateTime? ExpirationCode { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<ChatUser> Chats { get; set; }
    }
}
