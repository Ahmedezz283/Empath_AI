using System.Text.Json.Serialization;

namespace Empath_AI.DTO.GSR
{
    public class GSRRecordDTO
    {
        [JsonPropertyName("rawGSRValue")]
        public int RawGSRValue { get; set; }

        [JsonPropertyName("skinConductance")]
        public float SkinConductance { get; set; }

        [JsonPropertyName("stressLevel")]
        public string StressLevel { get; set; }

        [JsonPropertyName("stressScore")]
        public int StressScore { get; set; }
        public int userid { get; set; }
    }
}
