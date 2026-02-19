namespace Empath_AI.DTO.User
{
    public class SocialLoginResponseDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
