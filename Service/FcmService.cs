using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Empath_AI.Services
{
    public class FcmService
    {
        public FcmService()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "empath-ai-29ea1-firebase-adminsdk-fbsvc-57baf752be.json")
                    )
                });
            }
        }

        public async Task<bool> SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new Message
                {
                    Token = fcmToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>(),
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification
                        {
                            Sound = "default",
                            ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = "default"
                        }
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"✅ FCM sent: {response}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 FCM error: {ex.Message}");
                return false;
            }
        }
    }
}