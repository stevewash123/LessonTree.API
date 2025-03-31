using LessonTree.DAL;
using LessonTree.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.API.Controllers
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IConfiguration configuration,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger<AccountController> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserResource userResource)
        {
            if (userResource == null)
            {
                _logger.LogWarning("Login request failed: userResource is null");
                return BadRequest("Invalid request body: userResource is required");
            }

            _logger.LogDebug("Attempting login for user: {UserName}", userResource.Username);

            var user = await _userManager.FindByNameAsync(userResource.Username);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User {UserName} not found", userResource.Username);
                return Unauthorized();
            }

            _logger.LogDebug("User {UserName} found, checking password", userResource.Username);
            if (!await _userManager.CheckPasswordAsync(user, userResource.Password))
            {
                _logger.LogWarning("Login failed for {UserName}: Invalid password", userResource.Username);
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim("sub", user.Id.ToString()),
        new Claim("lastName", user.LastName ?? ""),
        new Claim("firstName", user.FirstName ?? ""),
        new Claim("districtId", user.DistrictId?.ToString() ?? ""), // Add SchoolId claim
        new Claim("schoolId", user.SchoolId?.ToString() ?? "") // Add SchoolId claim
    };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            _logger.LogInformation("User logged in: {UserName}, Roles: {Roles}", user.UserName, string.Join(", ", roles));
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
        // ... other actions omitted
    }
}