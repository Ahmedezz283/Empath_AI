using Empath_AI.Data;
using Empath_AI.DTO.User;
using Empath_AI.Model;
using Empath_AI.Repository;
using Empath_AI.Service;
using Empath_AI.Services;
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
        private readonly FcmService _fcmService;

        public UserController(AppDbContext context, IUserRepository user, IConfiguration configuration, Email emailService, Token token, FcmService fcmService)
        {
            _context = context;
            _user = user;
            config = configuration;
            _emailService = emailService;
            _token = token;
            _fcmService = fcmService;
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
            /*if (!existUser.IsVerified)
                return Unauthorized("Please verify your account first. Check your email for the OTP.");*/

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

            /*var createdUser = await _user.FindUser(model.Email);
            var accessToken = _token.CreateToken(createdUser);*/

            return Ok(new { message = $"{ result.Message} check your email for an OTP", result.id/*, Token = accessToken,*/});
        }

        [Authorize]
        [HttpPost("upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture([FromForm] UserProfilePictureDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            int userId = int.Parse(userIdClaim);

            var result = await _user.AddUserProfile(userId, model);

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

            var resetLink = $"https://shahd237.github.io/Reset_Password/";
            await _emailService.SendEmailAsync(model.Email, "Reset Your Password",
    $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    </head>
    <body style='margin:0;padding:0;background-color:#f4f6fb;font-family:Arial,sans-serif;'>
        
        <!-- Wrapper -->
        <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f4f6fb;padding:40px 0;'>
            <tr>
                <td align='center'>
                    
                    <!-- Card -->
                    <table width='500' cellpadding='0' cellspacing='0' style='background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
                        
                        <!-- Header -->
                        <tr>
                            <td style='background:linear-gradient(135deg,#2c703f,#18993d);padding:40px;text-align:center;'>
                                <h1 style='color:#ffffff;margin:0;font-size:28px;font-weight:700;letter-spacing:1px;'>Empath AI</h1>
                                <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:14px;'>Your emotional wellness companion</p>
                            </td>
                        </tr>

                        <!-- Body -->
                        <tr>
                            <td style='padding:40px;text-align:center;'>
                                
                                <!-- Icon -->
                                <div style='width:64px;height:64px;background:#f0edff;border-radius:50%;margin:0 auto 24px;'>
                                    <span style='font-size:28px;line-height:64px;'>🔑</span>
                                </div>

                                <h2 style='color:#1a1a2e;font-size:22px;margin:0 0 8px;'>Reset Your Password</h2>
                                <p style='color:#6b7280;font-size:15px;margin:0 0 32px;line-height:1.6;'>
                                    We received a request to reset your password.<br>
                                    Click the button below to create a new one.
                                </p>

                                <!-- Reset Button -->
                                <a href='{resetLink}' 
                                   style='display:inline-block;background:linear-gradient(135deg,#2c703f,#18993d);color:#ffffff;text-decoration:none;font-size:16px;font-weight:700;padding:16px 40px;border-radius:12px;margin-bottom:32px;letter-spacing:0.5px;'>
                                    Reset My Password
                                </a>

                                <!-- Warning -->
                                <div style='background:#fff7ed;border:1px solid #fed7aa;border-radius:8px;padding:12px 20px;margin-bottom:32px;'>
                                    <p style='color:#c2410c;font-size:13px;margin:0;'>
                                        ⏱️ This link expires in <strong>15 minutes</strong>
                                    </p>
                                </div>

                                <!-- Fallback link -->
                                <p style='color:#9ca3af;font-size:12px;margin:0 0 8px;'>
                                    If the button doesn't work, copy and paste this link:
                                </p>
                                <p style='color:#6C63FF;font-size:12px;margin:0;word-break:break-all;'>
                                    {resetLink}
                                </p>

                            </td>
                        </tr>

                        <!-- Divider -->
                        <tr>
                            <td style='padding:0 40px;'>
                                <hr style='border:none;border-top:1px solid #f3f4f6;margin:0;'>
                            </td>
                        </tr>

                        <!-- Footer -->
                        <tr>
                            <td style='padding:24px 40px;text-align:center;'>
                                <p style='color:#9ca3af;font-size:12px;margin:0 0 4px;line-height:1.8;'>
                                    If you didn't request a password reset, ignore this email.<br>
                                    Your password will remain unchanged.
                                </p>
                                <p style='color:#9ca3af;font-size:12px;margin:8px 0 0;'>
                                    © 2026 Empath AI · This is an automated message, please do not reply.
                                </p>
                            </td>
                        </tr>

                    </table>
                    <!-- End Card -->

                </td>
            </tr>
        </table>
        <!-- End Wrapper -->

    </body>
    </html>
    ");


            return Ok("Reset link sent to your email.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDTO model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ResetToken == model.Token && u.ResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Invalid or expired token.");

            if (model.Password != model.Confirm_Password)
            {
                return Unauthorized("Not the same Password");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            user.Confirm_Password = BCrypt.Net.BCrypt.HashPassword(model.Confirm_Password);
            user.ResetToken = null;
            user.ResetTokenExpires = null;


            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Password reset successfully.");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _user.FindUser(userId);

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        [HttpPost("social-login")]
        public async Task<IActionResult> SocialLogin([FromBody] UserSocialLoginDTO model)
        {
            try
            {
                var (user, refreshToken) = await _user.SocialLoginAsync(model);
                var accessToken = _token.CreateToken(user);

                var response = new SocialLoginResponseDTO
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    Email = user.Email,
                    Role = user.Role
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _user.Logout(userId);

            if (!result)
                return NotFound("User not found");

            return Ok("Logged out successfully");
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var user = await _user.FindUser(userId);
            if (user == null)
                return NotFound("User not found");

            await _user.Delete(user);
            return Ok("Account deleted successfully");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _user.FindUser(id);
            if (user == null)
                return NotFound("User not found");

            await _user.Delete(user);
            return Ok("User deleted successfully");
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordDTO model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _user.FindUser(userId);

            if (user == null)
                return NotFound("User not found");

            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password))
                return BadRequest("Current password is incorrect");

            if (model.NewPassword != model.ConfirmNewPassword)
                return BadRequest("Passwords do not match");

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.Confirm_Password = user.Password;
            await _context.SaveChangesAsync();

            return Ok("Password changed successfully");
        }

        [Authorize]
        [HttpPost("emergency-contact")]
        public async Task<IActionResult> UpdateEmergencyContact([FromBody] string emergencyContact)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _user.FindUser(userId);

            if (user == null)
                return NotFound("User not found");

            user.Emergancy_Contact = emergencyContact;
            await _context.SaveChangesAsync();

            return Ok("Emergency contact updated successfully");
        }

       /* [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            var result = await _user.SendOtpAsync(email);
            if (!result)
                return NotFound("Email not found");

            return Ok("OTP sent to your email");
        }*/

        /*[HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] UserVerifyOtpDTO model)
        {
            var (success, message, id) = await _user.VerifyOtpAsync(model.Email, model.Otp);
            if (!success)
                return BadRequest(message);

            
            var user = await _user.FindUser(model.Email);
            var token = _token.CreateToken(user);

            return Ok(new { message, id, Token = token });
        }*/

        [Authorize]
        [HttpPost("save-fcm-token")]
        public async Task<IActionResult> SaveFcmToken([FromBody] string fcmToken)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _user.FindUser(userId);

            if (user == null)
                return NotFound("User not found");

            user.FcmToken = fcmToken;
            await _context.SaveChangesAsync();

            return Ok("FCM token saved");
        }

        [HttpPost("test-notification")]
        public async Task<IActionResult> TestNotification([FromBody] string fcmToken)
        {
            var result = await _fcmService.SendNotificationAsync(
                fcmToken,
                "Empath AI 💬",
                "This is a test notification from the backend!",
                new Dictionary<string, string>
                {
            { "conversationId", "1" },
            { "type", "bot_reply" }
                }
            );

            if (!result)
                return BadRequest("Failed to send notification");

            return Ok("Notification sent successfully");
        }
    }
}
