namespace Empath_AI.DTO.User
{
    public class UserSocialLoginDTO
    {
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Provider { get; set; } = null!; // "Google" or "Facebook"
        public string? ImageUrl { get; set; }
    }
}
