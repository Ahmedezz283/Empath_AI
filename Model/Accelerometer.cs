namespace Empath_AI.Model
{
    public class Accelerometer
    {
        public int Id { get; set; }
        public int? UserId { get; set; }

        // Accelerometer raw data
        public float AccelX { get; set; }
        public float AccelY { get; set; }
        public float AccelZ { get; set; }

        // Activity
        public int StepCount { get; set; }
        public string ActivityLevel { get; set; } // "sitting", "walking", "running"

        // Fall detection
        public bool FallDetected { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int? DeviceId { get; set; }
        public Devices Device { get; set; }
    }
}
