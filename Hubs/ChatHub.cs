using Empath_AI.DTO;
using Empath_AI.DTO.Conversation;
using Empath_AI.Model;
using Empath_AI.Repository;
using Empath_AI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Empath_AI.Hubs
{
    //[Authorize]
    public class ChatHub : Hub
    {
        private readonly IGeminiService _gemini;
        private readonly IMessageRepository _messageRepository;
        private readonly IHeartRateRepository _heart;
        private static readonly Dictionary<string, string> _connections = new();
        private int _connectionsCount = 0;

        public ChatHub(IGeminiService gemini, IMessageRepository messageRepository, IHeartRateRepository heartRateRepository, IMessageRepository messageService)
        {
            _gemini = gemini;
            _messageRepository = messageRepository;
            _heart = heartRateRepository;
        }


        /* public override Task OnConnectedAsync()
         {
             var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
             if (userId != null)
             {
                 _connections[userId] = Context.ConnectionId;
                 _connectionsCount++;
                 Console.WriteLine($"User connected: {userId}");
             }
             return base.OnConnectedAsync();
         }

         public override Task OnDisconnectedAsync(Exception? exception)
         {
             var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
             if (userId != null)
             {
                 _connections.Remove(userId);
                 _connectionsCount--;
                 Console.WriteLine($"User disconnected: {userId}");
             }
             return base.OnDisconnectedAsync(exception);
         }
 */

        public async Task SendMessage(MessageDTO messageDTO, string content)
        {
            /* var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

             if (string.IsNullOrEmpty(userId))
             {
                 Console.WriteLine("⚠ User ID not found in token. Connection unauthorized.");
                 await Clients.Caller.SendAsync("ReceiveError", "Unauthorized: Invalid or missing token.");
                 return;
             }

             messageDTO.UserId = int.Parse(userId);*/

            // 1️⃣ Save the user’s message first
            var userMessage = await _messageRepository.SaveUserMessageAsync(messageDTO, content);

            // 2️⃣ Prepare the AI system prompt
            var systemPrompt = @"
             You are EmpathAI — an emotional wellness companion.
             You must always:
             - be empathetic and supportive
             - detect emotional tone
             - suggest grounding / safe mental self-help strategies
             - never give medical, legal, or harmful advice
             - if user is in crisis → encourage contacting real professionals
             ";

            // 3️⃣ Call Gemini to generate AI reply
            var (success, reply, error) = await _gemini.GenerateTextAsync(systemPrompt, content);
            string final_reply;
            string wrong;
            if (success)
            {
                final_reply = reply;

            }
            else
            {
                wrong = $"[Gemini Error] {error}";
                final_reply = "Please try again later";
                await Clients.Caller.SendAsync(final_reply);
            }

            var botMessage = await _messageRepository.SaveBotMessageAsync(messageDTO, final_reply);
            // 4️⃣ Save bot message

            // 5️⃣ Send result to the caller or all connected clients
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                user = userMessage,
                bot = botMessage
            });

        }
        public async Task SendAudioBase64(MessageDTO messageDTO, string base64Audio, string mimeType)
        {
            Console.WriteLine("✅ Hub entered SendAudioBase64");
            try
            {
                Console.WriteLine($"🎧 Received base64 audio, length = {base64Audio?.Length ?? 0}");

                if (string.IsNullOrEmpty(base64Audio))
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", new { reply = "❌ Empty audio data" });
                    return;
                }

                byte[] audioBytes = Convert.FromBase64String(base64Audio);

                var prompt = @"
                 You will receive an audio file.
                 1) Transcribe speech
                 2) Detect emotion
                 
                 Return ONLY valid JSON. NO markdown. NO backticks.
                 
                 Format:
                 {""transcript"":""..."",""emotion"":""sad|happy|neutral|angry|anxious|stressed|excited""}
                 ";

                var (success, jsonResult, raw, error) = await _gemini.AnalyzeAudioAsync(audioBytes, mimeType, prompt);

                if (!success)
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", new { reply = $"Audio error: {error}" });
                    return;
                }

                var transcript = jsonResult?.GetProperty("transcript").GetString() ?? "";
                var emotion = jsonResult?.GetProperty("emotion").GetString() ?? "neutral";

                await Clients.Caller.SendAsync("ReceiveMessage", new { reply = $"🗣️ {transcript}" });
                await Clients.Caller.SendAsync("ReceiveMessage", new { reply = $"🎭 Emotion: {emotion}" });

                await _messageRepository.SaveUserMessageAsync(messageDTO, transcript);
                await _messageRepository.SaveBotMessageAsync(messageDTO, $"Emotion detected: {emotion}");

                Console.WriteLine("✅ Audio processed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 ERROR inside SendAudioBase64: " + ex);
                await Clients.Caller.SendAsync("ReceiveMessage", new { reply = "Server error: " + ex.Message });
            }
        }

    }
}






// the edited code with errors
/*using Empath_AI.DTO;
using Empath_AI.DTO.Conversation;
using Empath_AI.Model;
using Empath_AI.Repository;
using Empath_AI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Empath_AI.Hubs
{
    //[Authorize]
    public class ChatHub : Hub
    {
        private readonly IGeminiService _gemini;
        private readonly IMessageRepository _messageRepository;
        private readonly IHeartRateRepository _heart;
        private static readonly Dictionary<string, string> _connections = new();
        private readonly IConversationRepository _conversationRepository;
        private int _connectionsCount = 0;

        public ChatHub(IGeminiService gemini, IMessageRepository messageRepository, IHeartRateRepository heartRateRepository, IMessageRepository messageService, IConversationRepository conversationRepository)
        {
            _gemini = gemini;
            _messageRepository = messageRepository;
            _heart = heartRateRepository;
            _conversationRepository = conversationRepository;
        }


        */
/* public override Task OnConnectedAsync()
         {
             var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
             if (userId != null)
             {
                 _connections[userId] = Context.ConnectionId;
                 _connectionsCount++;
                 Console.WriteLine($"User connected: {userId}");
             }
             return base.OnConnectedAsync();
         }

         public override Task OnDisconnectedAsync(Exception? exception)
         {
             var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
             if (userId != null)
             {
                 _connections.Remove(userId);
                 _connectionsCount--;
                 Console.WriteLine($"User disconnected: {userId}");
             }
             return base.OnDisconnectedAsync(exception);
         }
 */

        /* public async Task StartConversation(int userId)
         {
             try
             {
                 var conversationId = await _conversationRepository.CreateConversation(userId);

                 await Clients.Caller.SendAsync("ConversationCreated", new
                 {
                     conversationId,
                     userId
                 });
             }
             catch (Exception ex)
             {
                 await Clients.Caller.SendAsync("ReceiveError", $"Failed to create conversation: {ex.Message}");
             }
         }*/
        /*

        // In ChatHub.cs — replace SendMessage with this:
        */
/*public async Task SendMessage(int userId, int conversationId, string content)
        {
            Console.WriteLine($">>> userId={userId}, conversationId={conversationId}, content={content}");

            var messageDTO = new MessageDTO
            {
                UserId = userId,
                Conversation_ID = conversationId
            };

            var userMessage = await _messageRepository.SaveUserMessageAsync(messageDTO, content);

            var systemPrompt = @"You are EmpathAI — an emotional wellness companion.
         You must always be empathetic and supportive, detect emotional tone,
         suggest grounding strategies, never give medical/legal/harmful advice,
         and if user is in crisis → encourage contacting real professionals.";

            var (success, reply, error) = await _gemini.GenerateTextAsync(systemPrompt, content);
            string final_reply = success ? reply : "Please try again later";

            var botMessage = await _messageRepository.SaveBotMessageAsync(messageDTO, final_reply);

            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                user = userMessage,
                bot = botMessage
            });
        }*/
/*

        public async Task SendMessage(MessageDTO messageDTO, string content)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("⚠ User ID not found in token. Connection unauthorized.");
                await Clients.Caller.SendAsync("ReceiveError", "Unauthorized: Invalid or missing token.");
                return;
            }

            messageDTO.UserId = int.Parse(userId);


            // 1️⃣ Save the user’s message first
            var userMessage = await _messageRepository.SaveUserMessageAsync(messageDTO, content);

            // 2️⃣ Prepare the AI system prompt
            var systemPrompt = @"
              You are EmpathAI — an emotional wellness companion.
              You must always:
              - be empathetic and supportive
              - detect emotional tone
              - suggest grounding / safe mental self-help strategies
              - never give medical, legal, or harmful advice
              - if user is in crisis → encourage contacting real professionals
              ";

            // 3️⃣ Call Gemini to generate AI reply
            var (success, reply, error) = await _gemini.GenerateTextAsync(systemPrompt, content);
            string final_reply;
            string wrong;
            if (success)
            {
                final_reply = reply;

            }
            else
            {
                */
/* wrong = $"[Gemini Error] {error}";
                 final_reply = "Please try again later";
                 await Clients.Caller.SendAsync(final_reply);*/
/*
                await Clients.Caller.SendAsync("receiveerror", $"[Gemini Error] {error}");
                return;
            }

            var botMessage = await _messageRepository.SaveBotMessageAsync(messageDTO, final_reply);
            // 4️⃣ Save bot message

            // 5️⃣ Send result to the caller or all connected clients
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                user = userMessage,
                bot = botMessage
            });
        }
        public async Task SendAudioBase64(MessageDTO messageDTO, string base64Audio, string mimeType)
        {
            Console.WriteLine("✅ Hub entered SendAudioBase64");
            try
            {
                Console.WriteLine($"🎧 Received base64 audio, length = {base64Audio?.Length ?? 0}");

                if (string.IsNullOrEmpty(base64Audio))
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", new { reply = "❌ Empty audio data" });
                    return;
                }

                byte[] audioBytes = Convert.FromBase64String(base64Audio);

                var prompt = @"
                 You will receive an audio file.
                 1) Transcribe speech
                 2) Detect emotion
                 
                 Return ONLY valid JSON. NO markdown. NO backticks.
                 
                 Format:
                 {""transcript"":""..."",""emotion"":""sad|happy|neutral|angry|anxious|stressed|excited""}
                 ";

                var (success, jsonResult, raw, error) = await _gemini.AnalyzeAudioAsync(audioBytes, mimeType, prompt);

                if (!success)
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", new { reply = $"Audio error: {error}" });
                    return;
                }

                var transcript = jsonResult?.GetProperty("transcript").GetString() ?? "";
                var emotion = jsonResult?.GetProperty("emotion").GetString() ?? "neutral";

                await Clients.Caller.SendAsync("ReceiveMessage", new { reply = $"🗣️ {transcript}" });
                await Clients.Caller.SendAsync("ReceiveMessage", new { reply = $"🎭 Emotion: {emotion}" });

                await _messageRepository.SaveUserMessageAsync(messageDTO, transcript);
                await _messageRepository.SaveBotMessageAsync(messageDTO, $"Emotion detected: {emotion}");

                Console.WriteLine("✅ Audio processed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 ERROR inside SendAudioBase64: " + ex);
                await Clients.Caller.SendAsync("ReceiveMessage", new { reply = "Server error: " + ex.Message });
            }
        }

    }
}
*/