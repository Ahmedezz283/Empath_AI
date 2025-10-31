using Empath_AI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace Empath_AI.Service
{
    public class Bot
    {
        private readonly IHubContext<Hubs.ChatHub> _hubContext;
        private readonly IHeartRateRepository _heart;


        public Bot(IHubContext<Hubs.ChatHub> hubContext, IHeartRateRepository heart)
        {
            _hubContext = hubContext;
            _heart = heart;
        }

        public async Task<string> GetChatbotResponseWithHeartRate(string userMessage, int userid)
        {
            double? heartRate = await _heart.GetLatestHeartRate(userid);

            if (heartRate == null)
                return $"You said: {userMessage}. I couldn’t find your latest heart rate 😕.";

            string feedback;

            if (heartRate < 60)
                feedback = "Your heart rate is a bit low 💙. Maybe you’re relaxed or resting?";
            else if (heartRate < 100)
                feedback = "Your heart rate looks normal ❤️. Keep it up!";
            else
                feedback = "Your heart rate is a bit high ❤️‍🔥. Maybe you’re stressed or active.";

            return $"You said: '{userMessage}'. Your latest heart rate is {heartRate} bpm. {feedback}";
        }
    }
}
