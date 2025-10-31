using Empath_AI.Data;
using Empath_AI.DTO.User;
using Empath_AI.Model;
using Microsoft.EntityFrameworkCore;

namespace Empath_AI.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;

        public MessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Message> SaveMessageAsync(Message message)
        {
            // Basic guard / defaults (customize as needed)
            if (string.IsNullOrWhiteSpace(message.Message_Type))
                message.Message_Type = "text";

            if (string.IsNullOrWhiteSpace(message.Sender_Type))
                message.Sender_Type = "User";

            if (message.Created_At == default)
                message.Created_At = DateTime.UtcNow;

            // Save via repository
            var saved = await AddMessageAsync(message);
            return saved;
        }


        public async Task<Message> AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetMessagesByConversationAsync(int conversationId)
        {
            return await _context.Messages
                .Where(m => m.Conversation_ID == conversationId)
                .OrderBy(m => m.Created_At)
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessagesByUserAsync(int userId)
        {
            return await _context.Messages
                .Where(m => m.User_ID == userId)
                .OrderBy(m => m.Created_At)
                .ToListAsync();
        }
    
    }
}
