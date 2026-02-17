using Empath_AI.Migrations;

namespace Empath_AI.Repository
{
    public class MedicalReportRepository : IMedicalReportRepository
    {
        private readonly AppDbContext _context;
    
        public MedicalReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> AddMedicalReport(int userId, MedicalReport model)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return (false, "User not found");

            var existingReport = await _context.Medical_Reports
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (existingReport != null)
                return (false, "Medical report already exists for this user");

            var report = new Medical_Report
            {
                UserId = userId,
                Notes = model.Notes,
                Blood_Pressure = model.Blood_Pressure,
                Heart_problem = model.Heart_problem,
                Diabetes = model.Diabetes,
                smoking = model.smoking
            };

            await _context.Medical_Reports.AddAsync(report);
            await _context.SaveChangesAsync();
            return (true, "Medical report added successfully");
        }

    }
}
