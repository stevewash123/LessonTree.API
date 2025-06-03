// RESPONSIBILITY: Seeds test data for development environment with simplified course structure
// DOES NOT: Seed production data or complex hierarchies
// CALLED BY: Application startup configuration in development mode

using LessonTree.DAL;
using LessonTree.DAL.Domain;
using LessonTree.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

                logger.LogInformation("Seeding simplified test data in Development mode...");

                // Clear existing data
                context.LessonAttachments.RemoveRange(context.LessonAttachments);
                context.LessonStandards.RemoveRange(context.LessonStandards);
                context.Notes.RemoveRange(context.Notes);
                context.Lessons.RemoveRange(context.Lessons);
                context.SubTopics.RemoveRange(context.SubTopics);
                context.Topics.RemoveRange(context.Topics);
                context.Courses.RemoveRange(context.Courses);
                context.Standards.RemoveRange(context.Standards);
                context.Attachments.RemoveRange(context.Attachments);
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
                var district = new District { Name = "Test District", Description = "Test district for simplified data" };
                context.Districts.Add(district);
                await context.SaveChangesAsync();

                // Seed School
                var school = new School { Name = "Test School", Description = "Test school", DistrictId = district.Id };
                context.Schools.Add(school);
                await context.SaveChangesAsync();

                // Seed Department
                var department = new Department { Name = "Test Department", Description = "Test department", SchoolId = school.Id };
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
                        LastName = "Admin",
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

                // Seed 2 Courses with simplified structure
                var courses = new List<Course>
                {
                    new Course
                    {
                        Title = "Course1",
                        Description = "First test course",
                        UserId = adminUser.Id,
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Course1 Topic1",
                                Description = "First topic",
                                UserId = adminUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Private,
                                SortOrder = 0,
                                Lessons = new List<Lesson>
                                {
                                    new Lesson
                                    {
                                        Title = "Course1 Topic1 Lesson1",
                                        Objective = "Learn basics",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        SortOrder = 0
                                    }
                                },
                                SubTopics = new List<SubTopic>
                                {
                                    new SubTopic
                                    {
                                        Title = "Course1 Topic1 SubTopic1",
                                        Description = "First subtopic",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        SortOrder = 0,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson
                                            {
                                                Title = "Course1 Topic1 SubTopic1 Lesson1",
                                                Objective = "Learn subtopic basics",
                                                UserId = adminUser.Id,
                                                Archived = false,
                                                Visibility = VisibilityType.Private,
                                                SortOrder = 0
                                            },
                                            new Lesson
                                            {
                                                Title = "Course1 Topic1 SubTopic1 Lesson2",
                                                Objective = "Apply subtopic concepts",
                                                UserId = adminUser.Id,
                                                Archived = false,
                                                Visibility = VisibilityType.Private,
                                                SortOrder = 1
                                            }
                                        }
                                    }
                                }
                            },
                            new Topic
                            {
                                Title = "Course1 Topic2",
                                Description = "Second topic",
                                UserId = adminUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Private,
                                SortOrder = 1,
                                Lessons = new List<Lesson>
                                {
                                    new Lesson
                                    {
                                        Title = "Course1 Topic2 Lesson1",
                                        Objective = "Learn advanced",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        SortOrder = 0
                                    }
                                },
                                SubTopics = new List<SubTopic>
                                {
                                    new SubTopic
                                    {
                                        Title = "Course1 Topic2 SubTopic1",
                                        Description = "Second subtopic",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        SortOrder = 0,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson
                                            {
                                                Title = "Course1 Topic2 SubTopic1 Lesson1",
                                                Objective = "Master advanced concepts",
                                                UserId = adminUser.Id,
                                                Archived = false,
                                                Visibility = VisibilityType.Private,
                                                SortOrder = 0
                                            },
                                            new Lesson
                                            {
                                                Title = "Course1 Topic2 SubTopic1 Lesson2",
                                                Objective = "Review and assess",
                                                UserId = adminUser.Id,
                                                Archived = false,
                                                Visibility = VisibilityType.Private,
                                                SortOrder = 1
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Course
                    {
                        Title = "Course2",
                        Description = "Second test course",
                        UserId = adminUser.Id,
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Course2 Topic1",
                                Description = "First topic",
                                UserId = adminUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Private,
                                SortOrder = 0,
                                Lessons = new List<Lesson>
                                {
                                    new Lesson
                                    {
                                        Title = "Course2 Topic1 Lesson1",
                                        Objective = "Learn basics",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        SortOrder = 0
                                    }
                                },
                                SubTopics = new List<SubTopic>
                                {
                                    new SubTopic
                                    {
                                        Title = "Course2 Topic1 SubTopic1",
                                        Description = "First subtopic",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        SortOrder = 0,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson
                                            {
                                                Title = "Course2 Topic1 SubTopic1 Lesson1",
                                                Objective = "Learn subtopic basics",
                                                UserId = adminUser.Id,
                                                Archived = false,
                                                Visibility = VisibilityType.Private,
                                                SortOrder = 0
                                            },
                                            new Lesson
                                            {
                                                Title = "Course2 Topic1 SubTopic1 Lesson2",
                                                Objective = "Apply subtopic concepts",
                                                UserId = adminUser.Id,
                                                Archived = false,
                                                Visibility = VisibilityType.Private,
                                                SortOrder = 1
                                            }
                                        }
                                    }
                                }
                            },
                            new Topic
                            {
                                Title = "Course2 Topic2",
                                Description = "Second topic",
                                UserId = adminUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Private,
                                SortOrder = 1,
                                Lessons = new List<Lesson>
                                {
                                    new Lesson
                                    {
                                        Title = "Course2 Topic2 Lesson1",
                                        Objective = "Learn advanced",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        SortOrder = 0
                                    }
                                },
                                SubTopics = new List<SubTopic>
                                {
                                    new SubTopic
                                    {
                                        Title = "Course2 Topic2 SubTopic1",
                                        Description = "Second subtopic",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        SortOrder = 0,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson
                                            {
                                                Title = "Course2 Topic2 SubTopic1 Lesson1",
                                                Objective = "Master advanced concepts",
                                                UserId = adminUser.Id,
                                                Archived = false,
                                                Visibility = VisibilityType.Private,
                                                SortOrder = 0
                                            },
                                            new Lesson
                                            {
                                                Title = "Course2 Topic2 SubTopic1 Lesson2",
                                                Objective = "Review and assess",
                                                UserId = adminUser.Id,
                                                Archived = false,
                                                Visibility = VisibilityType.Private,
                                                SortOrder = 1
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                context.Courses.AddRange(courses);
                await context.SaveChangesAsync();

                logger.LogInformation("Simplified test data seeded successfully: 2 courses, 4 topics, 4 subtopics, 12 lessons total.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed simplified test data: {Message}", ex.Message);
                throw;
            }
        }
    }
}