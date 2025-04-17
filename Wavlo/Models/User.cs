using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Wavlo.Models
{
    public class User : IdentityUser
    {
        public User() : base()
        {
            Chats = new List<ChatUser>();
            UserImages = new List<UserImage>();
        }

         public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
      //  public string Email { get; set; }
        public string? VerificationCode { get; set; }
        public DateTime? ExpirationCode { get; set; }
       // public string PasswordHash { get; set; }
        public ICollection<ChatUser> Chats { get; set; }
        [ValidateNever]
        public List<UserImage> UserImages { get; set; }

        public List<RefreshToken>? RefreshTokens { get; set; } 

    }
}
