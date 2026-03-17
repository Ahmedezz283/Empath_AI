using Empath_AI.Data;
using Empath_AI.DTO.User;
using Empath_AI.Model;
using Empath_AI.Service;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Empath_AI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly Token _token;
        private readonly SocialAuthService _socialAuthService;
        private readonly Email _emailService;
       

        public UserRepository(AppDbContext context, Token token, SocialAuthService socialAuthService, Email emailService)
        {
            _context = context;
            _token = token;
            _socialAuthService = socialAuthService;
            _emailService = emailService;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        // Temporary storage for pending registrations
        private static readonly Dictionary<string, (UserRegisterDTO Data, string Otp, DateTime Expires)> _pendingUsers = new();

        /* public async Task<(bool Success, string Message, int? id)> CreateUserDetails(UserRegisterDTO user)
         {
             if (user.Password != user.Confirm_Password)
                 return (false, "Passwords do not match", null);

             // Check if email already exists in DB
             var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
             if (existingUser != null)
                 return (false, "Email already exists", null);

             // Check if already pending
             if (_pendingUsers.ContainsKey(user.Email))
                 _pendingUsers.Remove(user.Email);

             // Generate OTP
             var otp = GenerateOtp();

             // Store temporarily — do NOT save to DB yet
             _pendingUsers[user.Email] = (user, otp, DateTime.UtcNow.AddMinutes(10));

             // ✅ Send email directly with the otp variable
             await _emailService.SendEmailAsync(user.Email, "Your Empath AI Verification Code",
                 $@"
         <!DOCTYPE html>
         <html>
         <head>
             <meta charset='UTF-8'>
             <meta name='viewport' content='width=device-width, initial-scale=1.0'>
         </head>
         <body style='margin:0;padding:0;background-color:#f4f6fb;font-family:Arial,sans-serif;'>
             <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f4f6fb;padding:40px 0;'>
                 <tr>
                     <td align='center'>
                         <table width='500' cellpadding='0' cellspacing='0' style='background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
                             <tr>
                                 <td style='background:linear-gradient(135deg,#6C63FF,#a78bfa);padding:40px;text-align:center;'>
                                     <h1 style='color:#ffffff;margin:0;font-size:28px;font-weight:700;letter-spacing:1px;'>Empath AI</h1>
                                     <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:14px;'>Your emotional wellness companion</p>
                                 </td>
                             </tr>
                             <tr>
                                 <td style='padding:40px;text-align:center;'>
                                     <div style='width:64px;height:64px;background:#f0edff;border-radius:50%;margin:0 auto 24px;'>
                                         <span style='font-size:28px;line-height:64px;'>🔐</span>
                                     </div>
                                     <h2 style='color:#1a1a2e;font-size:22px;margin:0 0 8px;'>Verify Your Account</h2>
                                     <p style='color:#6b7280;font-size:15px;margin:0 0 32px;line-height:1.6;'>
                                         Enter the verification code below to complete your registration.
                                     </p>
                                     <div style='background:#f0edff;border-radius:12px;padding:24px;margin:0 0 32px;'>
                                         <p style='color:#6b7280;font-size:12px;text-transform:uppercase;letter-spacing:2px;margin:0 0 12px;'>Your verification code</p>
                                         <div style='font-size:42px;font-weight:800;letter-spacing:16px;color:#6C63FF;font-family:monospace;'>{otp}</div>
                                     </div>
                                     <div style='display:inline-block;background:#fff7ed;border:1px solid #fed7aa;border-radius:8px;padding:10px 20px;margin-bottom:32px;'>
                                         <p style='color:#c2410c;font-size:13px;margin:0;'>
                                             ⏱️ This code expires in <strong>10 minutes</strong>
                                         </p>
                                     </div>
                                     <p style='color:#9ca3af;font-size:13px;margin:0;line-height:1.6;'>
                                         If you didn't create an account with Empath AI,<br>you can safely ignore this email.
                                     </p>
                                 </td>
                             </tr>
                             <tr>
                                 <td style='padding:0 40px;'>
                                     <hr style='border:none;border-top:1px solid #f3f4f6;margin:0;'>
                                 </td>
                             </tr>
                             <tr>
                                 <td style='padding:24px 40px;text-align:center;'>
                                     <p style='color:#9ca3af;font-size:12px;margin:0;line-height:1.8;'>
                                         © 2026 Empath AI · Your emotional wellness companion<br>
                                         This is an automated message, please do not reply.
                                     </p>
                                 </td>
                             </tr>
                         </table>
                     </td>
                 </tr>
             </table>
         </body>
         </html>
         ");

             return (true, "OTP sent to your email. Please verify to complete registration.", null);
         }*/
        public async Task<(bool Success, string Message, int? id)> CreateUserDetails(UserRegisterDTO user)
        {

            Console.WriteLine("Creating user...");

            if (user.Password != user.Confirm_Password)
            {
                return (false, "Passwords do not match", null);
            }

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var user1 = new User()
            {
                First_Name = user.First_Name,
                Last_Name = user.Last_Name,
                Email = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Confirm_Password = BCrypt.Net.BCrypt.HashPassword(user.Confirm_Password),
                Phone = user.Phone,
                Age = user.Age,
                Role = "User",
                Emergancy_Contact = user.Emergancy_Contact,
                Gender = user.Gender?.ToLower() == "male",
                Created_At = egyptTime,
                IsVerified = false,
            };

            await _context.Users.AddAsync(user1);
            await _context.SaveChangesAsync();
            return (true, "User created successfully", user1.Id);
        }
        public async Task<(bool Success, string Message, string? ImageUrl)> AddUserProfile(int userId ,UserProfilePictureDTO model)
        {
            var userExists = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userExists == null)
                return (false, "User not found","");

            var existingReport = await _context.Medical_Reports
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (model.Image == null)
                return (true, "No picture uploaded, keeping existing profile picture", userExists.Image_URL);

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.Image.CopyToAsync(stream);
            }

            userExists.Image_URL = $"/uploads/{fileName}";
            _context.Users.Update(userExists);
            await _context.SaveChangesAsync();

            return (true, "Profile picture uploaded successfully", userExists.Image_URL);
        
        }
        
        public async Task<User?> FindUser(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<User?> FindUser(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }
        public async Task<bool> UpdateUser(UserRegisterDTO usernm, int Id)
        {
            var user = await FindUser(Id);

            if (user == null)
                return false;

            bool hasChanges = false;

            string SetIfChangedString(string? newValue, string currentValue)
            {
                if (!string.IsNullOrWhiteSpace(newValue) && newValue.ToLower() != "string" && newValue != currentValue)
                {
                    hasChanges = true;
                    return newValue;
                }
                return currentValue;
            }

            int SetIfChangedInt(int? newValue, int currentValue)
            {
                if (newValue.HasValue && newValue.Value != 0 && newValue.Value != currentValue)
                {
                    hasChanges = true;
                    return newValue.Value;
                }
                return currentValue;
            }

            user.First_Name = SetIfChangedString(usernm.First_Name, user.First_Name);
            user.Last_Name = SetIfChangedString(usernm.Last_Name, user.Last_Name);
            user.Email = SetIfChangedString(usernm.Email, user.Email);
            user.Emergancy_Contact = SetIfChangedString(usernm.Emergancy_Contact, user.Emergancy_Contact);
            user.Phone = SetIfChangedString(usernm.Phone, user.Phone);
            user.Age = SetIfChangedInt(usernm.Age, user.Age);

            if (!string.IsNullOrWhiteSpace(usernm.Gender) && usernm.Gender.ToLower() != "string")
            {
                var newGender = usernm.Gender.ToLower() == "male";
                if (newGender != user.Gender)
                {
                    user.Gender = newGender;
                    hasChanges = true;
                }
            }

            if (!hasChanges)
                return true;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task Delete(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> GeneratePasswordResetTokenAsync(string email, string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            user.ResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(15);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
        /*        public async Task<(User user, string refreshToken)> SocialLoginAsync(UserSocialLoginDTO model)
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == model.Email);

                    if (user == null)
                    {
                        user = new User
                        {
                            Email = model.Email,
                            First_Name = model.FirstName,
                            Last_Name = model.LastName,
                            Provider = model.Provider,
                            Image_URL = model.ImageUrl,
                            Emergancy_Contact = model.Emergancy_Contact,
                            Phone = model.Phone,
                            Gender = model.Gender.HasValue ? model.Gender.Value : (bool?)null,//test
                            Password = null,
                            Role = "User",
                            Created_At = DateTimeOffset.UtcNow
                        };
                        await _context.Users.AddAsync(user);
                        await _context.SaveChangesAsync();
                    }


                    var refreshToken = _token.GenerateRefreshToken();
                    user.RefreshToken = refreshToken;
                    await _context.SaveChangesAsync();

                    return (user, refreshToken);
                }
        */

        public async Task<bool> Logout(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return false;

            user.RefreshToken = null;
            user.RefreshTokenExpires = null;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<(User user, string refreshToken)> SocialLoginAsync(UserSocialLoginDTO model)
        {
            // 1️⃣ Validate token and get full user info
            var validatedUser = await _socialAuthService.ValidateSocialTokenAsync(model.Provider, model.Token);

            // 2️⃣ Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == validatedUser.Email);

            if (user == null)
            {
                // 3️⃣ Create new user with safe defaults for non-nullable fields
                user = new User
                {
                    Email = validatedUser.Email,
                    First_Name = validatedUser.FirstName,
                    Last_Name = validatedUser.LastName,
                    Provider = validatedUser.Provider,
                    Image_URL = validatedUser.ImageUrl,
                    Password = null,
                    Role = "User",
                    Created_At = DateTimeOffset.UtcNow,
                    Phone = "",                 // safe default for non-nullable
                    Emergancy_Contact = ""      // safe default
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }

            // 4️⃣ Generate refresh token and save
            var refreshToken = _token.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            await _context.SaveChangesAsync();

            return (user, refreshToken);
        }
        public string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }
        /*public async Task<bool> SendOtpAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            var otp = GenerateOtp();
            user.OtpCode = otp;
            user.OtpExpires = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(email, "Your Empath AI Verification Code",
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
                                <div style='width:64px;height:64px;background:#f0edff;border-radius:50%;margin:0 auto 24px;display:flex;align-items:center;justify-content:center;'>
                                    <span style='font-size:28px;'>🔐</span>
                                </div>

                                <h2 style='color:#1a1a2e;font-size:22px;margin:0 0 8px;'>Verify Your Account</h2>
                                <p style='color:#6b7280;font-size:15px;margin:0 0 32px;line-height:1.6;'>
                                    Enter the verification code below to complete your registration.
                                </p>

                                <!-- OTP Box -->
                                <div style='background:#f0edff;border-radius:12px;padding:24px;margin:0 0 32px;'>
                                    <p style='color:#6b7280;font-size:12px;text-transform:uppercase;letter-spacing:2px;margin:0 0 12px;'>Your verification code</p>
                                    <div style='font-size:42px;font-weight:800;letter-spacing:16px;color:#6C63FF;font-family:monospace;'>{otp}</div>
                                </div>

                                <!-- Timer -->
                                <div style='display:inline-block;background:#fff7ed;border:1px solid #fed7aa;border-radius:8px;padding:10px 20px;margin-bottom:32px;'>
                                    <p style='color:#c2410c;font-size:13px;margin:0;'>
                                        ⏱️ This code expires in <strong>10 minutes</strong>
                                    </p>
                                </div>

                                <p style='color:#9ca3af;font-size:13px;margin:0;line-height:1.6;'>
                                    If you didn't create an account with Empath AI,<br>you can safely ignore this email.
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
                                <p style='color:#9ca3af;font-size:12px;margin:0;line-height:1.8;'>
                                    © 2026 Empath AI · Your emotional wellness companion<br>
                                    This is an automated message, please do not reply.
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

            return true;
        }*/
        /* public async Task<(bool Success, string Message, int? id)> VerifyOtpAsync(string email, string otp)
         {
             // Check pending users
             if (_pendingUsers.TryGetValue(email, out var pending))
             {
                 if (pending.Otp != otp)
                     return (false, "Invalid OTP", null);

                 if (pending.Expires < DateTime.UtcNow)
                 {
                     _pendingUsers.Remove(email);
                     return (false, "OTP expired, please register again", null);
                 }

                 // ✅ OTP valid → create account now
                 var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                 var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

                 var user = new User()
                 {
                     First_Name = pending.Data.First_Name,
                     Last_Name = pending.Data.Last_Name,
                     Email = pending.Data.Email,
                     Password = BCrypt.Net.BCrypt.HashPassword(pending.Data.Password),
                     Confirm_Password = BCrypt.Net.BCrypt.HashPassword(pending.Data.Confirm_Password),
                     Phone = pending.Data.Phone,
                     Age = pending.Data.Age,
                     Role = "User",
                     Emergancy_Contact = pending.Data.Emergancy_Contact,
                     Gender = pending.Data.Gender?.ToLower() == "male",
                     Created_At = egyptTime,
                     IsVerified = true
                 };

                 await _context.Users.AddAsync(user);
                 await _context.SaveChangesAsync();

                 _pendingUsers.Remove(email);

                 return (true, "Account verified and created successfully", user.Id);
             }

             return (false, "No pending registration found for this email", null);
         }*/
    }
}
