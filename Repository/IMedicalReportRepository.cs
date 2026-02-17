using Empath_AI.DTO.MedicalReport;
using Empath_AI.Migrations;

namespace Empath_AI.Repository
{
    public interface IMedicalReportRepository
    {
        Task<(bool Success, string Message)> AddMedicalReport(int userId, MedicalReportDTO model);
    }
}
