using Empath_AI.DTO.Conversation;
using Empath_AI.Migrations;
using Empath_AI.Model;
using FirebaseAdmin.Messaging;
using Microsoft.VisualBasic;

namespace Empath_AI.Repository
{
    public interface IConversationRepository
    {
        Task<IEnumerable<Conversation>> GetAll();
        Task<Conversation?> GetConversationById(int Id);
        Task<List<Conversation>> GetConversationByUserId(int UserId);
        Task CreateConversation(ConversationDTO conversationDTO,User user);
        Task<bool> UpdateTitle(int Id,string NewTitle);
        Task<bool> UpdateLastActivity(int Id);
        Task DeleteConversation(Conversation conversation);
        Task<IEnumerable<Conversation>> SearchConversationByTitle(int UserId, string KeyWord);
        Task<IEnumerable<Conversation>> GetRecentConversations(int UserId, int days = 7);

        //Task<List<Message>> GetConversationMessages(int conversationId)
         Task<List<ConversationSummaryDTO>> GetConversationSummeries(int UserID);
        Task<bool> ArchiveConversation(int conversationid);




    }
}
