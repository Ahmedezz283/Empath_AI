namespace Empath_AI.DTO.MedicalReport
{
    public class MedicalReportDTO
    {
        public string? Notes { get; set; }

        public bool HasBloodPressure { get; set; }
        public bool HasHeartProblem { get; set; }
        public bool HasDiabetes { get; set; }
        public bool IsSmoker { get; set; }
    }
}
