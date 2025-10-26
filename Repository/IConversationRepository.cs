using Empath_AI.DTO;
using Empath_AI.Migrations;
using Empath_AI.Model;

namespace Empath_AI.Repository
{
    public interface IConversationRepository
    {
        Task<IEnumerable<Conversation>> GetAll();
        Task<Conversation> GetConversationById(int Id);
        Task<Conversation> CreateConversation(ConversationDto conversationDto);
        Task<bool> UpdateTitle(int Id,string NewTitle);
        Task<bool> UpdateLastActivity(int Id);
        Task <bool>DeleteConversation(int Id);










    }
}
