using Empath_AI.Data;
using Empath_AI.DTO.GSR;
using Empath_AI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Empath_AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GSRController : ControllerBase
    {
        private readonly IGSRRepository _repo;
        private readonly AppDbContext _context;

        public GSRController(IGSRRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] GSRRecordDTO dto)
        {
            if (dto == null) return BadRequest("Invalid data.");
            await _repo.AddAsync(dto);
            return Ok("GSR recorded successfully.");
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var data = await _repo.GetByUserIdAsync(userId);
            return Ok(data);
        }

        [HttpGet("stress/{userId}")]
        public async Task<IActionResult> GetHighStress(int userId)
        {
            var data = await _repo.GetHighStressByUserIdAsync(userId);
            return Ok(data);
        }
    }
}
