using Empath_AI.Data;
using Empath_AI.DTO.Device;
using Empath_AI.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Empath_AI.Repository
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly AppDbContext _context;
        private readonly Token _token;

        public DeviceRepository(AppDbContext context, Token token)
        {
            _context = context;
            _token = token;
        }
        public async Task<User?> FindUser(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<Devices?> FindDevice(int id)
        {
            return await _context.Devices.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<(bool Success, string Message)> AddDevice(DeviceRegisterDTO model)
        {
            var user = await FindUser(model.UserId);
            if (user == null)
                return (false, "User not found");

            var existingDevice = await _context.Devices
                .FirstOrDefaultAsync(d => d.DeviceToken == model.DeviceToken && d.UserId == model.UserId);

            if (existingDevice != null)
                return (false, "Device already registered");

            var device = new Devices
            {
                UserId = model.UserId,
                Name = model.Name,
                serial_number = model.serial_number,
                IsActive = true,
                Created_At = DateTime.UtcNow,
                Last_Active = DateTime.UtcNow
            };
           
            await _context.Devices.AddAsync(device);
            await _context.SaveChangesAsync();

            device.DeviceToken = _token.CreateDeviceToken(device);

            _context.Devices.Update(device);
            await _context.SaveChangesAsync();

            return (true, "Device registered successfully");
        }

        public async Task<IEnumerable<Devices>> GetAllDevices()
        {
            return await _context.Devices.ToListAsync();
        }
        public async Task<IEnumerable<Devices>> GetDevicesByUserId(int userId)
        {
            return await _context.Devices.Where(d => d.UserId == userId).ToListAsync();
        }
        public async Task<Devices> GetDeviceById(int deviceId)
        {
            return await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
        }
        public async Task<(bool Success, string Message)> UpdateDeviceToken(int userId, string newToken)
        {
            var device = await FindDevice(userId);
            if (device == null)
                return (false, "Device not found");

            device.DeviceToken = newToken;
            device.Last_Active = DateTime.UtcNow;

            _context.Devices.Update(device);
            await _context.SaveChangesAsync();

            return (true, "Device token updated");
        }

    }
}
