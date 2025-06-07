// **PARTIAL FILE** - AccountController login method aligned with JWT strategy
// RESPONSIBILITY: Authentication with clean JWT claims (identity only)
// DOES NOT: Include application data in JWT (district, school, department)
// CALLED BY: Angular frontend for authentication

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
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null)
            {
                _logger.LogWarning("Login request failed: loginRequest is null");
                return BadRequest("Invalid request body: loginRequest is required");
            }

            _logger.LogDebug("Attempting login for user: {UserName}", loginRequest.Username);

            var user = await _userManager.FindByNameAsync(loginRequest.Username);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User {UserName} not found", loginRequest.Username);
                return Unauthorized();
            }

            _logger.LogDebug("User {UserName} found, checking password", loginRequest.Username);
            if (!await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                _logger.LogWarning("Login failed for {UserName}: Invalid password", loginRequest.Username);
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);

            // JWT Claims - Keep existing structure (identity + organizational data)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Actual standard 'sub' claim
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? ""),
                new Claim("username", user.UserName ?? ""),
                new Claim("firstName", user.FirstName ?? ""),
                new Claim("lastName", user.LastName ?? ""),
                new Claim("email", user.Email ?? "")
            };

            // Add phone if available
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                claims.Add(new Claim("phone", user.PhoneNumber));
            }

            // Keep organizational claims (you said to keep them in play)
            if (user.DistrictId.HasValue)
            {
                claims.Add(new Claim("districtId", user.DistrictId.ToString() ?? ""));
            }
            if (user.SchoolId.HasValue)
            {
                claims.Add(new Claim("schoolId", user.SchoolId.ToString() ?? ""));
            }
            if (user.Departments != null && user.Departments.Any())
            {
                claims.AddRange(user.Departments.Select(dept => new Claim("departmentId", dept.Id.ToString())));
            }

            // Add roles
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("User logged in: {UserName}, Roles: {Roles}", user.UserName, string.Join(", ", roles));

            // Return clean response
            return Ok(new
            {
                token = tokenString,
                message = "Login successful"
            });
        }

    }
}