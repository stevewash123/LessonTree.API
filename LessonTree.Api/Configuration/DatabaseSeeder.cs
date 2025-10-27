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
                // Allow seeding in all environments for demo purposes
                // if (!env.IsDevelopment())
                // {
                //     logger.LogInformation("Skipping test data seeding: not in Development mode.");
                //     return;
                // }

                logger.LogInformation("🌱 Starting optimized test data seeding (PostgreSQL performance optimized)...");

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

            // Seed Guest User for demo environment
            var guestUser = await userManager.FindByNameAsync("guest");
            if (guestUser == null)
            {
                logger.LogInformation("Creating guest user");
                guestUser = new User
                {
                    UserName = "guest",
                    FirstName = "Demo",
                    LastName = "User",
                    DistrictId = district.Id,
                    SchoolId = school.Id
                };
                var result = await userManager.CreateAsync(guestUser, "Guest123!");
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to create guest user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    throw new Exception("Guest user creation failed.");
                }

                result = await userManager.AddToRoleAsync(guestUser, "freeUser");
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to assign freeUser role to guest: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    throw new Exception("Guest role assignment failed.");
                }

                guestUser.Departments.Add(department);
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

                // ✅ OPTIMIZED: Use LEFT JOIN instead of subquery for better PostgreSQL performance
                var existingScheduleConfigIds = await context.Schedules
                    .Select(s => s.ScheduleConfigurationId)
                    .Distinct()
                    .ToListAsync();

                var configurationsWithoutSchedules = await context.ScheduleConfigurations
                    .Where(config => !existingScheduleConfigIds.Contains(config.Id))
                    .ToListAsync();

                logger.LogInformation($"📋 Found {configurationsWithoutSchedules.Count} configurations that need schedule generation");

                // ✅ SAFETY: Limit to max 3 configurations to prevent hangs
                var configsToProcess = configurationsWithoutSchedules.Take(3).ToList();
                if (configurationsWithoutSchedules.Count > 3)
                {
                    logger.LogWarning($"⚠️ Limited processing to first 3 configurations (out of {configurationsWithoutSchedules.Count}) to prevent timeouts");
                }

                foreach (var config in configsToProcess)
                {
                    logger.LogInformation($"🔄 Generating schedule for configuration {config.Id} ('{config.Title}') for user {config.UserId}");

                    try
                    {
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
                    catch (Exception configEx)
                    {
                        logger.LogError(configEx, $"❌ Exception generating schedule for configuration {config.Id}: {configEx.Message}");
                        // Continue with next configuration
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
        // Updated SeedCoursesAsync method to create 5 high school level courses with 20 lessons each
        private static async Task<List<Course>> SeedCoursesAsync(LessonTreeContext context, User adminUser, ILogger logger)
        {
            logger.LogInformation("📚 Seeding 3 high school level courses with 10 lessons each (optimized for faster seeding)");

            var courses = new List<Course>();
            var userId = adminUser.Id;
            var globalLessonCounter = 1;

            // Define high school subjects (reduced for faster seeding)
            var subjects = new[]
            {
                new { Title = "Algebra II", Description = "Advanced algebraic concepts including polynomials, exponential and logarithmic functions" },
                new { Title = "American History", Description = "Comprehensive study of American history from colonial times to present" },
                new { Title = "Biology", Description = "Introduction to biological sciences including cell biology, genetics, and ecology" }
            };

            for (int courseIndex = 0; courseIndex < subjects.Length; courseIndex++)
            {
                var subject = subjects[courseIndex];
                var course = new Course
                {
                    Title = subject.Title,
                    Description = subject.Description,
                    UserId = userId,
                    Archived = false,
                    Visibility = VisibilityType.Private,
                    Topics = new List<Topic>()
                };

                // Create 2 topics per course (5 lessons each = 10 total)
                for (int topicIndex = 1; topicIndex <= 2; topicIndex++)
                {
                    var topic = new Topic
                    {
                        Title = $"{subject.Title} - Unit {topicIndex}",
                        Description = $"Unit {topicIndex} covering key concepts in {subject.Title}",
                        CourseId = 0,
                        UserId = userId,
                        SortOrder = topicIndex,
                        Archived = false,
                        Visibility = VisibilityType.Private,
                        Lessons = new List<Lesson>(),
                        SubTopics = new List<SubTopic>()
                    };

                    // Create 5 lessons per topic (total 10 per course)
                    for (int lessonIndex = 1; lessonIndex <= 5; lessonIndex++)
                    {
                        var lesson = new Lesson
                        {
                            Title = $"{subject.Title} - Lesson {globalLessonCounter}",
                            Objective = GenerateLessonObjective(subject.Title, topicIndex, lessonIndex),
                            Methods = GenerateTeachingMethods(subject.Title),
                            Materials = GenerateMaterials(subject.Title),
                            Assessment = GenerateAssessment(subject.Title),
                            ClassTime = "50 minutes",
                            SpecialNeeds = "Accommodations available for diverse learners",
                            Level = "High School",
                            TopicId = 0,
                            SubTopicId = null,
                            UserId = userId,
                            SortOrder = lessonIndex - 1,
                            Archived = false,
                            Visibility = VisibilityType.Private
                        };

                        topic.Lessons.Add(lesson);
                        globalLessonCounter++;
                    }

                    course.Topics.Add(topic);
                }

                courses.Add(course);
            }

            context.Courses.AddRange(courses);
            await context.SaveChangesAsync();

            var totalLessons = courses.SelectMany(c => c.Topics)
                .SelectMany(t => t.Lessons)
                .Count();

            logger.LogInformation($"✅ Seeded {courses.Count} high school courses with {totalLessons} lessons total");
            return courses;
        }

        private static string GenerateLessonObjective(string subject, int unit, int lesson)
        {
            return subject switch
            {
                "Algebra II" => $"Students will master {GetAlgebraTopics(unit, lesson)}",
                "American History" => $"Students will analyze {GetHistoryTopics(unit, lesson)}",
                "Biology" => $"Students will understand {GetBiologyTopics(unit, lesson)}",
                "English Literature" => $"Students will examine {GetLiteratureTopics(unit, lesson)}",
                "Chemistry" => $"Students will explore {GetChemistryTopics(unit, lesson)}",
                _ => $"Students will learn key concepts in Unit {unit}, Lesson {lesson}"
            };
        }

        private static string GetAlgebraTopics(int unit, int lesson) => unit switch
        {
            1 => lesson switch { 1 => "polynomial operations", 2 => "factoring techniques", 3 => "synthetic division", 4 => "polynomial graphs", 5 => "polynomial applications", _ => "algebraic concepts" },
            2 => lesson switch { 1 => "exponential functions", 2 => "logarithmic functions", 3 => "exponential equations", 4 => "logarithmic equations", 5 => "exponential models", _ => "exponential concepts" },
            3 => lesson switch { 1 => "rational functions", 2 => "rational equations", 3 => "partial fractions", 4 => "rational inequalities", 5 => "rational models", _ => "rational concepts" },
            4 => lesson switch { 1 => "conic sections", 2 => "parabolas and circles", 3 => "ellipses and hyperbolas", 4 => "systems with conics", 5 => "conic applications", _ => "geometric concepts" },
            _ => "mathematical concepts"
        };

        private static string GetHistoryTopics(int unit, int lesson) => unit switch
        {
            1 => lesson switch { 1 => "colonial foundations", 2 => "revolutionary causes", 3 => "Declaration of Independence", 4 => "Revolutionary War", 5 => "Articles of Confederation", _ => "early American history" },
            2 => lesson switch { 1 => "Constitutional Convention", 2 => "federalism debates", 3 => "Bill of Rights", 4 => "early presidencies", 5 => "party system emergence", _ => "constitutional period" },
            3 => lesson switch { 1 => "westward expansion", 2 => "slavery debates", 3 => "sectional tensions", 4 => "Civil War causes", 5 => "Civil War outcomes", _ => "Civil War era" },
            4 => lesson switch { 1 => "Reconstruction policies", 2 => "industrial revolution", 3 => "immigration patterns", 4 => "progressive reforms", 5 => "World War I impact", _ => "modern America" },
            _ => "historical developments"
        };

        private static string GetBiologyTopics(int unit, int lesson) => unit switch
        {
            1 => lesson switch { 1 => "cell structure", 2 => "cell membrane function", 3 => "cellular respiration", 4 => "photosynthesis", 5 => "cell division", _ => "cellular biology" },
            2 => lesson switch { 1 => "DNA structure", 2 => "DNA replication", 3 => "protein synthesis", 4 => "genetic variation", 5 => "inheritance patterns", _ => "genetics" },
            3 => lesson switch { 1 => "evolution theory", 2 => "natural selection", 3 => "speciation", 4 => "phylogeny", 5 => "biogeography", _ => "evolution" },
            4 => lesson switch { 1 => "ecosystem structure", 2 => "energy flow", 3 => "nutrient cycles", 4 => "population dynamics", 5 => "conservation biology", _ => "ecology" },
            _ => "biological concepts"
        };

        private static string GetLiteratureTopics(int unit, int lesson) => unit switch
        {
            1 => lesson switch { 1 => "literary elements", 2 => "character analysis", 3 => "plot structure", 4 => "theme development", 5 => "point of view", _ => "narrative elements" },
            2 => lesson switch { 1 => "poetic devices", 2 => "meter and rhyme", 3 => "figurative language", 4 => "poetic forms", 5 => "contemporary poetry", _ => "poetry analysis" },
            3 => lesson switch { 1 => "dramatic structure", 2 => "character motivation", 3 => "tragic elements", 4 => "dramatic irony", 5 => "theatrical techniques", _ => "drama analysis" },
            4 => lesson switch { 1 => "critical theories", 2 => "historical context", 3 => "author's purpose", 4 => "comparative analysis", 5 => "literary criticism", _ => "literary criticism" },
            _ => "literary concepts"
        };

        private static string GetChemistryTopics(int unit, int lesson) => unit switch
        {
            1 => lesson switch { 1 => "atomic structure", 2 => "electron configuration", 3 => "periodic trends", 4 => "ionic bonding", 5 => "covalent bonding", _ => "atomic concepts" },
            2 => lesson switch { 1 => "molecular geometry", 2 => "intermolecular forces", 3 => "phase changes", 4 => "solution properties", 5 => "colligative properties", _ => "molecular concepts" },
            3 => lesson switch { 1 => "reaction types", 2 => "stoichiometry", 3 => "limiting reactants", 4 => "percent yield", 5 => "energy changes", _ => "chemical reactions" },
            4 => lesson switch { 1 => "gas laws", 2 => "kinetic theory", 3 => "equilibrium", 4 => "acids and bases", 5 => "oxidation-reduction", _ => "chemical principles" },
            _ => "chemical concepts"
        };

        private static string GenerateTeachingMethods(string subject) => subject switch
        {
            "Algebra II" => "Direct instruction, guided practice, problem-solving activities, graphing calculator use",
            "American History" => "Document analysis, timeline activities, group discussions, multimedia presentations",
            "Biology" => "Laboratory investigations, microscopy, data analysis, scientific method practice",
            "English Literature" => "Close reading, literary analysis, creative writing, group discussions",
            "Chemistry" => "Laboratory experiments, molecular modeling, calculations, safety procedures",
            _ => "Interactive instruction, hands-on activities, collaborative learning"
        };

        private static string GenerateMaterials(string subject) => subject switch
        {
            "Algebra II" => "Graphing calculators, algebra tiles, coordinate grids, function tables",
            "American History" => "Primary sources, maps, timelines, historical documents, multimedia resources",
            "Biology" => "Microscopes, specimens, lab equipment, models, data collection tools",
            "English Literature" => "Literary texts, annotation tools, writing materials, discussion guides",
            "Chemistry" => "Lab equipment, periodic tables, molecular models, safety equipment",
            _ => "Textbooks, worksheets, digital resources, manipulatives"
        };

        private static string GenerateAssessment(string subject) => subject switch
        {
            "Algebra II" => "Problem sets, graphing exercises, unit tests, project presentations",
            "American History" => "Document-based questions, timeline projects, research papers, discussions",
            "Biology" => "Lab reports, data analysis, scientific drawings, unit assessments",
            "English Literature" => "Literary essays, creative projects, reading comprehension, discussions",
            "Chemistry" => "Lab reports, calculations, concept maps, practical assessments",
            _ => "Quizzes, projects, presentations, formative assessments"
        };

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