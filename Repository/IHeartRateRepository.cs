using Empath_AI.DTO.HeartRate;

namespace Empath_AI.Repository
{
    public interface IHeartRateRepository
    {
        Task<(bool Success, string Message)> AddHeartRateAsync(string deviceToken, HeartRateDTO model);
        Task<double?> GetLatestHeartRate(int userid);
    }
}
