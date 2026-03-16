using System.ComponentModel.DataAnnotations;

namespace Empath_AI.Model
{
    public class User
    {
        public int Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string? Image_URL { get; set; }
        [MinLength(11)]
        [MaxLength(11)]
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
        public string? Confirm_Password { get; set; }
        public bool? Gender { get; set; }
        public string Provider { get; set; } = "Local";
        public string Emergancy_Contact { get; set; }
        public int Age { get; set; }
        public string Role { get; set; } = "User";
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpires { get; set; }

        public DateTimeOffset Created_At { get; set; } = DateTimeOffset.UtcNow;
        public string? OtpCode { get; set; }
        public DateTime? OtpExpires { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? FcmToken { get; set; }

        //public ICollection<Conversation> conversations { get; set; }
    }
}
