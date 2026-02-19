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

        public UserRepository(AppDbContext context, Token token, SocialAuthService socialAuthService)
        {
            _context = context;
            _token = token;
            _socialAuthService = socialAuthService;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<(bool Success, string Message, int? id)> CreateUserDetails(UserRegisterDTO user)
        {

            Console.WriteLine("Creating user...");

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
            };
            if (user1.Password != user1.Confirm_Password)
            {
                return (false, "Passwords do not match", null);
            }

            await _context.Users.AddAsync(user1);
            await _context.SaveChangesAsync();
            return (true, "User created successfully", user1.Id);
        }
        public async Task<(bool Success, string Message, string? ImageUrl)> AddUserProfile(UserProfilePictureDTO model)
        { 
            var user = await FindUser(model.Email);
            if (user == null)
                return (false, "User not found", null);

            if (model.Image == null)
                return (true, "No picture uploaded, keeping existing profile picture", user.Image_URL);

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.Image.CopyToAsync(stream);
            }

            user.Image_URL = $"/uploads/{fileName}";
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return (true, "Profile picture uploaded successfully", user.Image_URL);
        
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
            User user = await FindUser(Id);

            if (user == null)
            {
                return false;
            }

            user.First_Name = usernm.First_Name;
            user.Last_Name = usernm.Last_Name;
            user.Email = usernm.Email;
            user.Emergancy_Contact = usernm.Emergancy_Contact;
            user.Gender = usernm.Gender?.ToLower() == "male";
            user.Age =usernm.Age;
            user.Phone = usernm.Phone;


            _context.Users.Update(user);
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

    }
}
