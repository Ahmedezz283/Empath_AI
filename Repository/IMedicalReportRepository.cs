using Empath_AI.DTO.MedicalReport;
using Empath_AI.Migrations;

namespace Empath_AI.Repository
{
    public interface IMedicalReportRepository
    {
        Task<(bool Success, string Message)> AddMedicalReport(int userId, MedicalReportDTO model);
        Task<Medical_Report?> FindMedicalReport(int id);
        Task<bool> UpdateMedicalReport(MedicalReportDTO usernm, int Id);
    }
}
