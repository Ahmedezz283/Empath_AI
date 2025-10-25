using Empath_AI.Data;
using Empath_AI.DTO;
using Empath_AI.Model;
using Microsoft.EntityFrameworkCore;

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

        public async Task CreateUser(UserRegisterDTO user)
        {
            var user1 = new User()
            {
                First_Name = user.First_Name,
                Last_Name = user.Last_Name,
                Email = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Image_URL = user.Image_URl,
                Phone = user.Phone,
                Age = user.Age,
                Emergancy_Contact = user.Emergancy_Contact,
                Gender = user.Gender?.ToLower() == "male",
            };
            await _context.Users.AddAsync(user1);
            await _context.SaveChangesAsync();
        }
        public async Task<User?> FindUser(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<User?> FindUser(string name)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.First_Name == name);
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
            user.Password = BCrypt.Net.BCrypt.HashPassword(usernm.Password);
            user.Age =usernm.Age;
            user.Phone = usernm.Phone;
            user.Image_URL = usernm.Image_URl;


            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task Delete(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
