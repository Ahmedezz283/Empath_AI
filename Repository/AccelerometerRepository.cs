using Empath_AI.Data;
using Empath_AI.DTO.Accelerometer;
using Empath_AI.Model;
using Microsoft.EntityFrameworkCore;

namespace Empath_AI.Repository
{
    public class AccelerometerRepository : IAccelerometerRepository
    {
        private readonly AppDbContext _context;

        public AccelerometerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> AddAsync(AccelerometerDTO data)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            /*var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.DeviceToken == deviceToken && d.IsActive);

            if (device == null)
                return (false, "Unauthorized device");

            var existingHeartRate = await _context.Hearts
                     .FirstOrDefaultAsync(h => h.UserId == device.UserId);*/

            /*if (existingHeartRate != null)
            {
                existingHeartRate.HeartRateValue = model.HeartRateValue;
                existingHeartRate.Timestamp = egyptTime;
                _context.Hearts.Update(existingHeartRate);
            }
            else*/
            //{
            var accelerometer = new Accelerometer
            {
                /* DeviceId = device.Id,
                 UserId = device.UserId,*/
                AccelX = data.AccelX,
                AccelY = data.AccelY,
                AccelZ = data.AccelZ,
                StepCount = data.StepCount,
                ActivityLevel = data.ActivityLevel,
                FallDetected = data.FallDetected,
                Timestamp = egyptTime
            };
            //  }
            await _context.Accelerometer.AddAsync(accelerometer);

            // device.Last_Active = egyptTime;
            //_context.Devices.Update(device);
            await _context.SaveChangesAsync();

            return (true, "Heart rate recorded successfully");
        }

        public async Task<IEnumerable<Accelerometer>> GetByUserIdAsync(int userId)
        {
            return await _context.Accelerometer
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<Accelerometer>> GetFallsByUserIdAsync(int userId)
        {
            return await _context.Accelerometer
                .Where(s => s.UserId == userId && s.FallDetected)
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }
    }
}
