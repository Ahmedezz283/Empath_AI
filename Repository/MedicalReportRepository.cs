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
                HasAMentalIllness = model.HasAMentalIllness,
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
            bool hasChanges = false;

            string SetIfChangedString(string? newValue, string currentValue)
            {
                if (!string.IsNullOrWhiteSpace(newValue) && newValue.ToLower() != "string" && newValue != currentValue)
                {
                    hasChanges = true;
                    return newValue;
                }
                return currentValue;
            }

            bool SetIfChangedBool(bool? newValue, bool currentValue)
            {
                if (newValue.HasValue && newValue.Value != currentValue)
                {
                    hasChanges = true;
                    return newValue.Value;
                }
                return currentValue;
            }


            user.IsSmoker = SetIfChangedBool(usernm.IsSmoker, user.IsSmoker);
            user.HasHeartProblem = SetIfChangedBool(usernm.HasHeartProblem, user.HasHeartProblem);
            user.HasBloodPressure = SetIfChangedBool(usernm.HasBloodPressure, user.HasBloodPressure);
            user.HasDiabetes = SetIfChangedBool(usernm.HasDiabetes, user.HasDiabetes);
            user.HasAMentalIllness = SetIfChangedBool(usernm.HasAMentalIllness, user.HasAMentalIllness);
            user.Notes = SetIfChangedString(usernm.Notes, user.Notes);


            void UpdateTimestamp()
            {
                user.UpdatedAt = DateTime.UtcNow;
                hasChanges = true;
            }

            if (!hasChanges)
                return true;


            await _context.SaveChangesAsync();
            return true;
        }
    }
}
