using Empath_AI.DTO.User;
using Empath_AI.Model;

namespace Empath_AI.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAll();
        Task<(bool Success, string Message, int? id)> CreateUserDetails(UserRegisterDTO user);
        Task<(bool Success, string Message, string? ImageUrl)> AddUserProfile(UserProfilePictureDTO model);
        Task<User?> FindUser(int id);
        Task<User?> FindUser(string email);
        Task<bool> UpdateUser(UserRegisterDTO usernm, int Id);
        Task Delete(User user);
        Task<bool> GeneratePasswordResetTokenAsync(string email, string token);
        Task<(User user, string refreshToken)> SocialLoginAsync(UserSocialLoginDTO model);
        Task<bool> Logout(int userId);
    }
}
