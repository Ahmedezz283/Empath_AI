using Empath_AI.Model;

public class Medical_Report
{
    public int Id { get; set; }

    public string? Notes { get; set; }

    public bool HasBloodPressure { get; set; }
    public bool HasHeartProblem { get; set; }
    public bool HasDiabetes { get; set; }
    public bool IsSmoker { get; set; }
    public bool HasAMentalIllness { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
