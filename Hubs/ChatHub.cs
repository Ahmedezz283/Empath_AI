using Empath_AI.DTO.Conversation;
using Empath_AI.Model;
using Empath_AI.Repository;
using Empath_AI.Service;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

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

        public async Task SendMessage(MessageDTO messageDTO, string content)
        {
            try
            {
                messageDTO.Content = content;

                var userMessage = await _messageService.SaveUserMessageAsync(messageDTO, content);
                await Clients.All.SendAsync("ReceiveMessage", userMessage);

                var botReply = await _bot.GetChatbotResponseWithHeartRate(messageDTO);
                var botMessage = await _messageService.SaveBotMessageAsync(messageDTO, botReply);
                await Clients.All.SendAsync("ReceiveMessage", botMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in SendMessage: {ex}");
                throw;
            }
        }


    }
}
