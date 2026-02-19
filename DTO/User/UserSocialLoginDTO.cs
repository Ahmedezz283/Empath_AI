using System.ComponentModel.DataAnnotations;

namespace Empath_AI.DTO.User
{
    public class UserSocialLoginDTO
    {
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Provider { get; set; } = null!; // "Google" or "Facebook"
        public string? ImageUrl { get; set; }
        public string Emergancy_Contact { get; set; }
        public string Phone { get; set; }
        public bool? Gender { get; set; }
        public string Token { get; set; }
    }
}
