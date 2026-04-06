using Empath_AI.Data;
using Empath_AI.DTO.GSR;
using Empath_AI.Repository;
using Empath_AI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Empath_AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GSRController : ControllerBase
    {
        private readonly IGSRRepository _repo;
        private readonly AppDbContext _context;
        private readonly AI_ModelService _emotionService;

        public GSRController(IGSRRepository repo, AppDbContext context, AI_ModelService emotionService)
        {
            _repo = repo;
            _context = context;
            _emotionService = emotionService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] GSRRecordDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");
            var authUserId = int.Parse(userIdClaim);

            if (dto.userid != 0)
                _emotionService.AddGSRReading(dto.userid, dto.RawGSRValue);

            if (dto == null) return BadRequest("Invalid data.");
            await _repo.AddAsync(dto,authUserId);
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
