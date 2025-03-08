using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Wavlo.Data;
using Wavlo.Models;

namespace Wavlo
{

   [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _context;

        public ChatHub(ChatDbContext context)
        {
           _context = context;
        }
        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name ?? "Unknown User";
            await Clients.Caller.SendAsync("ReceiveMessage", username, "Welcome to the chat!");
            await base.OnConnectedAsync();
        }

        public /*async Task*/ void sendMessage(string name , string mess)
        {
            //DbSave
            Chat chat = new Chat()
            {
                Name = name,
                Message = mess,
               // Timestamp = DateTime.Now,
            };
            _context.Chats.Add(chat);
            _context.SaveChanges();



            Clients.All.SendAsync("newMessage",name , mess);



            //var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            //{
            //    throw new HubException("Unauthorized: Invalid User ID.");
            //}

            //var chat = new Chat { UserId = userId, Message = message };

            //try
            //{
            //    _context.Chats.Add(chat);
            //    await _context.SaveChangesAsync();
            //    await Clients.All.SendAsync("ReceiveMessage", userId.ToString(), message);
            //}
            //catch (Exception ex)
            //{
            //    throw new HubException("Error saving message: " + ex.Message);
            //}
        }
        public void joinGroup(string gname , string name)
        {
            Groups.AddToGroupAsync(Context.ConnectionId, gname);
            Clients.OthersInGroup(gname).SendAsync("newMember", name, gname);
        }

        public void sendToGroup(string gname , string name , string message)
        {
            Clients.Group(gname).SendAsync("groupMessage",name , gname , message);
        }
    }
}
