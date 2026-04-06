using Empath_AI.Data;
using Empath_AI.DTO;
using Empath_AI.DTO.Conversation;
using Empath_AI.Model;
using Empath_AI.Repository;
using Empath_AI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
        private readonly FcmService _fcmService;
        private readonly AppDbContext _context;
        private readonly AI_ModelService _emotionService;
        private static readonly Dictionary<string, string> _connections = new();
        private int _connectionsCount = 0;

        public ChatHub(IGeminiService gemini, IMessageRepository messageRepository, IHeartRateRepository heartRateRepository, IMessageRepository messageService, FcmService fcmService, AppDbContext context, AI_ModelService emotionService)
        {
            _gemini = gemini;
            _messageRepository = messageRepository;
            _heart = heartRateRepository;
            _fcmService = fcmService;
            _context = context;
            _emotionService = emotionService;
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

        /*public async Task SendMessage(MessageDTO messageDTO, string content)
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

            //firebase test
            var user = await _context.Users.FindAsync(messageDTO.UserId);
            if (user != null && !string.IsNullOrEmpty(user.FcmToken))
            {
                await _fcmService.SendNotificationAsync(
                    user.FcmToken,
                    "Empath AI 💬",
                    reply.Length > 100 ? reply.Substring(0, 100) + "..." : reply,
                    new Dictionary<string, string>
                    {
                                { "conversationId", messageDTO.Conversation_ID.ToString() },
                                { "type", "bot_reply" }
                    }
                );
            }

        }
*/


        public async Task SendMessage(MessageDTO messageDTO, string content)
        {
            try
            {
                Console.WriteLine(">>> SendMessage called");

                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($">>> userId: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("ReceiveError", "Unauthorized: Invalid or missing token.");
                    return;
                }

                messageDTO.UserId = int.Parse(userId);
                Console.WriteLine($">>> Saving user message...");

                var userMessage = await _messageRepository.SaveUserMessageAsync(messageDTO, content);
                Console.WriteLine($">>> User message saved");

                Console.WriteLine($">>> Getting emotion...");
                var emotion = await _emotionService.GetEmotionAsync(messageDTO.UserId);
                Console.WriteLine($">>> Emotion: {emotion}");

                var latestAccel = await _context.Accelerometer
                      .Where(a => a.UserId == messageDTO.UserId)
                      .OrderByDescending(a => a.Timestamp)
                      .FirstOrDefaultAsync();

                if (latestAccel != null)
                    Console.WriteLine($"[Accelerometer] State: {latestAccel.ActivityLevel} | Steps: {latestAccel.StepCount} | Fall: {latestAccel.FallDetected}");
                else
                    Console.WriteLine("[Accelerometer] No data found for this user");

                // ✅ Build sensor context string
                var sensorContext = "";

                if (latestAccel != null)
                {
                    sensorContext += $@"
                     - Behavioral State: {latestAccel.ActivityLevel}
                     - Step Count: {latestAccel.StepCount}
                     - Fall Detected: {(latestAccel.FallDetected ? "YES - user may have fallen" : "No")}";
                }

                // ✅ Build Gemini system prompt with emotion + accelerometer only
                var systemPrompt = $@"You are EmpathAI — an emotional wellness companion.

                Current biosensor data for this user:
                - Detected Emotion (AI model): {emotion.ToUpper()}
                {sensorContext}
                
                Based on this data, you must:
                - Acknowledge and respond to their {emotion} emotional state
                - Consider their physical activity and behavioral state in your response
                - Be empathetic and supportive
                - Suggest grounding / safe mental self-help strategies relevant to their current state when is asked or when needed
                - Never give pharmaceutical, legal, or harmful advice
                - If user is in crisis → encourage contacting real professionals
                - dont make answers too long or too short keep its length appropriate according to the context of the conversation";

                Console.WriteLine($">>> Calling Gemini...");
                var (success, reply, error) = await _gemini.GenerateTextAsync(systemPrompt, content);
                Console.WriteLine($">>> Gemini success: {success}");

                if (!success)
                {
                    await Clients.Caller.SendAsync("ReceiveError", $"[Gemini Error] {error}");
                    return;
                }

                Console.WriteLine($">>> Saving bot message...");
                var botMessage = await _messageRepository.SaveBotMessageAsync(messageDTO, reply);
                Console.WriteLine($">>> Bot message saved");

                await Clients.Caller.SendAsync("ReceiveMessage", new
                {
                    user = userMessage,
                    bot = botMessage,
                    emotion = emotion
                });
                Console.WriteLine($">>> ReceiveMessage sent");

                var user = await _context.Users.FindAsync(messageDTO.UserId);
                if (user != null && !string.IsNullOrEmpty(user.FcmToken))
                {
                    await _fcmService.SendNotificationAsync(
                        user.FcmToken,
                        "Empath AI 💬",
                        reply.Length > 100 ? reply.Substring(0, 100) + "..." : reply,
                        new Dictionary<string, string>
                        {
                    { "conversationId", messageDTO.Conversation_ID.ToString() },
                    { "type", "bot_reply" },
                    { "emotion", emotion }
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 SendMessage error: {ex}");
                await Clients.Caller.SendAsync("ReceiveError", $"Server error: {ex.Message}");
            }
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