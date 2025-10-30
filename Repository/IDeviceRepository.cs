using Empath_AI.DTO.Device;
using Empath_AI.Model;

namespace Empath_AI.Repository
{
    public interface IDeviceRepository
    {
        Task<User?> FindUser(int id);
        Task<Devices?> FindDevice(int id);
        Task<(bool Success, string Message)> AddDevice(DeviceRegisterDTO model);
        Task<IEnumerable<Devices>> GetAllDevices();
        Task<IEnumerable<Devices>> GetDevicesByUserId(int userId);
        Task<Devices> GetDeviceById(int deviceId);
        Task<(bool Success, string Message)> UpdateDeviceToken(int userId, string newToken);

    }
}
