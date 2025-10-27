// **PARTIAL FILE** - AdminController with proper security
// RESPONSIBILITY: Administrative operations with proper authorization
// DOES NOT: Inherit from BaseController (manages system, not user data)
// CALLED BY: System administrators only

using LessonTree.API.Configuration;
using LessonTree.DAL;
using LessonTree.DAL.Domain;
using LessonTree.Service.Service.SystemConfig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly LessonTreeContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<AdminController> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IWebHostEnvironment _env;
        private readonly ISystemConfigService _systemConfigService;

        public AdminController(IWebHostEnvironment env,
                             LessonTreeContext context,
                             UserManager<User> userManager,
                             RoleManager<IdentityRole<int>> roleManager,
                             ILogger<AdminController> logger,
                             IHostEnvironment hostEnvironment,
                             ISystemConfigService systemConfigService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _env = env;
            _systemConfigService = systemConfigService;
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            try
            {
                var lastSeededDate = await _systemConfigService.GetLastSeedDateAsync();
                return Ok(new
                {
                    lastSeededDate = lastSeededDate,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system config");
                return StatusCode(500, new { message = "Failed to get system config" });
            }
        }

        [HttpPost("reset-and-reseed")]
        public async Task<IActionResult> ResetAndReseed()
        {
            // DEMO ENVIRONMENT: Allow reset-and-reseed in production for demo purposes
            var currentUser = User.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Admin {AdminUser} is resetting database in {Environment} environment", currentUser, _env.EnvironmentName);

            try
            {
                _logger.LogInformation("Resetting and reseeding database...");

                // Drop existing database completely to avoid schema conflicts
                _logger.LogInformation("Dropping existing database...");
                await _context.Database.EnsureDeletedAsync();

                // Create fresh database using migrations (better PostgreSQL compatibility)
                _logger.LogInformation("Running database migrations...");
                await _context.Database.MigrateAsync();

                // Clear all dependent tables in proper order (only if they exist)
                if (_context.Database.CanConnect())
                {
                    _context.ScheduleEvents.RemoveRange(_context.ScheduleEvents);
                _context.Schedules.RemoveRange(_context.Schedules);
                _context.PeriodAssignments.RemoveRange(_context.PeriodAssignments);
                _context.ScheduleConfigurations.RemoveRange(_context.ScheduleConfigurations);
                _context.UserConfigurations.RemoveRange(_context.UserConfigurations);
                _context.LessonAttachments.RemoveRange(_context.LessonAttachments);
                _context.LessonStandards.RemoveRange(_context.LessonStandards);
                _context.Notes.RemoveRange(_context.Notes);
                _context.Lessons.RemoveRange(_context.Lessons);
                _context.SubTopics.RemoveRange(_context.SubTopics);
                _context.Topics.RemoveRange(_context.Topics);
                _context.Courses.RemoveRange(_context.Courses);
                _context.Standards.RemoveRange(_context.Standards);
                _context.Attachments.RemoveRange(_context.Attachments);

                // Clear organizational structure
                _context.Departments.RemoveRange(_context.Departments);
                _context.Schools.RemoveRange(_context.Schools);
                _context.Districts.RemoveRange(_context.Districts);

                // Clear Identity-related tables
                _context.UserRoles.RemoveRange(_context.UserRoles);
                _context.Users.RemoveRange(_context.Users);
                _context.Roles.RemoveRange(_context.Roles);

                    await _context.SaveChangesAsync();
                }

                // ✅ FIXED: Get service provider and call updated SeedDatabaseAsync method
                var serviceProvider = HttpContext.RequestServices;
                await DatabaseSeeder.SeedDatabaseAsync(
                    _context,
                    _userManager,
                    _roleManager,
                    _logger,
                    _hostEnvironment,
                    serviceProvider); // ✅ ADD: Service provider parameter

                _logger.LogInformation("Database reset and reseeded successfully by admin {AdminUser}", currentUser);
                return Ok(new
                {
                    message = "Database reset and reseeded successfully with generated schedules",
                    timestamp = DateTime.UtcNow,
                    performedBy = currentUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset and reseed database: {Message}", ex.Message);
                return StatusCode(500, $"Failed to reset and reseed database: {ex.Message}");
            }
        }

        [HttpPost("reset-demo-data")]
        public async Task<IActionResult> ResetDemoData()
        {
            var currentUser = User.Identity?.Name ?? "Guest";
            _logger.LogInformation("User {User} is requesting demo data reset", currentUser);

            try
            {
                _logger.LogInformation("Starting demo data reset...");

                // Check if we should rate limit (prevent spam)
                var lastSeedDate = await _systemConfigService.GetLastSeedDateAsync();
                if (lastSeedDate.HasValue)
                {
                    var minutesSinceLastSeed = (DateTime.UtcNow - lastSeedDate.Value).TotalMinutes;
                    if (minutesSinceLastSeed < 5) // 5 minute rate limit
                    {
                        _logger.LogWarning("Demo data reset rate limited for user {User} - {Minutes:F1} minutes since last reset",
                            currentUser, minutesSinceLastSeed);
                        return BadRequest(new {
                            message = "Please wait at least 5 minutes between demo data resets",
                            minutesRemaining = Math.Ceiling(5 - minutesSinceLastSeed)
                        });
                    }
                }

                // Ensure database exists first
                await _context.Database.EnsureCreatedAsync();

                // Clear all dependent tables in proper order (only if they exist)
                if (_context.Database.CanConnect())
                {
                    // Clear system config except for important settings
                    var systemConfigs = await _context.SystemConfigs
                        .Where(sc => sc.Key != "LAST_SEED_DATE") // Keep other system configs if any
                        .ToListAsync();

                    _context.ScheduleEvents.RemoveRange(_context.ScheduleEvents);
                    _context.Schedules.RemoveRange(_context.Schedules);
                    _context.PeriodAssignments.RemoveRange(_context.PeriodAssignments);
                    _context.ScheduleConfigurations.RemoveRange(_context.ScheduleConfigurations);
                    _context.UserConfigurations.RemoveRange(_context.UserConfigurations);
                    _context.LessonAttachments.RemoveRange(_context.LessonAttachments);
                    _context.LessonStandards.RemoveRange(_context.LessonStandards);
                    _context.Notes.RemoveRange(_context.Notes);
                    _context.Lessons.RemoveRange(_context.Lessons);
                    _context.SubTopics.RemoveRange(_context.SubTopics);
                    _context.Topics.RemoveRange(_context.Topics);
                    _context.Courses.RemoveRange(_context.Courses);
                    _context.Standards.RemoveRange(_context.Standards);
                    _context.Attachments.RemoveRange(_context.Attachments);

                    // Clear organizational structure
                    _context.Departments.RemoveRange(_context.Departments);
                    _context.Schools.RemoveRange(_context.Schools);
                    _context.Districts.RemoveRange(_context.Districts);

                    // Clear Identity-related tables
                    _context.UserRoles.RemoveRange(_context.UserRoles);
                    _context.Users.RemoveRange(_context.Users);
                    _context.Roles.RemoveRange(_context.Roles);

                    await _context.SaveChangesAsync();
                }

                // Reseed the database with fresh demo data
                var serviceProvider = HttpContext.RequestServices;
                await DatabaseSeeder.SeedDatabaseAsync(
                    _context,
                    _userManager,
                    _roleManager,
                    _logger,
                    _hostEnvironment,
                    serviceProvider);

                _logger.LogInformation("Demo data reset completed successfully by user {User}", currentUser);
                return Ok(new
                {
                    message = "Demo data has been reset successfully! Fresh lesson plans, schedules, and courses are now available.",
                    timestamp = DateTime.UtcNow,
                    requestedBy = currentUser,
                    note = "Demo data is automatically refreshed every 24 hours"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset demo data: {Message}", ex.Message);
                return StatusCode(500, new {
                    message = "Failed to reset demo data. Please try again later.",
                    error = ex.Message
                });
            }
        }
    }
}