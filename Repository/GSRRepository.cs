using Empath_AI.Data;
using Empath_AI.DTO.GSR;
using Empath_AI.Model;
using Microsoft.EntityFrameworkCore;

namespace Empath_AI.Repository
{
    public class GSRRepository : IGSRRepository
    {
        private readonly AppDbContext _context;

        public GSRRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(GSRRecordDTO dto)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var record = new GSRRecord
            {
                RawGSRValue = dto.RawGSRValue,
                SkinConductance = dto.SkinConductance,
                StressLevel = dto.StressLevel ?? "low",
                StressScore = dto.StressScore,
                Timestamp = egyptTime
            };

            await _context.GSRRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<GSRRecord>> GetByUserIdAsync(int userId)
        {
            return await _context.GSRRecords
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<GSRRecord>> GetHighStressByUserIdAsync(int userId)
        {
            return await _context.GSRRecords
                .Where(g => g.UserId == userId && g.StressLevel == "high")
                .OrderByDescending(g => g.Timestamp)
                .ToListAsync();
        }
    }
}
