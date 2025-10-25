using Empath_AI.DTO;
using Empath_AI.Model;

namespace Empath_AI.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAll();
        Task CreateUser(UserRegisterDTO user);
    }
}
