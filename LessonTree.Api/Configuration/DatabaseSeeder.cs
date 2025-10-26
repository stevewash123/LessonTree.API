using LessonTree.DAL;
using LessonTree.DAL.Domain;
using LessonTree.Models.Enums;
using LessonTree.BLL.Services;
using LessonTree.Service.Service.SystemConfig;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LessonTree.API.Configuration
{
    public static class DatabaseSeeder
    {
        public static async Task<bool> ShouldSeedDatabaseAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                var systemConfigService = serviceProvider.GetRequiredService<ISystemConfigService>();
                var shouldSeed = await systemConfigService.ShouldReseedAsync();

                logger.LogInformation("Should seed database: {ShouldSeed}", shouldSeed);
                return shouldSeed;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking if should seed database. Defaulting to not seed.");
                return false;
            }
        }

        public static async Task SeedDatabaseAsync(
            LessonTreeContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger logger,
            IHostEnvironment env,
            IServiceProvider serviceProvider) // ✅ Service provider for generation service
        {
            try
            {
                if (!env.IsDevelopment())
                {
                    logger.LogInformation("Skipping test data seeding: not in Development mode.");
                    return;
                }

                logger.LogInformation("🌱 Starting comprehensive test data seeding...");

                // ✅ PHASE 1: Seed base data (users, courses, configurations)
                await SeedBaseDataAsync(context, userManager, roleManager, logger);

                // ✅ PHASE 2: Generate schedules using real ScheduleGenerationService
                await GenerateSchedulesFromConfigurationsAsync(serviceProvider, logger);

                // ✅ PHASE 3: Update last seed date
                await UpdateLastSeedDateAsync(serviceProvider, logger);

                logger.LogInformation("🎉 Comprehensive test data seeding completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to seed comprehensive test data: {Message}", ex.Message);
                throw;
            }
        }

        // ✅ PHASE 1: Base data seeding (existing logic)
        private static async Task SeedBaseDataAsync(
            LessonTreeContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger logger)
        {
            logger.LogInformation("📋 Phase 1: Seeding base data (users, courses, configurations)");

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
            context.PeriodAssignments.RemoveRange(context.PeriodAssignments);
            context.ScheduleConfigurations.RemoveRange(context.ScheduleConfigurations);
            context.Departments.RemoveRange(context.Departments);
            context.Schools.RemoveRange(context.Schools);
            context.Districts.RemoveRange(context.Districts);
            await context.SaveChangesAsync();

            // Seed Roles (existing code)
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

            // Seed District, School, Department (existing code)
            var district = new District { Name = "Test District", Description = "Test district" };
            context.Districts.Add(district);
            await context.SaveChangesAsync();

            var school = new School { Name = "Test School", Description = "Test school", DistrictId = district.Id };
            context.Schools.Add(school);
            await context.SaveChangesAsync();

            var department = new Department { Name = "Mathematics", Description = "Math department", SchoolId = school.Id };
            context.Departments.Add(department);
            await context.SaveChangesAsync();

            // Seed Admin User (existing code)
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

            // Seed Courses with Topics and Lessons (existing code - all the complex course structure)
            var courses = await SeedCoursesAsync(context, adminUser, logger);

            // ✅ Seed Schedule Configuration (WITHOUT generating schedule yet)
            var scheduleConfig = new ScheduleConfiguration
            {
                UserId = adminUser.Id,
                Title = "Test Schedule Configuration",
                SchoolYear = "2025-2026",
                StartDate = DateTime.Today.AddDays(7), // Start next week
                EndDate = DateTime.Today.AddDays(21),   // End in 3 weeks
                PeriodsPerDay = 2,
                TeachingDays = "Monday,Tuesday,Wednesday,Thursday,Friday",
                Status = ScheduleStatus.Active,
                IsTemplate = false,
                CreatedDate = DateTime.UtcNow,
                PeriodAssignments = new List<PeriodAssignment>()
            };

            // Assign Course 1 to Period 1, Course 2 to Period 2
            var course1 = courses[0];
            var course2 = courses[1];

            scheduleConfig.PeriodAssignments.Add(new PeriodAssignment
            {
                Period = 1,
                CourseId = course1.Id,
                SpecialPeriodType = null,
                TeachingDays = "Monday,Tuesday,Wednesday,Thursday,Friday",
                Room = "Room 101",
                Notes = "Course 1 - Period 1",
                BackgroundColor = "#e3f2fd",
                FontColor = "#1976d2"
            });

            scheduleConfig.PeriodAssignments.Add(new PeriodAssignment
            {
                Period = 2,
                CourseId = course2.Id,
                SpecialPeriodType = null,
                TeachingDays = "Monday,Tuesday,Wednesday,Thursday,Friday",
                Room = "Room 102",
                Notes = "Course 2 - Period 2",
                BackgroundColor = "#f3e5f5",
                FontColor = "#7b1fa2"
            });

            context.ScheduleConfigurations.Add(scheduleConfig);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Seeded ScheduleConfiguration {scheduleConfig.Id} with 2 periods and 2 course assignments");

            // Seed User Configuration
            var userConfig = new UserConfiguration
            {
                UserId = adminUser.Id,
                SettingsJson = "{\"theme\":\"light\"}",
                LastUpdated = DateTime.UtcNow
            };

            context.UserConfigurations.Add(userConfig);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Phase 1 completed: Base data seeded successfully");
        }

        // ✅ PHASE 2: Generate schedules using real service (SIMPLIFIED - no duplicate persistence)
        private static async Task GenerateSchedulesFromConfigurationsAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            logger.LogInformation("🚀 Phase 2: Generating schedules from configurations using ScheduleGenerationService");

            try
            {
                var context = serviceProvider.GetRequiredService<LessonTreeContext>();
                var scheduleGenerationService = serviceProvider.GetRequiredService<IScheduleGenerationService>();

                // Find all configurations that don't have schedules yet
                var configurationsWithoutSchedules = await context.ScheduleConfigurations
                    .Where(config => !context.Schedules.Any(schedule => schedule.ScheduleConfigurationId == config.Id))
                    .ToListAsync();

                logger.LogInformation($"📋 Found {configurationsWithoutSchedules.Count} configurations that need schedule generation");

                foreach (var config in configurationsWithoutSchedules)
                {
                    logger.LogInformation($"🔄 Generating schedule for configuration {config.Id} ('{config.Title}') for user {config.UserId}");

                    // ✅ SIMPLIFIED: Just call the service - it now handles persistence internally
                    var generationResult = await scheduleGenerationService.GenerateScheduleFromConfigurationAsync(
                        config.Id,
                        config.UserId
                    );

                    if (generationResult.Success && generationResult.Schedule != null)
                    {
                        logger.LogInformation($"✅ Generated and saved schedule {generationResult.Schedule.Id} with {generationResult.TotalEventsGenerated} events");
                        logger.LogInformation($"   📊 Event breakdown across {generationResult.EventsByPeriod.Count} periods");

                        // Log warnings if any
                        if (generationResult.Warnings.Any())
                        {
                            logger.LogWarning($"⚠️ Generation warnings: {string.Join(", ", generationResult.Warnings)}");
                        }
                    }
                    else
                    {
                        logger.LogError($"❌ Schedule generation failed for configuration {config.Id}: {string.Join(", ", generationResult.Errors)}");
                        if (generationResult.Warnings.Any())
                        {
                            logger.LogWarning($"⚠️ Generation warnings: {string.Join(", ", generationResult.Warnings)}");
                        }
                    }
                }

                logger.LogInformation("✅ Phase 2 completed: Schedule generation finished");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"❌ Exception during schedule generation phase: {ex.Message}");
                // Don't throw - let the seeding complete even if schedule generation fails
            }
        }

        // ✅ Helper method for course seeding (extract existing course creation logic)
        // Fixed SeedCoursesAsync method with correct SortOrder logic
        private static async Task<List<Course>> SeedCoursesAsync(LessonTreeContext context, User adminUser, ILogger logger)
        {
            logger.LogInformation("📚 Seeding courses with topics and lessons");

            var courses = new List<Course>();
            var userId = adminUser.Id;
            var globalLessonCounter = 1;

            for (int courseIndex = 1; courseIndex <= 2; courseIndex++)
            {
                var course = new Course
                {
                    Title = $"Course {courseIndex}",
                    Description = $"Test course {courseIndex} for comprehensive testing",
                    UserId = userId,
                    Archived = false,
                    Visibility = VisibilityType.Private,
                    Topics = new List<Topic>()
                };

                // Create 2 topics per course
                for (int topicIndex = 1; topicIndex <= 2; topicIndex++)
                {
                    var topic = new Topic
                    {
                        Title = $"Course {courseIndex} - Topic {topicIndex}",
                        Description = $"Topic {topicIndex} for course {courseIndex}",
                        CourseId = 0,
                        UserId = userId,
                        SortOrder = topicIndex, // Topics: 1, 2 (within course)
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Lessons = new List<Lesson>(),
                        SubTopics = new List<SubTopic>()
                    };

                    // ✅ TRACK SORT ORDER WITHIN TOPIC (mixed space for lessons + subtopics)
                    int topicSortOrder = 0;

                    // Create 3 direct lessons per topic FIRST
                    for (int lessonIndex = 1; lessonIndex <= 3; lessonIndex++)
                    {
                        var lesson = new Lesson
                        {
                            Title = $"Lesson {globalLessonCounter}",
                            Objective = $"C{courseIndex}T{topicIndex}L{lessonIndex}",
                            Methods = "Instruction, Practice, Assessment",
                            Materials = "Textbook, Worksheets, Digital tools",
                            Assessment = "Quiz, Discussion, Homework",
                            ClassTime = "45 minutes",
                            SpecialNeeds = "Visual aids available",
                            Level = "Beginner",
                            TopicId = 0,
                            SubTopicId = null,
                            UserId = userId,
                            SortOrder = topicSortOrder++, // ✅ 0, 1, 2 (within topic)
                            Archived = false,
                            Visibility = VisibilityType.Private
                        };

                        topic.Lessons.Add(lesson);
                        globalLessonCounter++;
                    }

                    // ✅ CREATE SUBTOPIC AFTER DIRECT LESSONS
                    var subTopic = new SubTopic
                    {
                        Title = $"Course {courseIndex} - Topic {topicIndex} - SubTopic A",
                        Description = $"SubTopic A for topic {topicIndex}",
                        TopicId = 0,
                        UserId = userId,
                        SortOrder = topicSortOrder++, // ✅ NEXT POSITION: 3 (after direct lessons)
                        IsDefault = false,
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Lessons = new List<Lesson>()
                    };

                    // Create 2 lessons per subtopic
                    for (int subLessonIndex = 1; subLessonIndex <= 2; subLessonIndex++)
                    {
                        var subLesson = new Lesson
                        {
                            Title = $"Lesson {globalLessonCounter}",
                            Objective = $"C{courseIndex}T{topicIndex}S1L{subLessonIndex}",
                            Methods = "Guided practice, Independent work",
                            Materials = "Manipulatives, Digital resources",
                            Assessment = "Formative assessment, Peer review",
                            ClassTime = "30 minutes",
                            SpecialNeeds = "Differentiated instruction",
                            Level = "Intermediate",
                            TopicId = null,
                            SubTopicId = 0,
                            UserId = userId,
                            SortOrder = subLessonIndex - 1, // ✅ 0, 1 (within subtopic)
                            Archived = false,
                            Visibility = VisibilityType.Private
                        };

                        subTopic.Lessons.Add(subLesson);
                        globalLessonCounter++;
                    }

                    topic.SubTopics.Add(subTopic);
                    course.Topics.Add(topic);
                }

                courses.Add(course);
            }

            context.Courses.AddRange(courses);
            await context.SaveChangesAsync();

            // Log the corrected structure
            logger.LogInformation("✅ CORRECTED SORT ORDER STRUCTURE:");
            foreach (var course in courses.Take(1)) // Just log first course for verification
            {
                foreach (var topic in course.Topics.Take(1)) // Just log first topic
                {
                    logger.LogInformation($"  Topic {topic.Id} '{topic.Title}' (SortOrder: {topic.SortOrder}):");

                    // Direct lessons
                    foreach (var lesson in topic.Lessons.OrderBy(l => l.SortOrder))
                    {
                        logger.LogInformation($"    Lesson {lesson.Id} '{lesson.Title}' (SortOrder: {lesson.SortOrder}) [DIRECT]");
                    }

                    // SubTopics
                    foreach (var subtopic in topic.SubTopics.OrderBy(st => st.SortOrder))
                    {
                        logger.LogInformation($"    SubTopic {subtopic.Id} '{subtopic.Title}' (SortOrder: {subtopic.SortOrder}):");
                        foreach (var subLesson in subtopic.Lessons.OrderBy(l => l.SortOrder))
                        {
                            logger.LogInformation($"      Lesson {subLesson.Id} '{subLesson.Title}' (SortOrder: {subLesson.SortOrder}) [SUB]");
                        }
                    }
                }
            }

            var totalLessons = courses.SelectMany(c => c.Topics)
                .SelectMany(t => t.Lessons.Concat(t.SubTopics.SelectMany(st => st.Lessons)))
                .Count();

            logger.LogInformation($"✅ Seeded {courses.Count} courses with {totalLessons} lessons");
            return courses;
        }

        // ✅ PHASE 3: Update last seed date
        private static async Task UpdateLastSeedDateAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                var systemConfigService = serviceProvider.GetRequiredService<ISystemConfigService>();
                await systemConfigService.SetLastSeedDateAsync(DateTime.UtcNow);

                logger.LogInformation("✅ Updated last seed date to current time");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to update last seed date");
                // Don't throw - seeding was successful even if we can't update the date
            }
        }
    }
}