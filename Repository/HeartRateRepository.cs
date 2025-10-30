using Empath_AI.Data;
using Empath_AI.DTO.HeartRate;
using Empath_AI.Model;
using Microsoft.EntityFrameworkCore;

namespace Empath_AI.Repository
{
    public class HeartRateRepository : IHeartRateRepository
    {
        private readonly AppDbContext _context;

        public HeartRateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> AddHeartRateAsync(string deviceToken, HeartRateDTO model)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.DeviceToken == deviceToken && d.IsActive);

            if (device == null)
                return (false, "Unauthorized device");

            var heartRate = new HeartRateRecord
            {
                DeviceId = device.Id,
                UserId = device.UserId,
                HeartRateValue = model.HeartRateValue,
                Timestamp = DateTime.UtcNow
            };

            await _context.Hearts.AddAsync(heartRate);

            
            device.Last_Active = DateTime.UtcNow;
            _context.Devices.Update(device);
            await _context.SaveChangesAsync();

            return (true, "Heart rate recorded successfully");
        }

    }
}
