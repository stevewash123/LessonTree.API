// Full File
using LessonTree.DAL;
using LessonTree.DAL.Domain;
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
        public static async Task SeedDatabaseAsync(LessonTreeContext context, UserManager<User> userManager, ILogger logger, IHostEnvironment env)
        {
            try
            {
                if (!env.IsDevelopment())
                {
                    logger.LogInformation("Skipping test data seeding: not in Development mode.");
                    return;
                }

                logger.LogInformation("Seeding test data for Courses, Topics, SubTopics, and Lessons in Development mode...");

                // Clear existing data for a clean slate
                context.LessonAttachments.RemoveRange(context.LessonAttachments);
                context.LessonStandards.RemoveRange(context.LessonStandards);
                context.Lessons.RemoveRange(context.Lessons);
                context.SubTopics.RemoveRange(context.SubTopics);
                context.Topics.RemoveRange(context.Topics);
                context.Courses.RemoveRange(context.Courses);
                context.Standards.RemoveRange(context.Standards);
                context.Attachments.RemoveRange(context.Attachments);
                await context.SaveChangesAsync();

                // Seed Admin User (needed for ownership)
                string adminUsername = "admin";
                string adminPassword = "Admin123!";
                var adminUser = await userManager.FindByNameAsync(adminUsername);
                if (adminUser == null)
                {
                    logger.LogInformation("Creating admin user: {Username}", adminUsername);
                    adminUser = new User { UserName = adminUsername };
                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (!result.Succeeded)
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                        throw new Exception("Admin user creation failed.");
                    }
                }

                // Seed Non-Admin User (for ownership testing)
                string testUserUsername = "testuser";
                string testUserPassword = "Test123!";
                var testUser = await userManager.FindByNameAsync(testUserUsername);
                if (testUser == null)
                {
                    logger.LogInformation("Creating test user: {Username}", testUserUsername);
                    testUser = new User { UserName = testUserUsername };
                    var result = await userManager.CreateAsync(testUser, testUserPassword);
                    if (!result.Succeeded)
                    {
                        logger.LogError("Failed to create test user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                        throw new Exception("Test user creation failed.");
                    }
                }

                // Seed Test Courses
                var courses = new List<Course>
                {
                    // Course 1: Nulls scattered about (owned by admin)
                    new Course
                    {
                        Title = "Course with Nulls",
                        Description = null, // Null description
                        UserId = adminUser.Id,
                        Archived = false,
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Topic with Nulls",
                                Description = null, // Null description
                                UserId = adminUser.Id,
                                Archived = false,
                                SubTopics = new List<SubTopic>
                                {
                                    new SubTopic
                                    {
                                        Title = "SubTopic with Nulls",
                                        Description = null, // Null description
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson
                                            {
                                                Title = "Lesson with Nulls",
                                                Level = null, // Null level
                                                Objective = "Test null handling",
                                                Materials = null, // Null materials
                                                ClassTime = "45 minutes",
                                                Methods = null, // Null methods
                                                SpecialNeeds = null, // Null special needs
                                                Assessment = "Quiz",
                                                UserId = adminUser.Id
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },

                    // Course 2: Not owned by admin (testuser-owned, no subtopics)
                    new Course
                    {
                        Title = "TestUser Course",
                        Description = "Course owned by testuser, no subtopics.",
                        UserId = testUser.Id,
                        Archived = false,
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Direct Lessons",
                                Description = "Topic with lessons but no subtopics.",
                                UserId = testUser.Id,
                                Archived = false,
                                Lessons = new List<Lesson>
                                {
                                    new Lesson
                                    {
                                        Title = "Basic Lesson",
                                        Level = "9th Grade",
                                        Objective = "Learn basics.",
                                        Materials = "Textbook",
                                        ClassTime = "40 minutes",
                                        Methods = "Lecture",
                                        SpecialNeeds = null,
                                        Assessment = "Quiz",
                                        UserId = testUser.Id
                                    }
                                }
                            }
                        }
                    },

                    // Course 3: Admin-owned with subtopics
                    new Course
                    {
                        Title = "Admin Course with SubTopics",
                        Description = "Admin-owned course with subtopics.",
                        UserId = adminUser.Id,
                        Archived = false,
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Literature",
                                Description = "Exploring literary works.",
                                UserId = adminUser.Id,
                                Archived = false,
                                SubTopics = new List<SubTopic>
                                {
                                    new SubTopic
                                    {
                                        Title = "Shakespeare",
                                        Description = "Study of Shakespeare's works.",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson
                                            {
                                                Title = "Hamlet",
                                                Level = "11th Grade",
                                                Objective = "Analyze Hamlet.",
                                                Materials = "Play text",
                                                ClassTime = "60 minutes",
                                                Methods = "Discussion",
                                                SpecialNeeds = "Audio version",
                                                Assessment = "Essay",
                                                UserId = adminUser.Id
                                            }
                                        }
                                    },
                                    new SubTopic
                                    {
                                        Title = "Poetry",
                                        Description = "Study of poetic forms.",
                                        UserId = adminUser.Id,
                                        Archived = true, // Archived subtopic for testing
                                        Lessons = new List<Lesson>()
                                    }
                                }
                            }
                        }
                    },

                    // Course 4: Admin-owned without subtopics
                    new Course
                    {
                        Title = "Admin Course without SubTopics",
                        Description = "Admin-owned course with direct lessons.",
                        UserId = adminUser.Id,
                        Archived = true, // Archived course for testing
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Grammar",
                                Description = "Mastering grammar rules.",
                                UserId = adminUser.Id,
                                Archived = false,
                                Lessons = new List<Lesson>
                                {
                                    new Lesson
                                    {
                                        Title = "Parts of Speech",
                                        Level = "9th Grade",
                                        Objective = "Identify parts of speech.",
                                        Materials = "Workbook",
                                        ClassTime = "30 minutes",
                                        Methods = "Exercises",
                                        SpecialNeeds = "Large print",
                                        Assessment = "Test",
                                        UserId = adminUser.Id
                                    }
                                }
                            }
                        }
                    }
                };

                context.Courses.AddRange(courses);
                await context.SaveChangesAsync();

                logger.LogInformation("Test data seeded successfully in Development mode.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed test data: {Message}", ex.Message);
                throw;
            }
        }
    }
}