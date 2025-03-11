using Wavlo.Models;

namespace Wavlo.Repository
{
    public interface IChatRepository
    {
        Task<Chat> GetChatAsync(int id);
        Task<bool> JoinRoomAsync(int chatId, string userId);
        Task<int> CreateRoomAsync(string name, string userId);
        Task<IEnumerable<Chat>> GetChatsAsync(string userId);
        Task<int> CreatePrivateRoomAsync(string rootId, string targetId);
        Task<IEnumerable<Chat>> GetPrivateChatsAsync(string userId);

        Task<Message> CreateMessageAsync(int chatId, string message, string userId, string? attachmentUrl = null);

        Task<bool> EditMessageAsync(int messageId, string newContent, string? newAttachmentUrl = null);
        Task<bool> DeleteMessageAsync(int messageId);

    }
}
