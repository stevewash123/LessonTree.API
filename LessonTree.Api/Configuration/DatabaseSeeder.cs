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

                logger.LogInformation("Seeding test data in Development mode...");

                // Clear existing data
                context.LessonAttachments.RemoveRange(context.LessonAttachments);
                context.LessonStandards.RemoveRange(context.LessonStandards); 
                context.Notes.RemoveRange(context.Notes);           // Added
                //context.ScheduleDays.RemoveRange(context.ScheduleDays); // Added
                //context.Schedules.RemoveRange(context.Schedules);   // Added
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

                // Seed Districts
                var districts = new List<District>
                {
                    new District { Name = "Central District", Description = "Lorem ipsum dolor sit amet." },
                    new District { Name = "Northern District", Description = "Consectetur adipiscing elit." }
                };
                context.Districts.AddRange(districts);
                await context.SaveChangesAsync();

                // Seed Schools
                var schools = new List<School>
                {
                    new School { Name = "Central High", Description = "Sed do eiusmod tempor.", DistrictId = districts[0].Id },
                    new School { Name = "North Middle", Description = "Ut labore et dolore magna.", DistrictId = districts[1].Id }
                };
                context.Schools.AddRange(schools);
                await context.SaveChangesAsync();

                // Seed Departments
                var departments = new List<Department>
                {
                    new Department { Name = "English", Description = "Literature and grammar.", SchoolId = schools[0].Id },
                    new Department { Name = "Math", Description = "Algebra and calculus.", SchoolId = schools[0].Id },
                    new Department { Name = "Science", Description = "Physics and chemistry.", SchoolId = schools[1].Id }
                };
                context.Departments.AddRange(departments);
                await context.SaveChangesAsync();

                // Seed Users with Roles
                var users = new List<(string Username, string Password, string FirstName, string LastName, string Role, int? DistrictId, int? SchoolId)>
                {
                    ("admin", "Admin123!", "Wilson", "Pickett", "admin", districts[0].Id, schools[0].Id),
                    ("paiduser", "Paid123!", "Jane", "Doe", "paidUser", districts[0].Id, schools[0].Id),
                    ("freeuser", "Free123!", "John", "Smith", "freeUser", districts[1].Id, schools[1].Id)
                };

                foreach (var (username, password, firstName, lastName, role, districtId, schoolId) in users)
                {
                    var user = await userManager.FindByNameAsync(username);
                    if (user == null)
                    {
                        logger.LogInformation("Creating user: {Username}", username);
                        user = new User
                        {
                            UserName = username,
                            FirstName = firstName,
                            LastName = lastName,
                            DistrictId = districtId,
                            SchoolId = schoolId
                        };
                        var result = await userManager.CreateAsync(user, password);
                        if (!result.Succeeded)
                        {
                            logger.LogError("Failed to create user {Username}: {Errors}", username, string.Join(", ", result.Errors.Select(e => e.Description)));
                            throw new Exception($"User {username} creation failed.");
                        }

                        result = await userManager.AddToRoleAsync(user, role);
                        if (!result.Succeeded)
                        {
                            logger.LogError("Failed to assign role {Role} to user {Username}: {Errors}", role, username, string.Join(", ", result.Errors.Select(e => e.Description)));
                            throw new Exception($"Role assignment failed for {username}.");
                        }

                        if (username == "admin" || username == "paiduser")
                            user.Departments.Add(departments[0]); // English
                        else
                            user.Departments.Add(departments[2]); // Science
                    }
                }
                await context.SaveChangesAsync();

                var adminUser = await userManager.FindByNameAsync("admin");
                var paidUser = await userManager.FindByNameAsync("paiduser");
                var freeUser = await userManager.FindByNameAsync("freeuser");

                // Seed Courses
                var courses = new List<Course>
                {
                    // Admin: Course 1 - Topic -> SubTopic -> Lesson (with some nulls)
                    new Course
                    {
                        Title = "Admin Hierarchical Course",
                        Description = null, // Null description
                        UserId = adminUser.Id,
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Admin Topic 1",
                                Description = "Core concepts",
                                UserId = adminUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Private,
                                SubTopics = new List<SubTopic>
                                {
                                    new SubTopic
                                    {
                                        Title = "SubTopic A",
                                        Description = null, // Null description
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson { Title = "Lesson 1", Objective = "Learn basics", Level = "10th", Materials = "Book", ClassTime = "45 min", Methods = "Lecture", Assessment = "Quiz", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                            new Lesson { Title = "Lesson 2", Objective = "Apply concepts", Level = "10th", Materials = null, ClassTime = "50 min", Methods = "Discussion", Assessment = "Essay", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                            new Lesson { Title = "Lesson 3", Objective = "Review", Level = "10th", Materials = "Notes", ClassTime = "40 min", Methods = null, Assessment = "Test", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private }
                                        }
                                    },
                                    new SubTopic
                                    {
                                        Title = "SubTopic B",
                                        Description = "Advanced topics",
                                        UserId = adminUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson { Title = "Lesson 4", Objective = "Deep dive", Level = "11th", Materials = "Text", ClassTime = "60 min", Methods = "Group work", Assessment = "Project", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                            new Lesson { Title = "Lesson 5", Objective = "Practice", Level = "11th", Materials = "Worksheets", ClassTime = "45 min", Methods = "Exercises", Assessment = "Quiz", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                            new Lesson { Title = "Lesson 6", Objective = "Summary", Level = "11th", Materials = null, ClassTime = "50 min", Methods = "Review", Assessment = "Test", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // Admin: Course 2 - Direct Lessons (with empty topic)
                    new Course
                    {
                        Title = "Admin Direct Lesson Course",
                        Description = "Direct lesson focus",
                        UserId = adminUser.Id,
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Admin Direct Topic",
                                Description = null, // Null description
                                UserId = adminUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Private,
                                Lessons = new List<Lesson>
                                {
                                    new Lesson { Title = "Direct Lesson 1", Objective = "Intro", Level = "9th", Materials = "Slides", ClassTime = "40 min", Methods = "Lecture", Assessment = "Quiz", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 2", Objective = "Practice", Level = "9th", Materials = "Book", ClassTime = "45 min", Methods = "Exercises", Assessment = "Test", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 3", Objective = "Apply", Level = "9th", Materials = null, ClassTime = "50 min", Methods = "Discussion", Assessment = "Essay", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 4", Objective = "Review", Level = "9th", Materials = "Notes", ClassTime = "40 min", Methods = "Review", Assessment = "Quiz", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 5", Objective = "Advanced", Level = "9th", Materials = "Text", ClassTime = "60 min", Methods = "Group work", Assessment = "Project", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 6", Objective = "Summary", Level = "9th", Materials = "Worksheets", ClassTime = "45 min", Methods = "Exercises", Assessment = "Test", UserId = adminUser.Id, Archived = false, Visibility = VisibilityType.Private }
                                }
                            },
                            new Topic
                            {
                                Title = "Empty Topic",
                                Description = "No lessons here",
                                UserId = adminUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Private,
                                Lessons = new List<Lesson>() // Empty array
                            }
                        }
                    },
                    // PaidUser: Course 1 - Topic -> SubTopic -> Lesson (Public)
                    new Course
                    {
                        Title = "Paid Hierarchical Course",
                        Description = "Public course with hierarchy",
                        UserId = paidUser.Id,
                        Archived = false,
                        Visibility = VisibilityType.Public, // Public visibility
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Paid Topic 1",
                                Description = "Core concepts",
                                UserId = paidUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Public,
                                SubTopics = new List<SubTopic>
                                {
                                    new SubTopic
                                    {
                                        Title = "SubTopic A",
                                        Description = "Basics",
                                        UserId = paidUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Public,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson { Title = "Lesson 1", Objective = "Learn basics", Level = "10th", Materials = "Book", ClassTime = "45 min", Methods = "Lecture", Assessment = "Quiz", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Public },
                                            new Lesson { Title = "Lesson 2", Objective = "Apply concepts", Level = "10th", Materials = "Slides", ClassTime = "50 min", Methods = "Discussion", Assessment = "Essay", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Public },
                                            new Lesson { Title = "Lesson 3", Objective = "Review", Level = "10th", Materials = "Notes", ClassTime = "40 min", Methods = "Review", Assessment = "Test", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Public }
                                        }
                                    },
                                    new SubTopic
                                    {
                                        Title = "SubTopic B",
                                        Description = "Advanced",
                                        UserId = paidUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Public,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson { Title = "Lesson 4", Objective = "Deep dive", Level = "11th", Materials = "Text", ClassTime = "60 min", Methods = "Group work", Assessment = "Project", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Public },
                                            new Lesson { Title = "Lesson 5", Objective = "Practice", Level = "11th", Materials = "Worksheets", ClassTime = "45 min", Methods = "Exercises", Assessment = "Quiz", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Public },
                                            new Lesson { Title = "Lesson 6", Objective = "Summary", Level = "11th", Materials = "Book", ClassTime = "50 min", Methods = "Review", Assessment = "Test", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Public }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // PaidUser: Course 2 - Direct Lessons
                    new Course
                    {
                        Title = "Paid Direct Lesson Course",
                        Description = "Direct lesson focus",
                        UserId = paidUser.Id,
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Paid Direct Topic",
                                Description = "Direct lessons",
                                UserId = paidUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Private,
                                Lessons = new List<Lesson>
                                {
                                    new Lesson { Title = "Direct Lesson 1", Objective = "Intro", Level = "9th", Materials = "Slides", ClassTime = "40 min", Methods = "Lecture", Assessment = "Quiz", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 2", Objective = "Practice", Level = "9th", Materials = "Book", ClassTime = "45 min", Methods = "Exercises", Assessment = "Test", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 3", Objective = "Apply", Level = "9th", Materials = "Notes", ClassTime = "50 min", Methods = "Discussion", Assessment = "Essay", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 4", Objective = "Review", Level = "9th", Materials = "Text", ClassTime = "40 min", Methods = "Review", Assessment = "Quiz", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 5", Objective = "Advanced", Level = "9th", Materials = "Worksheets", ClassTime = "60 min", Methods = "Group work", Assessment = "Project", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 6", Objective = "Summary", Level = "9th", Materials = "Book", ClassTime = "45 min", Methods = "Exercises", Assessment = "Test", UserId = paidUser.Id, Archived = false, Visibility = VisibilityType.Private }
                                }
                            }
                        }
                    },
                    // FreeUser: Course 1 - Topic -> SubTopic -> Lesson
                    new Course
                    {
                        Title = "Free Hierarchical Course",
                        Description = "Hierarchy course",
                        UserId = freeUser.Id,
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Free Topic 1",
                                Description = "Core concepts",
                                UserId = freeUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Private,
                                SubTopics = new List<SubTopic>
                                {
                                    new SubTopic
                                    {
                                        Title = "SubTopic A",
                                        Description = "Basics",
                                        UserId = freeUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson { Title = "Lesson 1", Objective = "Learn basics", Level = "10th", Materials = "Book", ClassTime = "45 min", Methods = "Lecture", Assessment = "Quiz", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                            new Lesson { Title = "Lesson 2", Objective = "Apply concepts", Level = "10th", Materials = "Slides", ClassTime = "50 min", Methods = "Discussion", Assessment = "Essay", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                            new Lesson { Title = "Lesson 3", Objective = "Review", Level = "10th", Materials = "Notes", ClassTime = "40 min", Methods = "Review", Assessment = "Test", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private }
                                        }
                                    },
                                    new SubTopic
                                    {
                                        Title = "SubTopic B",
                                        Description = "Advanced",
                                        UserId = freeUser.Id,
                                        Archived = false,
                                        Visibility = VisibilityType.Private,
                                        Lessons = new List<Lesson>
                                        {
                                            new Lesson { Title = "Lesson 4", Objective = "Deep dive", Level = "11th", Materials = "Text", ClassTime = "60 min", Methods = "Group work", Assessment = "Project", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                            new Lesson { Title = "Lesson 5", Objective = "Practice", Level = "11th", Materials = "Worksheets", ClassTime = "45 min", Methods = "Exercises", Assessment = "Quiz", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                            new Lesson { Title = "Lesson 6", Objective = "Summary", Level = "11th", Materials = "Book", ClassTime = "50 min", Methods = "Review", Assessment = "Test", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // FreeUser: Course 2 - Direct Lessons
                    new Course
                    {
                        Title = "Free Direct Lesson Course",
                        Description = "Direct lesson focus",
                        UserId = freeUser.Id,
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Title = "Free Direct Topic",
                                Description = "Direct lessons",
                                UserId = freeUser.Id,
                                Archived = false,
                                Visibility = VisibilityType.Private,
                                Lessons = new List<Lesson>
                                {
                                    new Lesson { Title = "Direct Lesson 1", Objective = "Intro", Level = "9th", Materials = "Slides", ClassTime = "40 min", Methods = "Lecture", Assessment = "Quiz", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 2", Objective = "Practice", Level = "9th", Materials = "Book", ClassTime = "45 min", Methods = "Exercises", Assessment = "Test", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 3", Objective = "Apply", Level = "9th", Materials = "Notes", ClassTime = "50 min", Methods = "Discussion", Assessment = "Essay", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 4", Objective = "Review", Level = "9th", Materials = "Text", ClassTime = "40 min", Methods = "Review", Assessment = "Quiz", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 5", Objective = "Advanced", Level = "9th", Materials = "Worksheets", ClassTime = "60 min", Methods = "Group work", Assessment = "Project", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private },
                                    new Lesson { Title = "Direct Lesson 6", Objective = "Summary", Level = "9th", Materials = "Book", ClassTime = "45 min", Methods = "Exercises", Assessment = "Test", UserId = freeUser.Id, Archived = false, Visibility = VisibilityType.Private }
                                }
                            }
                        }
                    }
                };

                context.Courses.AddRange(courses);
                await context.SaveChangesAsync();

                var standards = new List<Standard>();
                foreach (var course in courses)
                {
                    standards.AddRange(new List<Standard>
                    {
                        new Standard
                        {
                            Title = $"{course.Title} Standard 1",
                            Description = "Core standard",
                            CourseId = course.Id,
                            TopicId = course.Topics.FirstOrDefault()?.Id, // Link to first topic
                            DistrictId = course.User.DistrictId, // Link to user's district
                            StandardType = "State"
                        },
                        new Standard
                        {
                            Title = $"{course.Title} Standard 2",
                            Description = "Skill standard",
                            CourseId = course.Id,
                            TopicId = course.Topics.FirstOrDefault()?.Id, // Link to first topic
                            DistrictId = course.User.DistrictId,
                            StandardType = "National"
                        },
                        new Standard
                        {
                            Title = $"{course.Title} Standard 3",
                            Description = "Knowledge standard",
                            CourseId = course.Id,
                            TopicId = course.Topics.FirstOrDefault()?.Id, // Link to first topic
                            DistrictId = course.User.DistrictId,
                            StandardType = "State"
                        },
                        new Standard
                        {
                            Title = $"{course.Title} Standard 4",
                            Description = "Application standard",
                            CourseId = course.Id,
                            DistrictId = course.User.DistrictId,
                            StandardType = "National"
                        }
                    });
                }
                context.Standards.AddRange(standards);
                await context.SaveChangesAsync();

                // Link Standards to Lessons (example: link all 4 standards to the first lesson of each course)
                var lessonStandards = new List<LessonStandard>();
                foreach (var course in courses)
                {
                    var firstLesson = course.Topics.SelectMany(t => t.Lessons)
                        .Concat(course.Topics.SelectMany(t => t.SubTopics.SelectMany(st => st.Lessons)))
                        .First();
                    var courseStandards = standards.Where(s => s.CourseId == course.Id).ToList();
                    foreach (var standard in courseStandards)
                    {
                        lessonStandards.Add(new LessonStandard { LessonId = firstLesson.Id, StandardId = standard.Id });
                    }
                }
                context.LessonStandards.AddRange(lessonStandards);
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