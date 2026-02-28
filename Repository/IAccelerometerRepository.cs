using Empath_AI.DTO.Accelerometer;
using Empath_AI.Model;

namespace Empath_AI.Repository
{
    public interface IAccelerometerRepository
    {
        Task<(bool Success, string Message)> AddAsync(AccelerometerDTO data);
        Task<IEnumerable<Accelerometer>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Accelerometer>> GetFallsByUserIdAsync(int userId);
    }
}
