using Empath_AI.DTO.GSR;
using Empath_AI.Model;

namespace Empath_AI.Repository
{
    public interface IGSRRepository
    {
        Task AddAsync(GSRRecordDTO dto);
        Task<IEnumerable<GSRRecord>> GetByUserIdAsync(int userId);
        Task<IEnumerable<GSRRecord>> GetHighStressByUserIdAsync(int userId);
    }
}
