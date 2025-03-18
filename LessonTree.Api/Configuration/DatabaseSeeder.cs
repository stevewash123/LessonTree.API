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

                // Seed Test Data in Development Mode (always seed, clearing existing data)
                if (env.IsDevelopment())
                {
                    logger.LogInformation("Seeding test data for Courses, Topics, SubTopics, Lessons, Standards, and Attachments in Development mode...");

                    // Clear existing data to ensure a clean state
                    context.LessonAttachments.RemoveRange(context.LessonAttachments);
                    context.LessonStandards.RemoveRange(context.LessonStandards);
                    context.Lessons.RemoveRange(context.Lessons);
                    context.SubTopics.RemoveRange(context.SubTopics);
                    context.Topics.RemoveRange(context.Topics);
                    context.Courses.RemoveRange(context.Courses);
                    context.Standards.RemoveRange(context.Standards);
                    context.Attachments.RemoveRange(context.Attachments);
                    await context.SaveChangesAsync();

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
                                    HasSubTopics = true, // Multiple subtopics
                                    SubTopics = new List<SubTopic>
                                    {
                                        new SubTopic
                                        {
                                            Title = "Shakespeare",
                                            Description = "Study of Shakespeare's plays and sonnets.",
                                            IsDefault = true, // Default subtopic with no lessons
                                            Lessons = new List<Lesson>() // Empty lessons
                                        },
                                        new SubTopic
                                        {
                                            Title = "American Literature",
                                            Description = "Key works from American authors.",
                                            IsDefault = false,
                                            Lessons = new List<Lesson>
                                            {
                                                new Lesson
                                                {
                                                    Title = "The Great Gatsby",
                                                    Level = "11th Grade",
                                                    Objective = "Understand themes and symbolism.",
                                                    Materials = "Novel, study guide",
                                                    ClassTime = "60 minutes",
                                                    Methods = "Lecture, discussion",
                                                    SpecialNeeds = "Audio version for visually impaired",
                                                    Assessment = "Essay"
                                                },
                                                new Lesson
                                                {
                                                    Title = "To Kill a Mockingbird",
                                                    Level = "10th Grade",
                                                    Objective = "Analyze character development.",
                                                    Materials = "Novel",
                                                    ClassTime = "45 minutes",
                                                    Methods = "Group reading",
                                                    SpecialNeeds = null, // Boundary data: null special needs
                                                    Assessment = "Quiz"
                                                }
                                            }
                                        },
                                        new SubTopic
                                        {
                                            Title = "Poetry",
                                            Description = "Study of poetic forms and techniques.",
                                            IsDefault = false,
                                            Lessons = new List<Lesson>() // Empty lessons
                                        }
                                    }
                                },
                                new Topic
                                {
                                    Title = "Grammar",
                                    Description = "Mastering English grammar rules.",
                                    HasSubTopics = false, // Only default subtopic
                                    SubTopics = new List<SubTopic>
                                    {
                                        new SubTopic
                                        {
                                            Title = "Default SubTopic",
                                            IsDefault = true,
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
                                                    SpecialNeeds = "Large print materials",
                                                    Assessment = "Test"
                                                },
                                                new Lesson
                                                {
                                                    Title = "Sentence Structure",
                                                    Level = "10th Grade",
                                                    Objective = "Identify parts of sentance.",
                                                    Materials = null, // Boundary data: null materials
                                                    ClassTime = "40 minutes",
                                                    Methods = "Lecture",
                                                    SpecialNeeds = "None",
                                                    Assessment = "Worksheet"
                                                }
                                            }
                                        }
                                    }
                                },
                                new Topic
                                {
                                    Title = "Writing",
                                    Description = "Developing writing skills.",
                                    HasSubTopics = true, // Changed from false to true to match test expectations
                                    SubTopics = new List<SubTopic>
                                    {
                                        new SubTopic
                                        {
                                            Title = "Default SubTopic",
                                            IsDefault = true,
                                            Lessons = new List<Lesson>
                                            {
                                                new Lesson
                                                {
                                                    Title = "Essay Writing",
                                                    Level = "11th Grade",
                                                    Objective = "Write a coherent essay.",
                                                    Materials = "Writing prompts",
                                                    ClassTime = "50 minutes",
                                                    Methods = "Workshop",
                                                    SpecialNeeds = "Extended time for dyslexic students",
                                                    Assessment = "Peer review"
                                                },
                                                new Lesson
                                                {
                                                    Title = "Creative Writing",
                                                    Level = "12th Grade",
                                                    Objective = "Develop a short story.",
                                                    Materials = "Examples of short stories",
                                                    ClassTime = "60 minutes",
                                                    Methods = "Writing exercises",
                                                    SpecialNeeds = null, // Boundary data: null special needs
                                                    Assessment = "Portfolio"
                                                }
                                            }
                                        }
                                    }
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
                                    HasSubTopics = false, // Only default subtopic
                                    SubTopics = new List<SubTopic>
                                    {
                                        new SubTopic
                                        {
                                            Title = "Default SubTopic",
                                            IsDefault = true,
                                            Lessons = new List<Lesson>() // No lessons
                                        }
                                    }
                                },
                                new Topic
                                {
                                    Title = "Chemistry",
                                    Description = "Fundamentals of matter and reactions.",
                                    HasSubTopics = false, // Only default subtopic
                                    SubTopics = new List<SubTopic>
                                    {
                                        new SubTopic
                                        {
                                            Title = "Default SubTopic",
                                            IsDefault = true,
                                            Lessons = new List<Lesson>() // No lessons
                                        }
                                    }
                                }
                            }
                        },
                        new Course
                        {
                            Title = "High School Math",
                            Description = "Mathematics course with no topics yet.",
                            Topics = new List<Topic>() // Empty topics array
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

                    // Seed Attachments
                    var attachments = new List<Attachment>
                    {
                        new Attachment { FileName = "Lesson Plan Template.docx", ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Blob = [] },
                        new Attachment { FileName = "Worksheet.pdf", ContentType = "application/pdf", Blob = [] },
                        new Attachment { FileName = "Presentation.pptx", ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation", Blob = [] }
                    };
                    context.Attachments.AddRange(attachments);
                    await context.SaveChangesAsync();

                    // Link Standards and Documents to Lessons
                    var americanLitSubTopic = literatureTopic.SubTopics.First(st => st.Title == "American Literature");
                    var lessonGreatGatsby = americanLitSubTopic.Lessons.First(l => l.Title == "The Great Gatsby");
                    var lessonMockingbird = americanLitSubTopic.Lessons.First(l => l.Title == "To Kill a Mockingbird");

                    var lessonStandards = new List<LessonStandard>
                    {
                        new LessonStandard { LessonId = lessonGreatGatsby.Id, StandardId = standards[0].Id },
                        new LessonStandard { LessonId = lessonMockingbird.Id, StandardId = standards[1].Id }
                    };
                    context.LessonStandards.AddRange(lessonStandards);

                    var lessonAttachments = new List<LessonAttachment>
                    {
                        new LessonAttachment { LessonId = lessonGreatGatsby.Id, AttachmentId = attachments[0].Id },
                        new LessonAttachment { LessonId = lessonMockingbird.Id, AttachmentId = attachments[1].Id }
                    };
                    context.LessonAttachments.AddRange(lessonAttachments);

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