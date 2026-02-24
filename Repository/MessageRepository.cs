using Empath_AI.Data;
using Empath_AI.DTO.Conversation;
using Empath_AI.DTO.User;
using Empath_AI.Model;
using Empath_AI.Service;
using Empath_AI.Services;
using Microsoft.EntityFrameworkCore;

namespace Empath_AI.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;
        private readonly IGeminiService _geminiService;

        public MessageRepository(AppDbContext context, IGeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        public async Task<Message> SaveUserMessageAsync(MessageDTO message, string content)
        {
            // 🔍 Debug — remove after confirming it works
            Console.WriteLine($"DEBUG SaveUserMessage → UserId={message.UserId}, Conversation_ID={message.Conversation_ID}, Content={content}");

            if (message.Conversation_ID == 0)
                throw new Exception("Conversation_ID is 0 — DTO deserialization failed. Check SignalR JSON naming policy.");

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var userMessage = new Message
            {
                User_ID = message.UserId,
                Sender_Type = "User",
                Content = content,
                Message_Type = "text",
                Conversation_ID = message.Conversation_ID,
                Created_At = egyptTime
            };

            await _context.Messages.AddAsync(userMessage);
            await _context.SaveChangesAsync();

            // Reload from DB so navigation properties and generated ID are populated
            await _context.Entry(userMessage).ReloadAsync();

            return userMessage;
        }
        public async Task<Message> SaveBotMessageAsync(MessageDTO message, string content)
        {

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var botMessage = new Message
            {
                Bot_ID = 10,
                Sender_Type = "Bot",
                Content = content,
                Message_Type = "text",
                Conversation_ID = message.Conversation_ID,
                Created_At = egyptTime
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
