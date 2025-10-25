using System.ComponentModel.DataAnnotations;

namespace Empath_AI.DTO
{
    public class UserRegisterDTO
    {
        public string First_Name { get; set; }
        [Required]
        public string Last_Name { get; set; }
        [Required]
        public string Phone { get; set; }
        public string? Image_URl {  get; set; }
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string? Gender { get; set; }
        public string Emergancy_Contact { get; set; }
        public int Age { get; set; }
    }
}
