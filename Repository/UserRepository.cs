using Empath_AI.Data;
using Empath_AI.DTO.User;
using Empath_AI.Model;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Empath_AI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
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
        /* public async Task<bool> AddMedicalReportAsync(int userId, string reportPath, string description)
     {
         var report = new MedicalReport
         {
             UserId = userId,
             ReportFile = reportPath,
             Description = description,
             CreatedAt = DateTime.UtcNow
         };

         _context.MedicalReports.Add(report);
         await _context.SaveChangesAsync();
         return true;
     }*/
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
    }
}
