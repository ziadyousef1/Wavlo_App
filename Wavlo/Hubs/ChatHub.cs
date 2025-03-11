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

        public async Task sendMessage(int roomId, string message, string? attachmentUrl = null)
        {
            //if (!int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            //{
            //    throw new HubException("Unauthorized: Invalid User ID.");
            //}

            //var chatMessage = new Chat
            //{
            //    RoomId = roomId,
            //    UserId = userId,
            //    Content = message,
            //    AttachmentUrl = attachmentUrl,
            //    SentAt = DateTime.UtcNow
            //};

            //_context.Chats.Add(chatMessage);
            //await _context.SaveChangesAsync();

            //await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", new
            //{
            //    Text = chatMessage.Content,
            //    AttachmentUrl = chatMessage.AttachmentUrl,
            //    UserId = chatMessage.UserId,
            //    SentAt = chatMessage.SentAt.ToString("dd/MM/yyyy hh:mm:ss")
            //});
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
