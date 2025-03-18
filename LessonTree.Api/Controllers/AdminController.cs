using LessonTree.API.Configuration;
using LessonTree.DAL;
using LessonTree.DAL.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly LessonTreeContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<AdminController> _logger; 
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IWebHostEnvironment _env;

        public AdminController(IWebHostEnvironment env, LessonTreeContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, ILogger<AdminController> logger, IHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _env = env;
        }

        [HttpPost("reset-and-reseed")]
        public async Task<IActionResult> ResetAndReseed()
        {
            if (!_env.IsDevelopment())
            {
                _logger.LogWarning("Attempted to reset and reseed database in non-Development environment: {Environment}", _env.EnvironmentName);
                return Forbid("Reset and reseed is only allowed in Development environment.");
            }

            _logger.LogDebug("Resetting and reseeding database in Development environment");
            try
            {
                _logger.LogInformation("Resetting and reseeding database...");
                _context.Database.ExecuteSqlRaw("DELETE FROM LessonAttachments;");
                _context.Database.ExecuteSqlRaw("DELETE FROM LessonStandards;");
                _context.Database.ExecuteSqlRaw("DELETE FROM Lessons;");
                _context.Database.ExecuteSqlRaw("DELETE FROM SubTopics;");
                _context.Database.ExecuteSqlRaw("DELETE FROM Topics;");
                _context.Database.ExecuteSqlRaw("DELETE FROM Courses;");
                _context.Database.ExecuteSqlRaw("DELETE FROM Standards;");
                _context.Database.ExecuteSqlRaw("DELETE FROM Attachments;");
                _context.Database.ExecuteSqlRaw("DELETE FROM AspNetUserRoles;");
                _context.Database.ExecuteSqlRaw("DELETE FROM AspNetUsers;");
                _context.Database.ExecuteSqlRaw("DELETE FROM AspNetRoles;");
                _context.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence;");
                await DatabaseSeeder.SeedDatabaseAsync(_context, _userManager, _roleManager, _logger, _hostEnvironment);
                _logger.LogInformation("Database reset and reseeded successfully.");
                return Ok("Database reset and reseeded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset and reseed database: {Message}", ex.Message);
                return StatusCode(500, "Failed to reset and reseed database: " + ex.Message);
            }
        }
    }
}