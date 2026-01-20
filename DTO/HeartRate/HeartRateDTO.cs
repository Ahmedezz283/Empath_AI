using System.Text.Json.Serialization;

namespace Empath_AI.DTO.HeartRate
{
    public class HeartRateDTO
    {
        [JsonPropertyName("heartRate")]
        public double HeartRateValue { get; set; }
    }
}
