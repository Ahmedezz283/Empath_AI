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

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.DeviceToken == deviceToken && d.IsActive);

            if (device == null)
                return (false, "Unauthorized device");

            var existingHeartRate = await _context.Hearts
                     .FirstOrDefaultAsync(h => h.UserId == device.UserId);

            if (existingHeartRate != null)
            {
                existingHeartRate.HeartRateValue = model.HeartRateValue;
                existingHeartRate.Timestamp = egyptTime;
                _context.Hearts.Update(existingHeartRate);
            }
            else
            {
                var heartRate = new HeartRateRecord
                {
                    DeviceId = device.Id,
                    UserId = device.UserId,
                    HeartRateValue = model.HeartRateValue,
                    Timestamp = egyptTime
                };

                await _context.Hearts.AddAsync(heartRate);
            }


            device.Last_Active = egyptTime;
            _context.Devices.Update(device);
            await _context.SaveChangesAsync();

            return (true, "Heart rate recorded successfully");
        }

        public async Task<double?> GetLatestHeartRate(int userid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userid);
            if (user == null) return null;

            var record = await _context.Hearts
                .Where(h => h.UserId == user.Id)
                .OrderByDescending(h => h.Timestamp)
                .FirstOrDefaultAsync();

            return  record?.HeartRateValue;
        }
    }
}
