using Empath_AI.Data;
using Empath_AI.Model;
using Empath_AI.Migrations;
using Microsoft.EntityFrameworkCore;
using Empath_AI.DTO.MedicalReport;

namespace Empath_AI.Repository
{
    public class MedicalReportRepository : IMedicalReportRepository
    {
        private readonly AppDbContext _context;
    
        public MedicalReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> AddMedicalReport(int userId, MedicalReportDTO model)
        {
            var userExists = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userExists == null)
                return (false, "User not found");

            var existingReport = await _context.Medical_Reports
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (existingReport != null)
                return (false, "Medical report already exists for this user");

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var report = new Medical_Report
            {
                UserId = userId,
                Notes = model.Notes,
                HasBloodPressure = model.HasBloodPressure,
                HasHeartProblem = model.HasHeartProblem,
                HasDiabetes = model.HasDiabetes,
                IsSmoker = model.IsSmoker,
                CreatedAt = egyptTime,
            };

            await _context.Medical_Reports.AddAsync(report);
            await _context.SaveChangesAsync();
            return (true, "Medical report added successfully");
        }

    }
}
