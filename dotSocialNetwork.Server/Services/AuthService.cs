using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using dotSocialNetwork.Server.Data;
using dotSocialNetwork.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace dotSocialNetwork.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IHttpContextAccessor httpContext, IConfiguration config)
        {
            _context = context;
            _httpContext = httpContext;
            _config = config;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) ///?????????????????????????
                throw new Exception("Invalid credentials");

            var token = GenerateJwtToken(user);
            return token;
        }

        public int GetCurrentUserId()
        {
            var userId = _httpContext.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? int.Parse(userId) : 0;
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<List<User>> SearchUsersAsync(string query)
        {
            var currentUserId = GetCurrentUserId();
            return await _context.Users
                .Where(u => u.Username.Contains(query) && u.Id != currentUserId)
                .Select(u => new User
                {
                    Id = u.Id,
                    Username = u.Username
                })
                .Take(10) // Ограничим количество результатов
                .ToListAsync();
        }
    }
}
