using Empath_AI.DTO.User;
using Empath_AI.Model;

namespace Empath_AI.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAll();
        Task CreateUser(UserRegisterDTO user);
        Task<User?> FindUser(int id);
        Task<User?> FindUser(string name);
        Task<bool> UpdateUser(UserRegisterDTO usernm, int Id);
        Task Delete(User user);
    }
}
