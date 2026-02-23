using Empath_AI.Data;
using Empath_AI.DTO.User;
using Empath_AI.Model;
using Empath_AI.Repository;
using Empath_AI.Service;
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
        private readonly Token _token;

        public UserController(AppDbContext context, IUserRepository user, IConfiguration configuration, Email emailService, Token token)
        {
            _context = context;
            _user = user;
            config = configuration;
            _emailService = emailService;
            _token = token;
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

            var accessToken = _token.CreateToken(existUser);
            var refreshToken = _token.GenerateRefreshToken();

            existUser.RefreshToken = refreshToken;
            existUser.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Token = accessToken,
                RefreshToken = refreshToken
            });
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

            var createdUser = await _user.FindUser(model.Email);
            var accessToken = _token.CreateToken(createdUser);

            return Ok(new { message = result.Message, result.id, Token = accessToken,});
        }

        [HttpPost("upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture([FromForm] UserProfilePictureDTO model)
        {
            var result = await _user.AddUserProfile(model);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message, imageUrl = result.ImageUrl });
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(int id, [FromBody] UserRegisterDTO instVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            User instructor = await _user.FindUser(id);

            if (instructor == null)
                return NotFound("User not found");

            var isUpdated = await _user.UpdateUser(instVM, id);

            if (!isUpdated)
                return BadRequest("Update failed");

            return Ok("User updated successfully");
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] UserTokenRequestDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpires <= DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var newAccessToken = _token.CreateToken(user);
            var newRefreshToken = _token.GenerateRefreshToken();

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

            var resetLink = $"https://www.youtube.com/";
            await _emailService.SendEmailAsync(model.Email, "Reset Your Password",
                 $"<a href='{resetLink}'>Click here to reset your password</a>");


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

        //[HttpPost("social-login")]
        //public async Task<IActionResult> SocialLogin([FromBody] UserSocialLoginDTO model)
        //{
        //    try
        //    {
        //        var (user, refreshToken) = await _user.SocialLoginAsync(model);
        //        var accessToken = _token.CreateToken(user);

        //        var response = new SocialLoginResponseDTO
        //        {
        //            Token = accessToken,
        //            RefreshToken = refreshToken,
        //            UserId = user.Id,
        //            Email = user.Email,
        //            Role = user.Role
        //        };

        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}


    }
}
