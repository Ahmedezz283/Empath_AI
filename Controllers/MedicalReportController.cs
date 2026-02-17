using Empath_AI.Data;
using Empath_AI.DTO.MedicalReport;
using Empath_AI.Model;
using Empath_AI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Empath_AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalReportController : ControllerBase
    {
        private readonly IMedicalReportRepository _medicalRepo;
        private readonly AppDbContext _context;

        public MedicalReportController(IMedicalReportRepository medicalReportRepository, AppDbContext context)
        {
            _medicalRepo = medicalReportRepository;
            _context = context;
        }

        [Authorize]
        [HttpPost("AddMedicalReport")]
        public async Task<IActionResult> AddMedicalReport([FromBody] MedicalReportDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            int userId = int.Parse(userIdClaim);
            

            var result = await _medicalRepo.AddMedicalReport(userId, model);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

    }
}
