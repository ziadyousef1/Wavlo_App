using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Wavlo.Data;
using Wavlo.Models;

namespace Wavlo.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ChatDbContext _context;
        public ChatRepository(ChatDbContext context)
        {
            _context = context;
        }
        public async Task<Message> CreateMessageAsync(int chatId, string message, string userId, string? targetUserId = null, string? attachmentUrl = null)
        {
            var chatExists = await _context.Chats.AnyAsync(c => c.Id == chatId);
            if (!chatExists)
            {
                throw new Exception("Chat room not found");
            }

            var senderUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            string senderName = senderUser?.FirstName ?? "Unknown";
            var newMessage = new Message
            {
                ChatId = chatId,
                Content = message,
                UserId = userId,
                Name = senderName,
                SentAt = DateTime.Now,
                AttachmentUrl = attachmentUrl
            };

            _context.Add(newMessage);
            await _context.SaveChangesAsync();

            return newMessage;

        }

        public async Task<int> CreatePrivateRoomAsync(string rootId, string targetId , string name)
        {
            var existingChat = await _context.Chats
         .Include(c => c.Users)
         .FirstOrDefaultAsync(c => c.Type == ChatType.Private &&
                                   c.Users.Any(u => u.UserId == rootId) &&
                                   c.Users.Any(u => u.UserId == targetId));

            if (existingChat != null)
            {
                return existingChat.Id;
            }

            var chat = new Chat
            {
                Type = ChatType.Private,
                Name = name,
                Users = new List<ChatUser>
        {
            new ChatUser { UserId = rootId },
            new ChatUser { UserId = targetId }
        }
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return chat.Id;

        }

        public async Task<int> CreateRoomAsync(string name, string userId , bool isGroup = false)
        {

            var chat = new Chat
            {
                Type = ChatType.Group,
                IsGroup = isGroup,
                Name = name,
            };
            chat.Users.Add(new ChatUser
            {
                UserId = userId,
                Role = UserRole.Admin,
            });

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return chat.Id;
        }

        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (messageId == null)
                return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true; 
        }

        public async Task<bool> EditMessageAsync(int messageId, string newContent, string? newAttachmentUrl = null)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (messageId == null)
                return false;

            message.Content = newContent;
            if (newAttachmentUrl != null)
            {
                message.AttachmentUrl = newAttachmentUrl;
            }
            message.SentAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Chat> GetChatAsync(int id)
        {
            {
                var chat = await _context.Chats
                    .Include(x => x.Messages)
                    .FirstOrDefaultAsync(x => x.Id == id);

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(chat, options);
                return JsonSerializer.Deserialize<Chat>(json, options);
            }
        }

        public async Task<IEnumerable<Chat>> GetChatsAsync(string userId)
        {
            return await _context.Chats
    .Include(x => x.Users)
    .Where(x => x.Users.Any(y => y.UserId == userId))
    .ToListAsync();

        }

        public async Task<IEnumerable<Chat>> GetPrivateChatsAsync(string userId)
        {
            return _context.Chats
                   .Include(x => x.Users)
                       .ThenInclude(x => x.User)
                   .Where(x => x.Type == ChatType.Private
                       && x.Users
                           .Any(y => y.UserId == userId))
                   .ToList();
        }

        public async Task<bool> JoinRoomAsync(int chatId, string userId)
        {
            bool alreadyJoined = await _context.ChatUsers
                                .AnyAsync(cu => cu.ChatId == chatId && cu.UserId == userId);

            if (alreadyJoined)
                return false;

            var chatUser = new ChatUser
            {
                ChatId = chatId,
                UserId = userId,
                Role = UserRole.Member,
            };

            _context.ChatUsers.Add(chatUser);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
