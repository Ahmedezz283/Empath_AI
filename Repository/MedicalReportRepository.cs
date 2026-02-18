using Empath_AI.Data;
using Empath_AI.DTO.MedicalReport;
using Empath_AI.DTO.User;
using Empath_AI.Migrations;
using Empath_AI.Model;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Medical_Report?> FindMedicalReport(int id)
        {
            return await _context.Medical_Reports.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<bool> UpdateMedicalReport(MedicalReportDTO usernm, int Id)
        {
            Medical_Report user = await FindMedicalReport(Id);

            if (user == null)
            {
                return false;
            }

            user.Notes = usernm.Notes;
            user.HasBloodPressure = usernm.HasBloodPressure;
            user.HasHeartProblem = usernm.HasHeartProblem;
            user.HasDiabetes = usernm.HasDiabetes;
            user.IsSmoker = usernm.IsSmoker;
            user.UpdatedAt = usernm.UpdatedAt;


            _context.Medical_Reports.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
