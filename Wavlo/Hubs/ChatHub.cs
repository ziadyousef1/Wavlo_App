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
        private static readonly Dictionary<string, string> UserConnections = new Dictionary<string, string>();

        public ChatHub(ChatDbContext context)
        {
           _context = context;
        }
        

        public override async Task OnConnectedAsync()
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == Context.UserIdentifier);
            if ( user is not null )
            {
                UserConnections[user.Id] = Context.ConnectionId;
            }
            

            var username = user?.UserName ?? "Unknown User";
             await Clients.Caller.SendAsync("ReceiveMessage", username, "Welcome to the chat!");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
              UserConnections.Remove(userId);
            

            var username = Context.User?.Identity?.Name ?? "Unknown User";
            await Clients.All.SendAsync("userDisconnected", username);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task sendMessage(string receiverId, string message, string? attachmentUrl = null)
        {
            
            
            if (!int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int senderId))
            {
                throw new HubException("Unauthorized: Invalid User ID.");
            }

            
            Message chatMessage = new Message
            {
                ChatId = 0,
                UserId = senderId.ToString(),
                Content = message,
                AttachmentUrl = attachmentUrl,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(chatMessage);
            _context.SaveChanges();

            
            if (UserConnections.TryGetValue(receiverId, out var receiverConnectionId))
            {
               await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderId, message, attachmentUrl);
            }
            else
            {
                throw new HubException("Receiver not connected.");
            }
        }

        //public async Task NotifyTyping(int roomId)
        //{
        //    var userName = Context.User?.Identity?.Name ?? "Unknown User";
        //    await Clients.Group(roomId.ToString()).SendAsync("userTyping", userName);
        //}
        public async Task joinGroup(string gname , string name)
        {
           await Groups.AddToGroupAsync(Context.ConnectionId, gname);
           await Clients.OthersInGroup(gname).SendAsync("newMember", name, gname);
        }
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("userLeft", Context.User?.Identity?.Name, groupName);
        }
        public void sendToGroup(string gname , string name , string message)
        {
            Clients.Group(gname).SendAsync("groupMessage",name , gname , message);
        }
    }
}
