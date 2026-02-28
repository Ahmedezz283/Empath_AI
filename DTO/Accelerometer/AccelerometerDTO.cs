using System.Text.Json.Serialization;

namespace Empath_AI.DTO.Accelerometer
{
    public class AccelerometerDTO
    {
        [JsonPropertyName("accelX")]
        public float AccelX { get; set; }

        [JsonPropertyName("accelY")]
        public float AccelY { get; set; }

        [JsonPropertyName("accelZ")]
        public float AccelZ { get; set; }

        [JsonPropertyName("stepCount")]
        public int StepCount { get; set; }

        [JsonPropertyName("activityLevel")]
        public string ActivityLevel { get; set; }

        [JsonPropertyName("fallDetected")]
        public bool FallDetected { get; set; }
    }
}
