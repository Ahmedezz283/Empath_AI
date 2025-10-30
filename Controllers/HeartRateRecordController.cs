using Empath_AI.Data;
using Empath_AI.DTO.HeartRate;
using Empath_AI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Empath_AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeartRateRecordController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHeartRateRepository _heart;

        public HeartRateRecordController(IHeartRateRepository heart, AppDbContext context)
        {
            _heart = heart;
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddHeartRate([FromBody] HeartRateDTO model)
        {
            var deviceToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var result = await _heart.AddHeartRateAsync(deviceToken, model);

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
