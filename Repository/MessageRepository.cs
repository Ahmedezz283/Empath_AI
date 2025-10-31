using Empath_AI.Data;
using Empath_AI.DTO.Conversation;
using Empath_AI.DTO.User;
using Empath_AI.Model;
using Empath_AI.Service;
using Microsoft.EntityFrameworkCore;

namespace Empath_AI.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;
        private readonly Bot _bot;

        public MessageRepository(AppDbContext context, Bot bot)
        {
            _context = context;
            _bot = bot;
        }

        public async Task<Message> SaveUserMessageAsync(MessageDTO message , string content)
        {
            var userMessage = new Message
            {
                User_ID = message.UserId,
                Sender_Type = "User",
                Content = content,
                Message_Type = "text",
                Conversation_ID = message.Conversation_ID,
                Created_At = DateTime.UtcNow
            };

            await _context.Messages.AddAsync(userMessage);
            await _context.SaveChangesAsync();
            return userMessage;
        }
        public async Task<Message> SaveBotMessageAsync(MessageDTO message, string content)
        {
            var botMessage = new Message
            {
                Bot_ID = message.bot_id,
                Sender_Type = "Bot",
                Content = content,
                Message_Type = "text",
                Conversation_ID = message.Conversation_ID,
                Created_At = DateTime.UtcNow
            };

            await _context.Messages.AddAsync(botMessage);
            await _context.SaveChangesAsync();
            return botMessage;
        }
       /* public async Task<Message> AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }*/
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
