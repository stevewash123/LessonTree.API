using LessonTree.API.Configuration;
using LessonTree.DAL;
using LessonTree.DAL.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public AdminController(IWebHostEnvironment env,
                             LessonTreeContext context,
                             UserManager<User> userManager,
                             RoleManager<IdentityRole<int>> roleManager,
                             ILogger<AdminController> logger,
                             IHostEnvironment hostEnvironment)
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

                // Clear all dependent tables
                _context.LessonAttachments.RemoveRange(_context.LessonAttachments);
                _context.LessonStandards.RemoveRange(_context.LessonStandards);
                _context.Notes.RemoveRange(_context.Notes);           // Added: Notes reference Lessons, Courses, Topics, SubTopics
                //_context.ScheduleDays.RemoveRange(_context.ScheduleDays); // Added: ScheduleDays reference Lessons
                //_context.Schedules.RemoveRange(_context.Schedules);   // Added: Schedules reference Courses
                _context.Lessons.RemoveRange(_context.Lessons);
                _context.SubTopics.RemoveRange(_context.SubTopics);
                _context.Topics.RemoveRange(_context.Topics);
                _context.Courses.RemoveRange(_context.Courses);
                _context.Standards.RemoveRange(_context.Standards);
                _context.Attachments.RemoveRange(_context.Attachments);
                _context.Departments.RemoveRange(_context.Departments);
                _context.Schools.RemoveRange(_context.Schools);
                _context.Districts.RemoveRange(_context.Districts);

                // Clear Identity-related tables
                _context.UserRoles.RemoveRange(_context.UserRoles);
                _context.Users.RemoveRange(_context.Users);
                _context.Roles.RemoveRange(_context.Roles);

                await _context.SaveChangesAsync();

                // Reseed the database
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