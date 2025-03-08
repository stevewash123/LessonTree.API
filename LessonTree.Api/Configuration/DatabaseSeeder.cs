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
        public static async Task SeedDatabaseAsync(LessonTreeContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, ILogger logger, IHostEnvironment env)
        {
            try
            {
                logger.LogInformation("Applying migrations to LessonTree.db...");
                context.Database.Migrate();

                // Seed Roles
                string[] roles = { "FreeUser", "PaidUser", "Admin" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        logger.LogInformation("Creating role: {Role}", role);
                        await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                    }
                }

                // Seed Admin User
                string adminUsername = "admin";
                string adminPassword = "Admin123!";
                var adminUser = await userManager.FindByNameAsync(adminUsername);
                if (adminUser == null)
                {
                    logger.LogInformation("Creating admin user: {Username}", adminUsername);
                    adminUser = new User { UserName = adminUsername };
                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Initial Admin user created: {Username}", adminUsername);
                    }
                    else
                    {
                        logger.LogError("Failed to create initial Admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // Seed Test Data in Development Mode
                if (env.IsDevelopment() && !context.Courses.Any())
                {
                    logger.LogInformation("Seeding test data for Courses, Topics, SubTopics, Lessons, Standards, and Documents in Development mode...");

                    var courses = new List<Course>
                    {
                        new Course
                        {
                            Title = "High School English",
                            Description = "A comprehensive English course for high school students.",
                            Topics = new List<Topic>
                            {
                                new Topic
                                {
                                    Title = "Literature",
                                    Description = "Exploring classic and modern literary works.",
                                    SubTopics = new List<SubTopic>
                                    {
                                        new SubTopic
                                        {
                                            Title = "Shakespeare",
                                            Description = "Study of Shakespeare's plays and sonnets.",
                                            Lessons = new List<Lesson>
                                            {
                                                new Lesson
                                                {
                                                    Title = "Introduction to Shakespeare",
                                                    Content = "Overview of Shakespeare's life and works.",
                                                    LastDateTaught = DateTime.Now.AddYears(-1),
                                                    Level = "9th Grade",
                                                    Objective = null, // Null for boundary testing
                                                    Materials = null, // Null for boundary testing
                                                    ClassTime = "45 minutes",
                                                    Methods = "Lecture, discussion",
                                                    SpecialNeeds = "Visual aids for hearing impaired",
                                                    Assessment = "Quiz"
                                                },
                                                new Lesson
                                                {
                                                    Title = "Romeo and Juliet",
                                                    Content = null, // Null for boundary testing
                                                    LastDateTaught = DateTime.Now.AddMonths(-6),
                                                    Level = "10th Grade",
                                                    Objective = "Analyze themes in Romeo and Juliet.",
                                                    Materials = "Play script",
                                                    ClassTime = "60 minutes",
                                                    Methods = "Reading, group work",
                                                    SpecialNeeds = null, // Null for boundary testing
                                                    Assessment = "Essay"
                                                }
                                            }
                                        },
                                        new SubTopic
                                        {
                                            Title = "American Literature",
                                            Description = "Key works from American authors.",
                                            Lessons = new List<Lesson>() // Empty Lessons array
                                        },
                                        new SubTopic
                                        {
                                            Title = "Poetry",
                                            Description = "Study of poetic forms and techniques.",
                                            Lessons = new List<Lesson>() // Empty Lessons array
                                        }
                                    }
                                },
                                new Topic
                                {
                                    Title = "Grammar",
                                    Description = "Mastering English grammar rules.",
                                    SubTopics = new List<SubTopic>() // Empty SubTopics array
                                },
                                new Topic
                                {
                                    Title = "Writing",
                                    Description = "Developing writing skills.",
                                    SubTopics = new List<SubTopic>() // Empty SubTopics array
                                }
                            }
                        },
                        new Course
                        {
                            Title = "High School Science",
                            Description = "A broad science course for high school students.",
                            Topics = new List<Topic>
                            {
                                new Topic
                                {
                                    Title = "Biology",
                                    Description = "Study of living organisms.",
                                    SubTopics = new List<SubTopic>() // Empty SubTopics array
                                },
                                new Topic
                                {
                                    Title = "Chemistry",
                                    Description = "Fundamentals of matter and reactions.",
                                    SubTopics = new List<SubTopic>() // Empty SubTopics array
                                }
                            }
                        },
                        new Course
                        {
                            Title = "High School Math",
                            Description = "Mathematics course with no topics yet.",
                            Topics = new List<Topic>() // Empty Topics array
                        }
                    };
                    context.Courses.AddRange(courses);
                    await context.SaveChangesAsync();

                    // Seed Standards
                    var literatureTopic = courses[0].Topics.First(t => t.Title == "Literature");
                    var biologyTopic = courses[1].Topics.First(t => t.Title == "Biology");

                    var standards = new List<Standard>
                    {
                        new Standard
                        {
                            Title = "CCSS.ELA-LITERACY.RL.9-10.1",
                            Description = "Cite strong and thorough textual evidence to support analysis of what the text says explicitly as well as inferences drawn from the text.",
                            StandardType = "Literature",
                            TopicId = literatureTopic.Id
                        },
                        new Standard
                        {
                            Title = "CCSS.ELA-LITERACY.RL.9-10.2",
                            Description = "Determine a theme or central idea of a text and analyze in detail its development over the course of the text, including how it emerges and is shaped and refined by specific details; provide an objective summary of the text.",
                            StandardType = "Literature",
                            TopicId = literatureTopic.Id
                        },
                        new Standard
                        {
                            Title = "NGSS.HS-LS1-1",
                            Description = "Construct an explanation based on evidence for how the structure of DNA determines the structure of proteins which carry out the essential functions of life through systems of specialized cells.",
                            StandardType = "Biology",
                            TopicId = biologyTopic.Id
                        }
                    };
                    context.Standards.AddRange(standards);
                    await context.SaveChangesAsync();

                    // Seed Documents
                    var documents = new List<Document>
                    {
                        new Document { FileName = "Lesson Plan Template.docx", ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Blob = null },
                        new Document { FileName = "Worksheet.pdf", ContentType = "application/pdf", Blob = null },
                        new Document { FileName = "Presentation.pptx", ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation", Blob = null }
                    };
                    context.Documents.AddRange(documents);
                    await context.SaveChangesAsync();

                    // Link Standards and Documents to Lessons
                    var lessonIntroShakespeare = literatureTopic.SubTopics[0].Lessons[0];
                    var lessonRomeoJuliet = literatureTopic.SubTopics[0].Lessons[1];

                    var lessonStandards = new List<LessonStandard>
                    {
                        new LessonStandard { LessonId = lessonIntroShakespeare.Id, StandardId = standards[0].Id },
                        new LessonStandard { LessonId = lessonRomeoJuliet.Id, StandardId = standards[0].Id },
                        new LessonStandard { LessonId = lessonRomeoJuliet.Id, StandardId = standards[1].Id }
                    };
                    context.LessonStandards.AddRange(lessonStandards);

                    var lessonDocuments = new List<LessonDocument>
                    {
                        new LessonDocument { LessonId = lessonIntroShakespeare.Id, DocumentId = documents[0].Id },
                        new LessonDocument { LessonId = lessonRomeoJuliet.Id, DocumentId = documents[1].Id }
                    };
                    context.LessonDocuments.AddRange(lessonDocuments);

                    await context.SaveChangesAsync();

                    logger.LogInformation("Test data seeded successfully in Development mode.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to migrate database or seed data: {Message}", ex.Message);
                throw;
            }
        }
    }
}