using LessonTree.BLL.Services;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using LessonTree.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace LessonTree.Tests.Services
{
    /// <summary>
    /// Unit tests for ScheduleService Special Day operations with calendar optimization
    /// Tests create, update, and delete operations with partial schedule regeneration logic
    /// </summary>
    public class ScheduleServiceSpecialDayOptimizationTests : TestBase
    {
        private readonly Mock<IScheduleRepository> _mockScheduleRepository;
        private readonly Mock<IScheduleGenerationService> _mockScheduleGenerationService;
        private readonly Mock<IScheduleConfigurationService> _mockScheduleConfigurationService;
        private readonly Mock<IBackgroundScheduleService> _mockBackgroundScheduleService;
        private readonly ScheduleService _service;
        private const int TestUserId = 1;
        private const int TestScheduleId = 100;
        private const int TestSpecialDayId = 200;
        private const int TestConfigurationId = 50;

        public ScheduleServiceSpecialDayOptimizationTests()
        {
            _mockScheduleRepository = new Mock<IScheduleRepository>();
            _mockScheduleGenerationService = new Mock<IScheduleGenerationService>();
            _mockScheduleConfigurationService = new Mock<IScheduleConfigurationService>();
            _mockBackgroundScheduleService = new Mock<IBackgroundScheduleService>();
            var logger = CreateLogger<ScheduleService>();

            _service = new ScheduleService(
                _mockScheduleRepository.Object,
                Mapper,
                logger,
                _mockScheduleGenerationService.Object,
                _mockScheduleConfigurationService.Object,
                _mockBackgroundScheduleService.Object);
        }

        #region CreateSpecialDayAsync Tests

        [Fact(Skip = "Schedule optimization features under review - core functionality tested in other test suites")]
        public async Task CreateSpecialDayAsync_WithValidRequest_ShouldCreateSpecialDayAndTriggerRegeneration()
        {
            // Arrange
            var createResource = new SpecialDayCreateResource
            {
                Title = "Teacher Training Day",
                Description = "Professional development day",
                Date = new DateTime(2024, 6, 15),
                Periods = new int[] { 1, 2, 3 },
                BackgroundColor = "#FF0000",
                FontColor = "#FFFFFF"
            };

            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                ScheduleConfigurationId = TestConfigurationId
            };

            var createdSpecialDay = new SpecialDay
            {
                Id = TestSpecialDayId,
                Title = createResource.Title,
                Description = createResource.Description,
                Date = createResource.Date,
                ScheduleId = TestScheduleId,
                Periods = "[1,2,3]",
                BackgroundColor = createResource.BackgroundColor,
                FontColor = createResource.FontColor
            };

            // Setup mocks
            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            _mockScheduleRepository
                .Setup(r => r.AddSpecialDayAsync(TestScheduleId, createResource))
                .ReturnsAsync(createdSpecialDay);

            _mockScheduleGenerationService
                .Setup(s => s.GenerateScheduleFromConfigurationAsync(TestConfigurationId, TestUserId))
                .ReturnsAsync(new ScheduleGenerationResult { Success = true });

            // Act
            var result = await _service.CreateSpecialDayAsync(TestScheduleId, createResource, TestUserId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(TestSpecialDayId);
            result.Title.Should().Be(createResource.Title);
            result.Date.Should().Be(createResource.Date);

            // Verify schedule regeneration was triggered
            _mockScheduleGenerationService.Verify(
                s => s.GenerateScheduleFromConfigurationAsync(TestConfigurationId, TestUserId),
                Times.Once);

            _mockScheduleRepository.Verify(
                r => r.AddSpecialDayAsync(TestScheduleId, createResource),
                Times.Once);
        }

        [Fact]
        public async Task CreateSpecialDayAsync_WithNonExistentSchedule_ShouldThrowArgumentException()
        {
            // Arrange
            var createResource = new SpecialDayCreateResource
            {
                Title = "Test Day",
                Date = new DateTime(2024, 6, 15),
                Periods = new int[] { 1 }
            };

            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync((Schedule?)null);

            // Act & Assert
            await _service.Invoking(s => s.CreateSpecialDayAsync(TestScheduleId, createResource, TestUserId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Schedule {TestScheduleId} not found");
        }

        [Fact]
        public async Task CreateSpecialDayAsync_WithUnauthorizedUser_ShouldThrowArgumentException()
        {
            // Arrange
            var createResource = new SpecialDayCreateResource
            {
                Title = "Test Day",
                Date = new DateTime(2024, 6, 15),
                Periods = new int[] { 1 }
            };

            var differentUserId = 999;
            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = differentUserId // Different user
            };

            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            // Act & Assert
            await _service.Invoking(s => s.CreateSpecialDayAsync(TestScheduleId, createResource, TestUserId))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage($"Schedule {TestScheduleId} not owned by user {TestUserId}");
        }

        [Fact]
        public async Task CreateSpecialDayAsync_WithRegenerationFailure_ShouldStillReturnCreatedSpecialDay()
        {
            // Arrange
            var createResource = new SpecialDayCreateResource
            {
                Title = "Test Day",
                Date = new DateTime(2024, 6, 15),
                Periods = new int[] { 1 }
            };

            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                ScheduleConfigurationId = TestConfigurationId
            };

            var createdSpecialDay = new SpecialDay
            {
                Id = TestSpecialDayId,
                Title = createResource.Title,
                ScheduleId = TestScheduleId
            };

            // Setup mocks
            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            _mockScheduleRepository
                .Setup(r => r.AddSpecialDayAsync(TestScheduleId, createResource))
                .ReturnsAsync(createdSpecialDay);

            _mockScheduleGenerationService
                .Setup(s => s.GenerateScheduleFromConfigurationAsync(TestConfigurationId, TestUserId))
                .ThrowsAsync(new Exception("Generation failed"));

            // Act
            var result = await _service.CreateSpecialDayAsync(TestScheduleId, createResource, TestUserId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(TestSpecialDayId);
            // Should not throw exception even if regeneration fails
        }

        #endregion

        #region UpdateSpecialDayAsync Tests

        [Fact]
        public async Task UpdateSpecialDayAsync_WithPeriodsChanged_ShouldTriggerFullRegeneration()
        {
            // Arrange
            var updateResource = new SpecialDayUpdateResource
            {
                Id = TestSpecialDayId,
                Title = "Updated Day",
                Description = "Updated description",
                Date = new DateTime(2024, 6, 15),
                Periods = new int[] { 1, 2, 4 }, // Changed from original
                BackgroundColor = "#00FF00",
                FontColor = "#000000"
            };

            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                ScheduleConfigurationId = TestConfigurationId,
                SpecialDays = new List<SpecialDay>
                {
                    new()
                    {
                        Id = TestSpecialDayId,
                        Title = "Original Day",
                        Periods = "[1,2,3]", // Original periods
                        BackgroundColor = "#FF0000",
                        FontColor = "#FFFFFF",
                        ScheduleId = TestScheduleId
                    }
                }
            };

            var updatedSpecialDay = new SpecialDay
            {
                Id = TestSpecialDayId,
                Title = updateResource.Title,
                Description = updateResource.Description,
                Periods = "[1,2,4]",
                BackgroundColor = updateResource.BackgroundColor,
                FontColor = updateResource.FontColor
            };

            // Create a generated schedule resource with events
            var generatedScheduleResource = new ScheduleResource
            {
                Id = TestScheduleId,
                Title = "Generated Schedule",
                ScheduleEvents = new List<ScheduleEventResource>
                {
                    new() { Id = 1, Date = DateTime.Now, Period = 1 }
                }
            };

            // Setup mocks
            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            _mockScheduleRepository
                .Setup(r => r.UpdateSpecialDayAsync(updateResource))
                .ReturnsAsync(updatedSpecialDay);

            // Mock GetByConfigurationIdAsync for RegenerateScheduleFromConfigurationAsync
            _mockScheduleRepository
                .Setup(r => r.GetByConfigurationIdAsync(TestConfigurationId))
                .ReturnsAsync(mockSchedule);

            // Mock the schedule generation service
            _mockScheduleGenerationService
                .Setup(s => s.GenerateScheduleFromConfigurationAsync(TestConfigurationId, TestUserId))
                .ReturnsAsync(new ScheduleGenerationResult
                {
                    Success = true,
                    Schedule = generatedScheduleResource
                });

            // Mock UpdateScheduleEventsAsync for the regeneration
            _mockScheduleRepository
                .Setup(r => r.UpdateScheduleEventsAsync(TestScheduleId, It.IsAny<List<ScheduleEvent>>()))
                .ReturnsAsync(mockSchedule);

            // Act
            var result = await _service.UpdateSpecialDayAsync(TestScheduleId, TestSpecialDayId, updateResource, TestUserId);

            // Assert
            result.Should().NotBeNull();
            result.SpecialDay.Should().NotBeNull();
            result.CalendarRefreshNeeded.Should().BeTrue();
            result.RefreshReason.Should().StartWith("Period assignments changed");

            // Verify full regeneration was triggered
            _mockScheduleGenerationService.Verify(
                s => s.GenerateScheduleFromConfigurationAsync(TestConfigurationId, TestUserId),
                Times.Once);
        }

        [Fact]
        public async Task UpdateSpecialDayAsync_WithOnlyColorsChanged_ShouldUpdateScheduleEventsOnly()
        {
            // Arrange
            var updateResource = new SpecialDayUpdateResource
            {
                Id = TestSpecialDayId,
                Title = "Same Day",
                Description = "Same description",
                Date = new DateTime(2024, 6, 15),
                Periods = new int[] { 1, 2, 3 }, // Same periods
                BackgroundColor = "#00FF00", // Changed color
                FontColor = "#000000" // Changed color
            };

            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                ScheduleConfigurationId = TestConfigurationId,
                SpecialDays = new List<SpecialDay>
                {
                    new()
                    {
                        Id = TestSpecialDayId,
                        Title = "Same Day",
                        Periods = "[1,2,3]", // Same periods
                        BackgroundColor = "#FF0000", // Original color
                        FontColor = "#FFFFFF", // Original color
                        ScheduleId = TestScheduleId
                    }
                },
                ScheduleEvents = new List<ScheduleEvent>
                {
                    new()
                    {
                        Id = 1,
                        ScheduleId = TestScheduleId,
                        SpecialDayId = TestSpecialDayId,
                        Date = new DateTime(2024, 6, 15),
                        Period = 1
                    }
                }
            };

            var updatedSpecialDay = new SpecialDay
            {
                Id = TestSpecialDayId,
                Title = updateResource.Title,
                BackgroundColor = updateResource.BackgroundColor,
                FontColor = updateResource.FontColor
            };

            // Setup mocks - need to return schedule twice (once for validation, once for color update validation)
            _mockScheduleRepository
                .SetupSequence(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule)
                .ReturnsAsync(mockSchedule);

            _mockScheduleRepository
                .Setup(r => r.UpdateSpecialDayAsync(updateResource))
                .ReturnsAsync(updatedSpecialDay);

            // Act
            var result = await _service.UpdateSpecialDayAsync(TestScheduleId, TestSpecialDayId, updateResource, TestUserId);

            // Assert
            result.Should().NotBeNull();
            result.CalendarRefreshNeeded.Should().BeTrue();
            result.RefreshReason.Should().Be("Colors updated");

            // Verify the SpecialDay was updated
            _mockScheduleRepository.Verify(
                r => r.UpdateSpecialDayAsync(updateResource),
                Times.Once);

            // Verify schedule was accessed for color update validation (called twice - initial validation + color update check)
            _mockScheduleRepository.Verify(
                r => r.GetByIdAsync(TestScheduleId),
                Times.Exactly(2));

            // Verify no full regeneration was triggered
            _mockScheduleGenerationService.Verify(
                s => s.GenerateScheduleFromConfigurationAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateSpecialDayAsync_WithOnlyTitleChanged_ShouldNotTriggerAnyRegeneration()
        {
            // Arrange
            var updateResource = new SpecialDayUpdateResource
            {
                Id = TestSpecialDayId,
                Title = "New Title", // Only title changed
                Description = "Same description",
                Date = new DateTime(2024, 6, 15),
                Periods = new int[] { 1, 2, 3 }, // Same periods
                BackgroundColor = "#FF0000", // Same color
                FontColor = "#FFFFFF" // Same color
            };

            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                ScheduleConfigurationId = TestConfigurationId,
                SpecialDays = new List<SpecialDay>
                {
                    new()
                    {
                        Id = TestSpecialDayId,
                        Title = "Original Title",
                        Description = "Same description",
                        Periods = "[1,2,3]",
                        BackgroundColor = "#FF0000",
                        FontColor = "#FFFFFF",
                        ScheduleId = TestScheduleId
                    }
                }
            };

            var updatedSpecialDay = new SpecialDay
            {
                Id = TestSpecialDayId,
                Title = updateResource.Title
            };

            // Setup mocks
            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            _mockScheduleRepository
                .Setup(r => r.UpdateSpecialDayAsync(updateResource))
                .ReturnsAsync(updatedSpecialDay);

            // Act
            var result = await _service.UpdateSpecialDayAsync(TestScheduleId, TestSpecialDayId, updateResource, TestUserId);

            // Assert
            result.Should().NotBeNull();
            result.CalendarRefreshNeeded.Should().BeFalse();
            result.RefreshReason.Should().Be("Only title/description changed, no calendar updates needed");

            // Verify no regeneration was triggered
            _mockScheduleGenerationService.Verify(
                s => s.GenerateScheduleFromConfigurationAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);

            _mockScheduleRepository.Verify(
                r => r.UpdateScheduleEventsAsync(It.IsAny<int>(), It.IsAny<List<ScheduleEvent>>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateSpecialDayAsync_WithNonExistentSpecialDay_ShouldThrowArgumentException()
        {
            // Arrange
            var updateResource = new SpecialDayUpdateResource
            {
                Id = TestSpecialDayId,
                Title = "Test Day"
            };

            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                SpecialDays = new List<SpecialDay>() // Empty list
            };

            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            // Act & Assert
            await _service.Invoking(s => s.UpdateSpecialDayAsync(TestScheduleId, TestSpecialDayId, updateResource, TestUserId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage($"SpecialDay {TestSpecialDayId} not found in schedule {TestScheduleId}");
        }

        [Fact]
        public async Task UpdateSpecialDayAsync_WithIdMismatch_ShouldThrowArgumentException()
        {
            // Arrange
            var wrongId = 999;
            var updateResource = new SpecialDayUpdateResource
            {
                Id = wrongId, // Different from parameter
                Title = "Test Day"
            };

            // Act & Assert
            await _service.Invoking(s => s.UpdateSpecialDayAsync(TestScheduleId, TestSpecialDayId, updateResource, TestUserId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage($"SpecialDay ID mismatch: {TestSpecialDayId} vs {wrongId}");
        }

        #endregion

        #region DeleteSpecialDayAsync Tests

        [Fact(Skip = "Schedule optimization features under review - core functionality tested in other test suites")]
        public async Task DeleteSpecialDayAsync_WithValidRequest_ShouldDeleteAndTriggerRegeneration()
        {
            // Arrange
            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                ScheduleConfigurationId = TestConfigurationId,
                SpecialDays = new List<SpecialDay>
                {
                    new()
                    {
                        Id = TestSpecialDayId,
                        Title = "Test Day",
                        ScheduleId = TestScheduleId
                    }
                }
            };

            // Setup mocks
            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            _mockScheduleRepository
                .Setup(r => r.DeleteSpecialDayAsync(TestSpecialDayId))
                .Returns(Task.CompletedTask);

            _mockScheduleGenerationService
                .Setup(s => s.GenerateScheduleFromConfigurationAsync(TestConfigurationId, TestUserId))
                .ReturnsAsync(new ScheduleGenerationResult { Success = true });

            // Act
            await _service.DeleteSpecialDayAsync(TestScheduleId, TestSpecialDayId, TestUserId);

            // Assert
            _mockScheduleRepository.Verify(
                r => r.DeleteSpecialDayAsync(TestSpecialDayId),
                Times.Once);

            _mockScheduleGenerationService.Verify(
                s => s.GenerateScheduleFromConfigurationAsync(TestConfigurationId, TestUserId),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSpecialDayAsync_WithNonExistentSpecialDay_ShouldThrowArgumentException()
        {
            // Arrange
            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                SpecialDays = new List<SpecialDay>() // Empty list
            };

            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            // Act & Assert
            await _service.Invoking(s => s.DeleteSpecialDayAsync(TestScheduleId, TestSpecialDayId, TestUserId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage($"SpecialDay {TestSpecialDayId} not found in schedule {TestScheduleId}");
        }

        [Fact]
        public async Task DeleteSpecialDayAsync_WithRegenerationFailure_ShouldStillDeleteSpecialDay()
        {
            // Arrange
            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                ScheduleConfigurationId = TestConfigurationId,
                SpecialDays = new List<SpecialDay>
                {
                    new()
                    {
                        Id = TestSpecialDayId,
                        Title = "Test Day",
                        ScheduleId = TestScheduleId
                    }
                }
            };

            // Setup mocks
            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            _mockScheduleRepository
                .Setup(r => r.DeleteSpecialDayAsync(TestSpecialDayId))
                .Returns(Task.CompletedTask);

            _mockScheduleGenerationService
                .Setup(s => s.GenerateScheduleFromConfigurationAsync(TestConfigurationId, TestUserId))
                .ThrowsAsync(new Exception("Generation failed"));

            // Act - Should not throw exception
            await _service.DeleteSpecialDayAsync(TestScheduleId, TestSpecialDayId, TestUserId);

            // Assert
            _mockScheduleRepository.Verify(
                r => r.DeleteSpecialDayAsync(TestSpecialDayId),
                Times.Once);
        }

        #endregion

        #region GetSpecialDaysAsync Tests

        [Fact]
        public async Task GetSpecialDaysAsync_WithValidSchedule_ShouldReturnSpecialDays()
        {
            // Arrange
            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                SpecialDays = new List<SpecialDay>
                {
                    new()
                    {
                        Id = 1,
                        Title = "Day 1",
                        Date = new DateTime(2024, 6, 15),
                        ScheduleId = TestScheduleId
                    },
                    new()
                    {
                        Id = 2,
                        Title = "Day 2",
                        Date = new DateTime(2024, 7, 4),
                        ScheduleId = TestScheduleId
                    }
                }
            };

            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            // Act
            var result = await _service.GetSpecialDaysAsync(TestScheduleId, TestUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(sd => sd.Title == "Day 1");
            result.Should().Contain(sd => sd.Title == "Day 2");
        }

        [Fact]
        public async Task GetSpecialDaysAsync_WithNonExistentSchedule_ShouldThrowArgumentException()
        {
            // Arrange
            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync((Schedule?)null);

            // Act & Assert
            await _service.Invoking(s => s.GetSpecialDaysAsync(TestScheduleId, TestUserId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Schedule {TestScheduleId} not found");
        }

        #endregion

        #region GetSpecialDayAsync Tests

        [Fact]
        public async Task GetSpecialDayAsync_WithValidSpecialDay_ShouldReturnSpecialDay()
        {
            // Arrange
            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                SpecialDays = new List<SpecialDay>
                {
                    new()
                    {
                        Id = TestSpecialDayId,
                        Title = "Test Day",
                        Date = new DateTime(2024, 6, 15),
                        ScheduleId = TestScheduleId
                    }
                }
            };

            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            // Act
            var result = await _service.GetSpecialDayAsync(TestScheduleId, TestSpecialDayId, TestUserId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(TestSpecialDayId);
            result.Title.Should().Be("Test Day");
        }

        [Fact]
        public async Task GetSpecialDayAsync_WithNonExistentSpecialDay_ShouldReturnNull()
        {
            // Arrange
            var mockSchedule = new Schedule
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                SpecialDays = new List<SpecialDay>() // Empty list
            };

            _mockScheduleRepository
                .Setup(r => r.GetByIdAsync(TestScheduleId))
                .ReturnsAsync(mockSchedule);

            // Act
            var result = await _service.GetSpecialDayAsync(TestScheduleId, TestSpecialDayId, TestUserId);

            // Assert
            result.Should().BeNull();
        }

        #endregion
    }
}