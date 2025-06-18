// **COMPLETE FILE** - Conservative DatabaseSeeder for basic testing
// RESPONSIBILITY: Seeds minimal test data that should compile
// DOES NOT: Seed complex schedule configurations yet
// CALLED BY: Application startup configuration in development mode

using LessonTree.DAL;
using LessonTree.DAL.Domain;
using LessonTree.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LessonTree.API.Configuration
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabaseAsync(
            LessonTreeContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger logger,
            IHostEnvironment env)
        {
            try
            {
                if (!env.IsDevelopment())
                {
                    logger.LogInformation("Skipping test data seeding: not in Development mode.");
                    return;
                }

                logger.LogInformation("Seeding basic test data in Development mode...");

                // Clear existing data in dependency order - keep it simple
                context.ScheduleEvents.RemoveRange(context.ScheduleEvents);
                context.Schedules.RemoveRange(context.Schedules);
                context.LessonAttachments.RemoveRange(context.LessonAttachments);
                context.LessonStandards.RemoveRange(context.LessonStandards);
                context.Notes.RemoveRange(context.Notes);
                context.Lessons.RemoveRange(context.Lessons);
                context.SubTopics.RemoveRange(context.SubTopics);
                context.Topics.RemoveRange(context.Topics);
                context.Courses.RemoveRange(context.Courses);
                context.Standards.RemoveRange(context.Standards);
                context.Attachments.RemoveRange(context.Attachments);
                context.UserConfigurations.RemoveRange(context.UserConfigurations);
                context.Departments.RemoveRange(context.Departments);
                context.Schools.RemoveRange(context.Schools);
                context.Districts.RemoveRange(context.Districts);
                await context.SaveChangesAsync();

                // Seed Roles
                string[] roleNames = { "admin", "paidUser", "freeUser" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        logger.LogInformation("Creating role: {RoleName}", roleName);
                        var role = new IdentityRole<int> { Name = roleName };
                        var result = await roleManager.CreateAsync(role);
                        if (!result.Succeeded)
                        {
                            logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                            throw new Exception($"Role {roleName} creation failed.");
                        }
                    }
                }

                // Seed District
                var district = new District { Name = "Test District", Description = "Test district" };
                context.Districts.Add(district);
                await context.SaveChangesAsync();

                // Seed School
                var school = new School { Name = "Test School", Description = "Test school", DistrictId = district.Id };
                context.Schools.Add(school);
                await context.SaveChangesAsync();

                // Seed Department
                var department = new Department { Name = "Mathematics", Description = "Math department", SchoolId = school.Id };
                context.Departments.Add(department);
                await context.SaveChangesAsync();

                // Seed Admin User
                var adminUser = await userManager.FindByNameAsync("admin");
                if (adminUser == null)
                {
                    logger.LogInformation("Creating admin user");
                    adminUser = new User
                    {
                        UserName = "admin",
                        FirstName = "Test",
                        LastName = "Teacher",
                        DistrictId = district.Id,
                        SchoolId = school.Id
                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (!result.Succeeded)
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                        throw new Exception("Admin user creation failed.");
                    }

                    result = await userManager.AddToRoleAsync(adminUser, "admin");
                    if (!result.Succeeded)
                    {
                        logger.LogError("Failed to assign admin role: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                        throw new Exception("Role assignment failed.");
                    }

                    adminUser.Departments.Add(department);
                    await context.SaveChangesAsync();
                }

                // Seed 1 Simple Course for testing
                var mathCourse = new Course
                {
                    Title = "Algebra I",
                    Description = "First year algebra course",
                    UserId = adminUser.Id,
                    Archived = false,
                    Visibility = VisibilityType.Private,
                    Topics = new List<Topic>
                    {
                        new Topic
                        {
                            Title = "Linear Equations",
                            Description = "Solving linear equations",
                            UserId = adminUser.Id,
                            Archived = false,
                            Visibility = VisibilityType.Private,
                            SortOrder = 0,
                            Lessons = new List<Lesson>
                            {
                                new Lesson
                                {
                                    Title = "Introduction to Linear Equations",
                                    Objective = "Understand what makes an equation linear",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 0
                                },
                                new Lesson
                                {
                                    Title = "Solving One-Step Equations",
                                    Objective = "Solve equations using addition and subtraction",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 1
                                }
                            }
                        }
                    }
                };

                context.Courses.Add(mathCourse);
                await context.SaveChangesAsync();

                // SEED MINIMAL USER CONFIGURATION (Profile data only)
                var userConfig = new UserConfiguration
                {
                    UserId = adminUser.Id,
                    SettingsJson = "{\"theme\":\"light\"}",
                    LastUpdated = DateTime.UtcNow
                };

                context.UserConfigurations.Add(userConfig);
                await context.SaveChangesAsync();

                logger.LogInformation("Basic test data seeded successfully:");
                logger.LogInformation("- 1 course: Algebra I with 2 lessons");
                logger.LogInformation("- Admin user created");
                logger.LogInformation("- Basic user configuration");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed basic test data: {Message}", ex.Message);
                throw;
            }
        }
    }
}