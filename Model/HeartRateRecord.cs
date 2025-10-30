namespace Empath_AI.Model
{
    public class HeartRateRecord
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public int? UserId { get; set; }
        public double HeartRateValue { get; set; }
        public DateTime Timestamp { get; set; }

        public Devices Device { get; set; }
    }
}
