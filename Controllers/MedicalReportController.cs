using Empath_AI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Empath_AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalReportController : ControllerBase
    {
        private readonly IMedicalReportRepository _medicalReportRepository;
        private readonly AppDbContext _context;

        public MedicalReportController(IMedicalReportRepository medicalReportRepository, AppDbContext context)
        {
            _medicalReportRepository = medicalReportRepository;
            _context = context;
        }

        //[Authorize]
        [HttpPost("AddMedicalReport")]
        public async Task<IActionResult> AddMedicalReport([FromBody] Medical_Report model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

           /* var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");*/

            int userId = int.Parse(userIdClaim);

            var result = await _medicalRepo.AddMedicalReport(userId, model);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

    }
}
