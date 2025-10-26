using Empath_AI.DTO.Conversation;
using Empath_AI.Migrations;
using Empath_AI.Model;
using Microsoft.VisualBasic;

namespace Empath_AI.Repository
{
    public interface IConversationRepository
    {
        Task<IEnumerable<Conversation>> GetAll();
        Task<Conversation> GetConversationById(int Id);
        Task CreateConversation(ConversationDTO conversationDTO,User user);
        Task<bool> UpdateTitle(int Id,string NewTitle);
        Task<bool> UpdateLastActivity(int Id);
        Task DeleteConversation(Conversion conversion);










    }
}
