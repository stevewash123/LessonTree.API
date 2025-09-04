using LessonTree.BLL.Services;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using LessonTree.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.Tests.Services
{
    /// <summary>
    /// Focused tests for ScheduleGenerationService critical algorithms
    /// Simplified to compile with current project structure
    /// </summary>
    public class ScheduleGenerationServiceTestsSimple : TestBase
    {
        private readonly Mock<IScheduleConfigurationRepository> _mockConfigRepository;
        private readonly Mock<ILessonRepository> _mockLessonRepository;
        private readonly Mock<IScheduleRepository> _mockScheduleRepository;
        private readonly ScheduleGenerationService _service;

        public ScheduleGenerationServiceTestsSimple()
        {
            _mockConfigRepository = new Mock<IScheduleConfigurationRepository>();
            _mockLessonRepository = new Mock<ILessonRepository>();
            _mockScheduleRepository = new Mock<IScheduleRepository>();
            var logger = CreateLogger<ScheduleGenerationService>();

            _service = new ScheduleGenerationService(
                _mockConfigRepository.Object,
                _mockLessonRepository.Object,
                _mockScheduleRepository.Object,
                Mapper,
                logger);
        }

        [Fact]
        public async Task ValidateConfigurationForGenerationAsync_WithNonExistentConfiguration_ShouldReturnError()
        {
            // Arrange
            const int configId = 999;
            const int userId = 1;
            
            _mockConfigRepository.Setup(r => r.GetByIdAsync(configId))
                .ReturnsAsync((ScheduleConfiguration?)null);

            // Act
            var result = await _service.ValidateConfigurationForGenerationAsync(configId, userId);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.CanGenerateSchedule.Should().BeFalse();
            result.Errors.Should().Contain($"Configuration {configId} not found");
        }

        [Fact]
        public async Task ValidateConfigurationForGenerationAsync_WithInvalidDateRange_ShouldReturnErrors()
        {
            // Arrange
            const int configId = 1;
            const int userId = 1;
            
            var configuration = new ScheduleConfiguration
            {
                Id = configId,
                UserId = userId,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(-10), // Invalid: end before start
                PeriodsPerDay = 6,
                TeachingDays = "monday,tuesday,wednesday,thursday,friday",
                PeriodAssignments = new List<PeriodAssignment>()
            };
            
            _mockConfigRepository.Setup(r => r.GetByIdAsync(configId))
                .ReturnsAsync(configuration);

            // Act
            var result = await _service.ValidateConfigurationForGenerationAsync(configId, userId);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.CanGenerateSchedule.Should().BeFalse();
            result.Errors.Should().Contain("Start date must be before end date");
        }

        [Fact]
        public async Task ValidateConfigurationForGenerationAsync_WithNoPeriodAssignments_ShouldReturnInvalid()
        {
            // Arrange
            const int configId = 1;
            const int userId = 1;
            
            var configuration = new ScheduleConfiguration
            {
                Id = configId,
                UserId = userId,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(180),
                PeriodsPerDay = 6,
                TeachingDays = "monday,tuesday,wednesday,thursday,friday",
                PeriodAssignments = new List<PeriodAssignment>()
            };
            
            _mockConfigRepository.Setup(r => r.GetByIdAsync(configId))
                .ReturnsAsync(configuration);

            // Act
            var result = await _service.ValidateConfigurationForGenerationAsync(configId, userId);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.CanGenerateSchedule.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ApplySpecialDayIntegrationAsync_WithSpecialDay_ShouldAddSpecialDayEvent()
        {
            // Arrange
            var specialDayDate = DateTime.Today.AddDays(3);
            var baseEvents = new List<ScheduleEventResource>
            {
                new()
                {
                    Id = 1,
                    Date = DateTime.Today,
                    Period = 1,
                    EventType = "Lesson",
                    EventCategory = "Lesson",
                    LessonId = 1
                },
                new()
                {
                    Id = 2,
                    Date = DateTime.Today.AddDays(1),
                    Period = 1,
                    EventType = "Lesson",
                    EventCategory = "Lesson", 
                    LessonId = 2
                }
            };
            
            var specialDays = new List<SpecialDayResource>
            {
                new()
                {
                    Id = 100,
                    Date = specialDayDate,
                    Title = "Teacher Work Day",
                    EventType = "WorkDay",
                    Periods = new int[] { 1 }
                }
            };

            // Act
            var result = await _service.ApplySpecialDayIntegrationAsync(baseEvents, specialDays);

            // Assert
            result.Should().NotBeEmpty();
            
            // Should have original events plus special day event
            result.Should().HaveCount(baseEvents.Count + 1);
            
            // Should contain the special day event
            var specialDayEvent = result.FirstOrDefault(e => e.EventType == "WorkDay");
            specialDayEvent.Should().NotBeNull();
            specialDayEvent!.Date.Should().Be(specialDayDate);
            specialDayEvent.Period.Should().Be(1);
            specialDayEvent.EventCategory.Should().Be("SpecialDay");
            specialDayEvent.Comment.Should().Be("Teacher Work Day");
        }

        [Fact]
        public async Task ApplySpecialDayIntegrationAsync_WithNoSpecialDays_ShouldReturnOriginalEvents()
        {
            // Arrange
            var baseEvents = new List<ScheduleEventResource>
            {
                new()
                {
                    Id = 1,
                    Date = DateTime.Today,
                    Period = 1,
                    EventType = "Lesson",
                    EventCategory = "Lesson",
                    LessonId = 1
                }
            };
            var specialDays = new List<SpecialDayResource>();

            // Act
            var result = await _service.ApplySpecialDayIntegrationAsync(baseEvents, specialDays);

            // Assert
            result.Should().BeEquivalentTo(baseEvents);
        }
    }
}