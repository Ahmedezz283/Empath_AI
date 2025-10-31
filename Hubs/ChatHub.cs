using Empath_AI.Model;
using Empath_AI.Repository;
using Empath_AI.Service;
using Microsoft.AspNetCore.SignalR;

namespace Empath_AI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly Bot _bot;
        private readonly IHeartRateRepository _heart;
        private readonly IMessageRepository _messageService;

        public ChatHub(Bot bot, IHeartRateRepository heartRateRepository, IMessageRepository messageService)
        {
            _bot = bot;
            _heart = heartRateRepository;
            _messageService = messageService;
        }

        public async Task SendMessage(int userId, string content)
        {
            // 1️⃣ Save user message in DB
            var userMessage = await _messageService.SaveMessageAsync(new Message
            {
                User_ID = userId,
                Sender_Type = "User",
                Content = content,
                Message_Type = "text",
                Created_At = DateTime.UtcNow
            });

            // 2️⃣ Broadcast user message to all clients (frontend updates instantly)
            await Clients.All.SendAsync("ReceiveMessage", userMessage);

            // 3️⃣ Get bot reply
            var botReply = await _bot.GetChatbotResponseWithHeartRate(content , userId);

            // 4️⃣ Save bot reply
            var botMessage = await _messageService.SaveMessageAsync(new Message
            {
                Bot_ID = 2005,
                Sender_Type = "Bot",
                Content = botReply,
                Message_Type = "text",
                Created_At = DateTime.UtcNow
            });

            // 5️⃣ Send bot message via SignalR
            await Clients.All.SendAsync("ReceiveMessage", botMessage);
        }

    }
}
