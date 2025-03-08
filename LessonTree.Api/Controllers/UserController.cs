using LessonTree.BLL.Service;
using LessonTree.Models;
using LessonTree.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ILogger<UserController> _logger; // Add logging

        public UserController(IUserService service, ILogger<UserController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            _logger.LogDebug("Entering GetUsers");
            var users = _service.GetAll();
            var userResources = users.Select(u => new UserResource
            {
                Id = u.Id,
                Username = u.UserName, // Updated from Username
                Password = null // Exclude password for security
            }).ToList();
            _logger.LogDebug("Returning {Count} users", userResources.Count);
            return Ok(userResources);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            _logger.LogDebug("Entering GetUser with ID: {UserId}", id);
            var user = _service.GetById(id);
            if (user == null)
            {
                _logger.LogError("User with ID {UserId} not found", id);
                return NotFound();
            }
            var userResource = new UserResource
            {
                Id = user.Id,
                Username = user.UserName, // Updated from Username
                Password = null // Exclude password for security
            };
            _logger.LogDebug("Returning user: {UserName}", user.UserName); // Updated from Username
            return Ok(userResource);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserResource userResource)
        {
            _logger.LogDebug("Entering UpdateUser with ID: {UserId}", id);
            if (id != userResource.Id)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} does not match body ID {BodyId}", id, userResource.Id);
                return BadRequest();
            }
            var user = _service.GetById(id);
            if (user == null)
            {
                _logger.LogError("User with ID {UserId} not found", id);
                return NotFound();
            }
            user.UserName = userResource.Username; // Updated from Username
            _service.Update(user);
            _logger.LogInformation("Updated user with ID: {UserId}, UserName: {UserName}", id, user.UserName); // Updated from Username
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            _logger.LogDebug("Entering DeleteUser with ID: {UserId}", id);
            _service.Delete(id);
            _logger.LogInformation("Deleted user with ID: {UserId}", id);
            return NoContent();
        }
    }
}