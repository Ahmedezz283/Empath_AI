using Empath_AI.Model;

namespace Empath_AI.Repository
{
    public interface IMessageRepository
    {
        Task<Message> AddMessageAsync(Message message);
        Task<List<Message>> GetMessagesByConversationAsync(int conversationId);
        Task<List<Message>> GetMessagesByUserAsync(int userId);
        Task<Message> SaveMessageAsync(Message message);
    }
}
