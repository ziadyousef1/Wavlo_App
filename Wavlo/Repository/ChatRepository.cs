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
        public async Task<Message> CreateMessageAsync(int chatId, string message, int userId, string? attachmentUrl = null)
        {
            var newMessage = new Message
            {
                ChatId = chatId,
                Content = message,
                Id = userId,
                SentAt = DateTime.Now,
                AttachmentUrl = attachmentUrl
            };

            _context.Add(newMessage);
            await _context.SaveChangesAsync();

            return newMessage;

        }

        public async Task<int> CreatePrivateRoomAsync(int rootId, int targetId)
        {
            var chat = new Chat
            {
                Type = ChatType.Private
            };

            chat.Users.Add(new ChatUser
            {
                UserId = targetId
            });

            chat.Users.Add(new ChatUser
            {
                UserId = rootId
            });

            _context.Chats.Add(chat);

            await _context.SaveChangesAsync();

            return chat.Id;

        }

        public async Task<int> CreateRoomAsync(string name, int userId)
        {
            var chat = new Chat
            {
                Type = ChatType.Group,
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
            return await _context.Chats
                 .Include(x => x.Messages)
                 .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Chat>> GetChatsAsync(int userId)
        {
            return await _context.Chats
                .Include(x => x.Users)
                .Where(x => !x.Users
                    .Any(y => y.UserId == userId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Chat>> GetPrivateChatsAsync(int userId)
        {
            return _context.Chats
                   .Include(x => x.Users)
                       .ThenInclude(x => x.User)
                   .Where(x => x.Type == ChatType.Private
                       && x.Users
                           .Any(y => y.UserId == userId))
                   .ToList();
        }

        public async Task<bool> JoinRoomAsync(int chatId, int userId)
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
