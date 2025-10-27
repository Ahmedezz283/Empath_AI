using System.ComponentModel.DataAnnotations;

namespace Empath_AI.DTO.User
{
    public class UserForgetPasswordDTO
    {
        [Required]
        public string Email { get; set; }
    }
}
