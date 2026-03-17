using System.ComponentModel.DataAnnotations;

namespace Empath_AI.DTO.User
{
    public class UserResetPasswordDTO
    {
        public string Token { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Confirm_Password { get; set; }
    }
}
