using Empath_AI.DTO.User;
using Empath_AI.Model;

namespace Empath_AI.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAll();
        Task CreateUserDetails(UserRegisterDTO user);
        Task<bool> AddUserProfile(int user_id, string URL);
        Task<User?> FindUser(int id);
        Task<User?> FindUser(string name);
        Task<bool> UpdateUser(UserRegisterDTO usernm, int Id);
        Task Delete(User user);
        Task<bool> GeneratePasswordResetTokenAsync(string email, string token);
    }
}
