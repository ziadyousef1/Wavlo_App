using Microsoft.AspNet.SignalR.Messaging;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wavlo.Models
{
    public class Chat
    {
        public Chat()
        {
            Messages = new List<Message>();
            Users = new List<ChatUser>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        //public int UserId { get; set; }
        //[ForeignKey("UserId")]
        //public User User { get; set; }
        public ChatType Type { get; set; }
        public ICollection<Message> Messages { get; set; }
        public ICollection<ChatUser> Users { get; set; }
    }
}
