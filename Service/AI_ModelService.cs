using System.Text;
using System.Text.Json;

namespace Empath_AI.Services
{
    public class AI_ModelService
    {
        private readonly HttpClient _httpClient;
        private readonly string _flaskUrl = "https://empathai-production.up.railway.app/predict";

        // Buffer 512 GSR readings per user
        private static readonly Dictionary<int, List<double>> _gsrBuffer = new();

        public AI_ModelService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ── Add GSR reading to buffer ─────────────────────────
        public void AddGSRReading(int userId, double rawGSRValue)
        {
            if (!_gsrBuffer.ContainsKey(userId))
                _gsrBuffer[userId] = new List<double>();

            _gsrBuffer[userId].Add(rawGSRValue);

            // Keep only last 512 readings
            if (_gsrBuffer[userId].Count > 512)
                _gsrBuffer[userId].RemoveAt(0);
        }

        // ── Get emotion from Flask API ────────────────────────
        public async Task<string> GetEmotionAsync(int? userId)
        {
            // ✅ Return unknown if userId is null (bot messages)
            if (!userId.HasValue)
                return "unknown";

            try
            {
                // Get GSR buffer for user
                var gsrReadings = _gsrBuffer.ContainsKey(userId.Value)
                    ? _gsrBuffer[userId.Value].ToList()
                    : new List<double>();

                // Pad with zeros if less than 512 readings
                while (gsrReadings.Count < 512)
                    gsrReadings.Add(0);

                // Take last 512
                gsrReadings = gsrReadings.TakeLast(512).ToList();

                // Build payload — zeros for missing sensors
                var payload = new
                {
                    gsr = gsrReadings,
                    ppg = Enumerable.Repeat(0.0, 512).ToList(),
                    emg1 = Enumerable.Repeat(0.0, 512).ToList(),
                    emg2 = Enumerable.Repeat(0.0, 512).ToList(),
                    temp = Enumerable.Repeat(0.0, 512).ToList()
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"[EmotionService] Sending {gsrReadings.Count} GSR readings to Flask...");

                var response = await _httpClient.PostAsync(_flaskUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[EmotionService] Flask response: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[EmotionService] Flask error: {response.StatusCode}");
                    return "unknown";
                }

                var result = JsonSerializer.Deserialize<EmotionResponse>(responseBody);
                return result?.emotion ?? "unknown";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmotionService] Exception: {ex.Message}");
                return "unknown";
            }
        }

        private class EmotionResponse
        {
            public string emotion { get; set; }
        }
    }
}