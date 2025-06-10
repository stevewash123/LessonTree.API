// **COMPLETE FILE** - Updated DatabaseSeeder for Master Schedule Testing
// RESPONSIBILITY: Seeds test data for development environment with period assignments and master schedule structure
// DOES NOT: Seed production data or complex hierarchies
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

                logger.LogInformation("Seeding master schedule test data in Development mode...");

                // Clear existing data in dependency order
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
                var district = new District { Name = "Test District", Description = "Test district for master schedule testing" };
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

                // Seed 2 Courses - Math and Science
                var courses = new List<Course>();

                // MATH COURSE: 2 topics, 1 subtopic each, 8 lessons total (4 on topic, 4 on subtopic per topic)
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
                            Description = "Solving linear equations and inequalities",
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
                                },
                                new Lesson
                                {
                                    Title = "Solving Two-Step Equations",
                                    Objective = "Solve equations requiring multiple operations",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 2
                                },
                                new Lesson
                                {
                                    Title = "Linear Equations Review",
                                    Objective = "Review and practice linear equation concepts",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 3
                                }
                            },
                            SubTopics = new List<SubTopic>
                            {
                                new SubTopic
                                {
                                    Title = "Linear Inequalities",
                                    Description = "Solving and graphing linear inequalities",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 0,
                                    Lessons = new List<Lesson>
                                    {
                                        new Lesson
                                        {
                                            Title = "Introduction to Inequalities",
                                            Objective = "Understand inequality symbols and concepts",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 0
                                        },
                                        new Lesson
                                        {
                                            Title = "Solving Linear Inequalities",
                                            Objective = "Solve one-variable linear inequalities",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 1
                                        },
                                        new Lesson
                                        {
                                            Title = "Graphing Inequalities",
                                            Objective = "Graph solutions on number lines",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 2
                                        },
                                        new Lesson
                                        {
                                            Title = "Compound Inequalities",
                                            Objective = "Solve and graph compound inequalities",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 3
                                        }
                                    }
                                }
                            }
                        },
                        new Topic
                        {
                            Title = "Systems of Equations",
                            Description = "Solving systems using multiple methods",
                            UserId = adminUser.Id,
                            Archived = false,
                            Visibility = VisibilityType.Private,
                            SortOrder = 1,
                            Lessons = new List<Lesson>
                            {
                                new Lesson
                                {
                                    Title = "Introduction to Systems",
                                    Objective = "Understand what systems of equations represent",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 0
                                },
                                new Lesson
                                {
                                    Title = "Graphing Method",
                                    Objective = "Solve systems by graphing both lines",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 1
                                },
                                new Lesson
                                {
                                    Title = "Substitution Method",
                                    Objective = "Solve systems using substitution",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 2
                                },
                                new Lesson
                                {
                                    Title = "Elimination Method",
                                    Objective = "Solve systems using elimination",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 3
                                }
                            },
                            SubTopics = new List<SubTopic>
                            {
                                new SubTopic
                                {
                                    Title = "Applications of Systems",
                                    Description = "Real-world problems using systems",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 0,
                                    Lessons = new List<Lesson>
                                    {
                                        new Lesson
                                        {
                                            Title = "Mixture Problems",
                                            Objective = "Solve mixture problems using systems",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 0
                                        },
                                        new Lesson
                                        {
                                            Title = "Distance Problems",
                                            Objective = "Solve distance-rate-time problems",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 1
                                        },
                                        new Lesson
                                        {
                                            Title = "Age Problems",
                                            Objective = "Solve age-related word problems",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 2
                                        },
                                        new Lesson
                                        {
                                            Title = "Systems Applications Test",
                                            Objective = "Assessment of systems applications",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 3
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                // SCIENCE COURSE: 2 topics, 1 subtopic each, 8 lessons total
                var scienceCourse = new Course
                {
                    Title = "Biology I",
                    Description = "Introduction to biological sciences",
                    UserId = adminUser.Id,
                    Archived = false,
                    Visibility = VisibilityType.Private,
                    Topics = new List<Topic>
                    {
                        new Topic
                        {
                            Title = "Cell Structure",
                            Description = "Structure and function of cells",
                            UserId = adminUser.Id,
                            Archived = false,
                            Visibility = VisibilityType.Private,
                            SortOrder = 0,
                            Lessons = new List<Lesson>
                            {
                                new Lesson
                                {
                                    Title = "Introduction to Cells",
                                    Objective = "Understand basic cell theory",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 0
                                },
                                new Lesson
                                {
                                    Title = "Prokaryotic vs Eukaryotic",
                                    Objective = "Compare and contrast cell types",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 1
                                },
                                new Lesson
                                {
                                    Title = "Cell Membrane",
                                    Objective = "Study membrane structure and function",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 2
                                },
                                new Lesson
                                {
                                    Title = "Cell Nucleus",
                                    Objective = "Understand nuclear function and DNA storage",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 3
                                }
                            },
                            SubTopics = new List<SubTopic>
                            {
                                new SubTopic
                                {
                                    Title = "Cell Organelles",
                                    Description = "Specialized structures within cells",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 0,
                                    Lessons = new List<Lesson>
                                    {
                                        new Lesson
                                        {
                                            Title = "Mitochondria and Energy",
                                            Objective = "Study cellular energy production",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 0
                                        },
                                        new Lesson
                                        {
                                            Title = "Endoplasmic Reticulum",
                                            Objective = "Understand protein and lipid synthesis",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 1
                                        },
                                        new Lesson
                                        {
                                            Title = "Golgi Apparatus",
                                            Objective = "Study protein processing and shipping",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 2
                                        },
                                        new Lesson
                                        {
                                            Title = "Cell Structure Review",
                                            Objective = "Review all organelles and functions",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 3
                                        }
                                    }
                                }
                            }
                        },
                        new Topic
                        {
                            Title = "Genetics",
                            Description = "Heredity and genetic variation",
                            UserId = adminUser.Id,
                            Archived = false,
                            Visibility = VisibilityType.Private,
                            SortOrder = 1,
                            Lessons = new List<Lesson>
                            {
                                new Lesson
                                {
                                    Title = "DNA Structure",
                                    Objective = "Understand DNA double helix structure",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 0
                                },
                                new Lesson
                                {
                                    Title = "DNA Replication",
                                    Objective = "Study how DNA copies itself",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 1
                                },
                                new Lesson
                                {
                                    Title = "Transcription",
                                    Objective = "Understand DNA to RNA conversion",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 2
                                },
                                new Lesson
                                {
                                    Title = "Translation",
                                    Objective = "Study RNA to protein synthesis",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 3
                                }
                            },
                            SubTopics = new List<SubTopic>
                            {
                                new SubTopic
                                {
                                    Title = "Heredity Patterns",
                                    Description = "How traits pass from parents to offspring",
                                    UserId = adminUser.Id,
                                    Archived = false,
                                    Visibility = VisibilityType.Private,
                                    SortOrder = 0,
                                    Lessons = new List<Lesson>
                                    {
                                        new Lesson
                                        {
                                            Title = "Mendel's Laws",
                                            Objective = "Understand basic inheritance patterns",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 0
                                        },
                                        new Lesson
                                        {
                                            Title = "Punnett Squares",
                                            Objective = "Predict offspring genotypes and phenotypes",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 1
                                        },
                                        new Lesson
                                        {
                                            Title = "Incomplete Dominance",
                                            Objective = "Study non-dominant inheritance patterns",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 2
                                        },
                                        new Lesson
                                        {
                                            Title = "Genetics Test",
                                            Objective = "Assessment of genetics concepts",
                                            UserId = adminUser.Id,
                                            Archived = false,
                                            Visibility = VisibilityType.Private,
                                            SortOrder = 3
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                courses.Add(mathCourse);
                courses.Add(scienceCourse);
                context.Courses.AddRange(courses);
                await context.SaveChangesAsync();

                // SEED USER CONFIGURATION with Period Assignments
                // UPDATED: DatabaseSeeder.cs - Add TeachingDays and schedule dates
                // Replace the UserConfiguration seeding section (around line 355) with this:

                // SEED USER CONFIGURATION with Period Assignments
                var userConfig = new UserConfiguration
                {
                    UserId = adminUser.Id,
                    SchoolYear = "2024-2025",
                    PeriodsPerDay = 6,
                    StartDate = new DateTime(2024, 8, 15),    // NEW: Required for master schedule
                    EndDate = new DateTime(2025, 6, 10),      // NEW: Required for master schedule
                    LastUpdated = DateTime.UtcNow,
                    PeriodAssignments = new List<PeriodAssignment>
                    {
                        // Math course periods
                        new PeriodAssignment
                        {
                            Period = 1,
                            CourseId = mathCourse.Id,
                            TeachingDays = "Monday,Wednesday,Friday",  // NEW: Required field
                            Room = "Math 101",
                            Notes = "Advanced section",
                            BackgroundColor = "#E3F2FD",
                            FontColor = "#1976D2"
                        },
                        new PeriodAssignment
                        {
                            Period = 3,
                            CourseId = mathCourse.Id,
                            TeachingDays = "Monday,Wednesday,Friday",  // NEW: Required field
                            Room = "Math 101",
                            Notes = "Regular section",
                            BackgroundColor = "#E3F2FD",
                            FontColor = "#1976D2"
                        },
                        // Science course periods
                        new PeriodAssignment
                        {
                            Period = 2,
                            CourseId = scienceCourse.Id,
                            TeachingDays = "Monday,Wednesday,Friday",  // NEW: Required field
                            Room = "Science 201",
                            Notes = "Lab available",
                            BackgroundColor = "#E8F5E8",
                            FontColor = "#388E3C"
                        },
                        new PeriodAssignment
                        {
                            Period = 5,
                            CourseId = scienceCourse.Id,
                            TeachingDays = "Monday,Wednesday,Friday",  // NEW: Required field
                            Room = "Science 201",
                            Notes = "Lab available",
                            BackgroundColor = "#E8F5E8",
                            FontColor = "#388E3C"
                        },
                        // Duty periods
                        new PeriodAssignment
                        {
                            Period = 4,
                            CourseId = null,
                            SpecialPeriodType = SpecialPeriodType.Lunch,
                            TeachingDays = "Monday,Wednesday,Friday",  // NEW: Required field
                            Room = "Cafeteria",
                            Notes = "Supervise lunch period",
                            BackgroundColor = "#FFF3E0",
                            FontColor = "#F57C00"
                        },
                        new PeriodAssignment
                        {
                            Period = 6,
                            CourseId = null,
                            SpecialPeriodType = SpecialPeriodType.Prep,
                            TeachingDays = "Monday,Wednesday,Friday",  // NEW: Required field
                            Room = "Math 101",
                            Notes = "Lesson planning and grading",
                            BackgroundColor = "#F3E5F5",
                            FontColor = "#7B1FA2"
                        }
                    }
                };

                context.UserConfigurations.Add(userConfig);
                await context.SaveChangesAsync();

                logger.LogInformation("Master schedule test data seeded successfully:");
                logger.LogInformation("- 2 courses: Algebra I (16 lessons), Biology I (16 lessons)");
                logger.LogInformation("- Each course: 2 topics with 1 subtopic each");
                logger.LogInformation("- Period assignments: Math (P1,P3), Science (P2,P5), Lunch (P4), Prep (P6)");
                logger.LogInformation("- Total lessons available: 32 lessons across 4 teaching periods");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed master schedule test data: {Message}", ex.Message);
                throw;
            }
        }
    }
}