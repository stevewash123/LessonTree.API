// **COMPLETE FILE** - Enhanced DatabaseSeeder with comprehensive test data
// RESPONSIBILITY: Seeds expanded test data with 2 courses, topics, subtopics, and lessons
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

                logger.LogInformation("Seeding comprehensive test data in Development mode...");

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

                // Seed 2 Courses with comprehensive structure
                var courses = new List<Course>();

                // ✅ FIXED: Separate counters for each entity type
                int globalTopicNumber = 1;
                int globalSubTopicNumber = 1;
                int globalLessonNumber = 1;

                for (int courseIndex = 1; courseIndex <= 2; courseIndex++)
                {
                    var course = new Course
                    {
                        Title = $"Course {courseIndex}",
                        Description = $"Test course {courseIndex} for comprehensive testing",
                        UserId = adminUser.Id,
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Topics = new List<Topic>()
                    };

                    // Create 2 topics per course
                    for (int topicIndex = 1; topicIndex <= 2; topicIndex++)
                    {
                        var topic = new Topic
                        {
                            Title = $"Course {courseIndex} Topic {globalTopicNumber}",
                            Description = $"Topic {globalTopicNumber} for Course {courseIndex}",
                            UserId = adminUser.Id,
                            Archived = false,
                            Visibility = VisibilityType.Private,
                            SortOrder = topicIndex - 1,
                            Lessons = new List<Lesson>(),
                            SubTopics = new List<SubTopic>()
                        };
                        globalTopicNumber++;

                        if (topicIndex == 1)
                        {
                            // First topic: 2 direct lessons + 2 subtopics (each with 2 lessons)
                            // ✅ FIXED: Sequential sort orders across ALL entities in Topic

                            int sortOrderCounter = 0;

                            // Add 2 direct lessons to the topic FIRST
                            for (int directLessonIndex = 1; directLessonIndex <= 2; directLessonIndex++)
                            {
                                var lesson = new Lesson
                                {
                                    Title = $"Course {courseIndex} Topic {globalTopicNumber - 1} Lesson {globalLessonNumber}",
                                    Objective = $"Learn lesson {globalLessonNumber} concepts in topic {globalTopicNumber - 1}",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = sortOrderCounter++  // 0, 1
                                };
                                topic.Lessons.Add(lesson);
                                globalLessonNumber++;
                            }

                            // Add 2 subtopics AFTER the direct lessons
                            for (int subTopicIndex = 1; subTopicIndex <= 2; subTopicIndex++)
                            {
                                var subTopic = new SubTopic
                                {
                                    Title = $"Course {courseIndex} Topic {globalTopicNumber - 1} SubTopic {globalSubTopicNumber}",
                                    Description = $"SubTopic {globalSubTopicNumber} under Topic {globalTopicNumber - 1}",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = sortOrderCounter++,  // 2, 3
                                    Lessons = new List<Lesson>()
                                };
                                globalSubTopicNumber++;

                                // Add 2 lessons to each subtopic
                                for (int subLessonIndex = 1; subLessonIndex <= 2; subLessonIndex++)
                                {
                                    var lesson = new Lesson
                                    {
                                        Title = $"Course {courseIndex} Topic {globalTopicNumber - 1} SubTopic {globalSubTopicNumber - 1} Lesson {globalLessonNumber}",
                                        Objective = $"Learn lesson {globalLessonNumber} in subtopic {globalSubTopicNumber - 1}",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        SortOrder = subLessonIndex - 1  // Within SubTopic: 0, 1
                                    };
                                    subTopic.Lessons.Add(lesson);
                                    globalLessonNumber++;
                                }

                                topic.SubTopics.Add(subTopic);
                            }
                        }
                        else
                        {
                            // Second topic: just 2 direct lessons (simpler structure)
                            for (int directLessonIndex = 1; directLessonIndex <= 2; directLessonIndex++)
                            {
                                var lesson = new Lesson
                                {
                                    Title = $"Course {courseIndex} Topic {globalTopicNumber - 1} Lesson {globalLessonNumber}",
                                    Objective = $"Learn lesson {globalLessonNumber} concepts in topic {globalTopicNumber - 1}",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = directLessonIndex - 1  // 0, 1
                                };
                                topic.Lessons.Add(lesson);
                                globalLessonNumber++;
                            }
                        }

                        course.Topics.Add(topic);
                    }

                    courses.Add(course);
                }

                context.Courses.AddRange(courses);
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

                logger.LogInformation("Comprehensive test data seeded successfully:");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed comprehensive test data: {Message}", ex.Message);
                throw;
            }
        }
    }
}