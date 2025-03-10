using Wavlo.Models;

namespace Wavlo.Repository
{
    public interface IChatRepository
    {
        Task<Chat> GetChatAsync(int id);
        Task<int> CreateRoomAsync(string name, int userId);  
        Task<bool> JoinRoomAsync(int chatId, int userId); 
        Task<IEnumerable<Chat>> GetChatsAsync(int userId);
        Task<int> CreatePrivateRoomAsync(int rootId, int targetId);
        Task<IEnumerable<Chat>> GetPrivateChatsAsync(int userId);

        Task<Message> CreateMessageAsync(int chatId, string message, int userId, string? attachmentUrl = null);

        Task<bool> EditMessageAsync(int messageId, string newContent, string? newAttachmentUrl = null);
        Task<bool> DeleteMessageAsync(int messageId);

    }
}
