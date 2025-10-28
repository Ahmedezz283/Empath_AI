using Empath_AI.Data;
using Empath_AI.DTO.User;
using Empath_AI.Model;
using Empath_AI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        [Authorize (Roles = "Admin")]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _user.FindUser(id);
            if (user == null)
                return NotFound("User not found");
            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDTO user)
        {
            var existUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == user.Email);

            if (existUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, existUser.Password))
            {
                return Unauthorized("Invalid email or password");
            }

            var accessToken = CreateToken(existUser);
            var refreshToken = GenerateRefreshToken();

            existUser.RefreshToken = refreshToken;
            existUser.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Token = accessToken,
                RefreshToken = refreshToken
            });
        }

        private string CreateToken(User exsitUser)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name , exsitUser.First_Name),
                new Claim(ClaimTypes.Email , exsitUser.Email),
                new Claim(ClaimTypes.Role , exsitUser.Role),
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

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] UserRegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _user.FindUser(model.Email);
            if (existingUser != null)
                return BadRequest("Email already exists.");

            var result = await _user.CreateUserDetails(model);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message, result.id });
        }

        [HttpPost("upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture([FromForm] UserProfilePictureDTO model)
        {
            var result = await _user.AddUserProfile(model);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message, imageUrl = result.ImageUrl });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] UserTokenRequestDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpires <= DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var newAccessToken = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            });
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

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            user.Confirm_Password = BCrypt.Net.BCrypt.HashPassword(model.Confirm_Password);
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            if (user.Password != user.Confirm_Password)
            {
                return Unauthorized("Not the same Password");
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Password reset successfully.");
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(string email)
        {
            var user = await _user.FindUser(email);
            if (user == null)
                return NotFound("User not found");
            return Ok(user);
        }

    }
}
