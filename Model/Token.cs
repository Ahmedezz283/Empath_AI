using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Empath_AI.Model
{
    public class Token
    {
        private readonly IConfiguration config;

        public Token(IConfiguration config)
        {
            this.config = config;
        }

        public string CreateToken(User exsitUser)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name , exsitUser.First_Name),
                new Claim(ClaimTypes.Email , exsitUser.Email),
                new Claim(ClaimTypes.Role , exsitUser.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));

            JwtSecurityToken token = new JwtSecurityToken
                (
                    issuer: config["JWT:Issure"],
                    audience: config["JWT:Audience"],
                    claims: userClaims,
                    expires: DateTime.UtcNow.AddMinutes(15),
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenStr;

        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}
