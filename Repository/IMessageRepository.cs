using Empath_AI.DTO.Conversation;
using Empath_AI.Model;

namespace Empath_AI.Repository
{
    public interface IMessageRepository
    {
        //Task<Message> AddMessageAsync(Message message);
        Task<Message> SaveUserMessageAsync(MessageDTO message, string content);
        Task<Message> SaveBotMessageAsync(MessageDTO message, string content);
        Task<List<Message>> GetMessagesByConversationAsync(int conversationId);
        Task<List<Message>> GetMessagesByUserAsync(int userId);
    }
}
