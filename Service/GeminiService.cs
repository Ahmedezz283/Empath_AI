using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Empath_AI.Services
{
    public class GeminiOptions
    {
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "gemini-2.0-flash-lite";
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models";
    }

    public interface IGeminiService
    {
        Task<(bool Success, string? Reply, string? Error)> GenerateTextAsync(string systemPrompt, string userMessage);
        Task<(bool Success, JsonElement? JsonResult, string? RawResult, string? Error)> AnalyzeAudioAsync(byte[] audioBytes, string mimeType, string systemPrompt);
    }

    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _http;
        private readonly GeminiOptions _options;

        public GeminiService(HttpClient http, IOptions<GeminiOptions> opt)
        {
            _http = http;
            _options = opt.Value;
        }

        private string BuildUrlForModel()
        {
            // e.g. https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key=API_KEY
            var model = _options.Model;
            var baseUrl = _options.BaseUrl?.TrimEnd('/') ?? "https://generativelanguage.googleapis.com/v1beta/models";
            return $"{baseUrl}/{model}:generateContent?key={_options.ApiKey}";
        }

        public async Task<(bool Success, string? Reply, string? Error)> GenerateTextAsync(string systemPrompt, string userMessage)
        {
            try
            {
                var url = BuildUrlForModel();

                // Build request body similar to Python client usage: "contents" array with role/parts
                var payload = new
                {
                    contents = new[]
                    {
                        new {
                            role = "user",
                            parts = new[] { new { text = $"{systemPrompt}\nUser: {userMessage}" } }
                        }
                    }
                };


                var json = JsonSerializer.Serialize(payload);
                using var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                var resp = await _http.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    return (false, null, $"Status: {(int)resp.StatusCode} - {body}");
                }

                using var doc = JsonDocument.Parse(body);
                // The exact response structure may vary; typically there's a 'candidates' or 'outputs' or 'outputs[0].content' etc.
                // We'll try common fields: 'candidates', or 'outputs' -> take the first text candidate.
                string? text = null;

                // Try naive extractions (adapt if your REST returns different structure)
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var first = candidates[0];
                    if (first.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                        text = content.GetString();
                }
                // fallback: outputs[].content or text
                if (text == null && doc.RootElement.TryGetProperty("outputs", out var outputs) && outputs.GetArrayLength() > 0)
                {
                    var fst = outputs[0];
                    if (fst.TryGetProperty("content", out var cont) && cont.ValueKind == JsonValueKind.String)
                        text = cont.GetString();
                }

                // Some variants put text under "response" or "result"
                if (text == null)
                {
                    // As last resort, try to extract any "text" string anywhere (not ideal)
                    var allText = ExtractFirstStringValue(doc.RootElement);
                    text = allText;
                }

                if (text == null)
                {
                    return (false, null, "Could not parse model response: " + body);
                }

                return (true, text, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        private string? ExtractFirstStringValue(JsonElement el)
        {
            // DFS for first string leaf (best-effort)
            if (el.ValueKind == JsonValueKind.String) return el.GetString();

            if (el.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in el.EnumerateObject())
                {
                    var found = ExtractFirstStringValue(prop.Value);
                    if (found != null) return found;
                }
            }
            else if (el.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in el.EnumerateArray())
                {
                    var found = ExtractFirstStringValue(item);
                    if (found != null) return found;
                }
            }
            return null;
        }

        public async Task<(bool Success, JsonElement? JsonResult, string? RawResult, string? Error)> AnalyzeAudioAsync(byte[] audioBytes, string mimeType, string systemPrompt)
        {
            try
            {
                var url = BuildUrlForModel();

                var audioB64 = Convert.ToBase64String(audioBytes);

                // Build contents with inline_data similar to your Python code
                var payload = new
                {
                    contents = new[]
                    {
                        new {
                            role = "user",
                            parts = new object[]
                            {
                                new { text = systemPrompt },
                                new {
                                    inline_data = new {
                                        mime_type = mimeType,
                                        data = audioB64
                                    }
                                }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                using var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                var resp = await _http.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return (false, null, body, $"Status {(int)resp.StatusCode}");

                // The model should return text (sometimes with code fences). We need to strip triple backticks and extract the JSON object.
                // Try to extract from response root first: common fields might be outputs[0].content or candidates[0].content
                string modelText = null;
                using (var doc = JsonDocument.Parse(body))
                {
                    modelText = ExtractFirstStringValue(doc.RootElement);
                }

                if (modelText == null)
                {
                    // fallback: raw body
                    modelText = body;
                }

                // Remove code fences
                modelText = Regex.Replace(modelText, @"```.*?```", "", RegexOptions.Singleline);

                // Extract JSON object: find the first {...} substring
                var match = Regex.Match(modelText, @"\{[\s\S]*\}");
                if (!match.Success)
                {
                    // If we can't find JSON, return raw for debugging
                    return (false, null, modelText, "Invalid JSON returned from model");
                }

                var jsonObjString = match.Value;

                var parsed = JsonDocument.Parse(jsonObjString);
                return (true, parsed.RootElement, modelText, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, ex.Message);
            }
        }
    }
}
