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
        public async Task<IActionResult> Add([FromBody] HeartRateDTO model)
        {
            try
            {
                Console.WriteLine($"📩 Received from Arduino: {model.HeartRateValue}");
                var result = await _heart.AddHeartRateAsync(model);
                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
