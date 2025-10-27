using Empath_AI.Data;
using Empath_AI.DTO.User;
using Empath_AI.Model;
using Empath_AI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Empath_AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _user;
        private readonly IConfiguration config;
        private readonly Email _emailService;

        public UserController(AppDbContext context, IUserRepository user, IConfiguration configuration, Email emailService)
        {
            _context = context;
            _user = user;
            config = configuration;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDTO user)
        {
            var existUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == user.Email);

            if (existUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, existUser.Password))
            {
                return Unauthorized("Invalid email or password");
            }
            var token = CreateToken(existUser);

            return Ok(new { token });

        }
        private string CreateToken(User exsitUser)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name , exsitUser.First_Name),
                new Claim(ClaimTypes.Email , exsitUser.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));

            JwtSecurityToken token = new JwtSecurityToken
                (
                    issuer: config["JWT:Issure"],
                    audience: config["JWT:Audience"],
                    claims: userClaims,
                    expires: DateTime.UtcNow.AddMinutes(15),
                    signingCredentials:new SigningCredentials(signingKey,SecurityAlgorithms.HmacSha256 )
                );

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenStr;

        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] UserForgetPasswordDTO model)
        {
            var token = Guid.NewGuid().ToString(); 
            var saved = await _user.GeneratePasswordResetTokenAsync(model.Email, token);

            if (!saved)
                return NotFound("Email not found");

            var resetLink = $"https://your-frontend-app.com/reset-password?token={token}";
            await _emailService.SendEmailAsync(model.Email, "Reset Your Password",
                $"Click the link to reset your password: {resetLink}");

            return Ok("Reset link sent to your email.");
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDTO model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ResetToken == model.Token && u.ResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Invalid or expired token.");

            user.Password = model.Password;
            user.Confirm_Password = model.Confirm_Password;
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Password reset successfully.");
        }
    }
}
