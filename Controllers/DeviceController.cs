using Empath_AI.Data;
using Empath_AI.DTO.Device;
using Empath_AI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Empath_AI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDeviceRepository _device;
        private readonly IConfiguration config;

        public DeviceController(IDeviceRepository device, AppDbContext context, IConfiguration config)
        {
            _device = device;
            _context = context;
            config = config;
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterDevice([FromBody] DeviceRegisterDTO model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            model.UserId = int.Parse(userIdClaim);

            var result = await _device.AddDevice(model);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }
    }
}
