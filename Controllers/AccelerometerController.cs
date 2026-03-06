using Empath_AI.Data;
using Empath_AI.DTO.Accelerometer;
using Empath_AI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Empath_AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccelerometerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAccelerometerRepository _accelerometerRepository;

        public AccelerometerController(AppDbContext context, IAccelerometerRepository accelerometerRepository)
        {
            _context = context;
            _accelerometerRepository = accelerometerRepository;
        }



        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] AccelerometerDTO dto)
        {
            if (dto == null) return BadRequest("Invalid data.");

            await _accelerometerRepository.AddAsync(dto);

            return Ok("Sensor data recorded successfully.");
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var data = await _accelerometerRepository.GetByUserIdAsync(userId);
            return Ok(data);
        }

        [HttpGet("falls/{userId}")]
        public async Task<IActionResult> GetFalls(int userId)
        {
            var falls = await _accelerometerRepository.GetFallsByUserIdAsync(userId);
            return Ok(falls);
        }
    }
}
