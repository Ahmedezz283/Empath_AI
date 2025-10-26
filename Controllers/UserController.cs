using Empath_AI.Data;
using Empath_AI.DTO.User;
using Empath_AI.Model;
using Empath_AI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Empath_AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _user;
        private readonly IConfiguration config;

        public UserController(AppDbContext context, IUserRepository user, IConfiguration configuration)
        {
            _context = context;
            _user = user;
            config = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDTO user)
        {
            var existUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == user.Email);

            if (existUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, existUser.Password))
            {
                return Unauthorized("Invalid email or password");
            }
            var token = CreateToken(existUser);

            return Ok(new { token });

        }
        private string CreateToken(User exsitUser)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name , exsitUser.First_Name),
                new Claim(ClaimTypes.Email , exsitUser.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));

            JwtSecurityToken token = new JwtSecurityToken
                (
                    issuer: config["JWT:Issure"],
                    audience: config["JWT:Audience"],
                    claims: userClaims,
                    expires: DateTime.UtcNow.AddMinutes(15),
                    signingCredentials:new SigningCredentials(signingKey,SecurityAlgorithms.HmacSha256 )
                );

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenStr;

        }
    }
}
