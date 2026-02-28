namespace Empath_AI.Model
{
    public class GSRRecord
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int RawGSRValue { get; set; }
        public float SkinConductance { get; set; }
        public string StressLevel { get; set; } // "low", "medium", "high"
        public int StressScore { get; set; }    // 0-100
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
