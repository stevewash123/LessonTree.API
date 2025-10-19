using LessonTree.API.Controllers;
using LessonTree.BLL.Services;
using LessonTree.Models.DTO;
using LessonTree.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LessonTree.Tests.Controllers
{
    /// <summary>
    /// Unit tests for ScheduleController calendar optimization features
    /// Tests new calendar date range endpoints, sequence analysis, and continuation features
    /// </summary>
    public class ScheduleControllerCalendarOptimizationTests : TestBase
    {
        private readonly Mock<IScheduleService> _mockScheduleService;
        private readonly ScheduleController _controller;
        private const int TestUserId = 1;
        private const int TestScheduleId = 100;

        public ScheduleControllerCalendarOptimizationTests()
        {
            _mockScheduleService = new Mock<IScheduleService>();
            var logger = CreateLogger<ScheduleController>();

            _controller = new ScheduleController(_mockScheduleService.Object, logger);

            // Setup controller context with authenticated user
            SetupControllerContext();
        }

        private void SetupControllerContext()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, TestUserId.ToString()),
                new("UserId", TestUserId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        #region GetEventsByDateRange Tests

        [Fact]
        public async Task GetEventsByDateRange_WithValidDateRange_ShouldReturnEvents()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 31);
            var courseId = 5;

            var mockSchedule = new ScheduleResource { Id = TestScheduleId, UserId = TestUserId };
            var mockEvents = new List<ScheduleEventResource>
            {
                new() { Id = 1, Date = new DateTime(2024, 1, 5), EventType = "Lesson", Period = 1 },
                new() { Id = 2, Date = new DateTime(2024, 1, 10), EventType = "SpecialDay", Period = 2 },
                new() { Id = 3, Date = new DateTime(2024, 1, 15), EventType = "Lesson", Period = 1 }
            };

            _mockScheduleService
                .Setup(s => s.GetByIdAsync(TestScheduleId, TestUserId))
                .ReturnsAsync(mockSchedule);

            _mockScheduleService
                .Setup(s => s.GetEventsByDateRangeAsync(TestScheduleId, startDate, endDate, TestUserId, courseId))
                .ReturnsAsync(mockEvents);

            // Act
            var result = await _controller.GetEventsByDateRange(TestScheduleId, startDate, endDate, courseId);

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var events = okResult.Value.Should().BeAssignableTo<List<ScheduleEventResource>>().Subject;
            events.Should().HaveCount(3);
            events.Should().Contain(e => e.EventType == "Lesson");
            events.Should().Contain(e => e.EventType == "SpecialDay");
        }

        [Fact]
        public async Task GetEventsByDateRange_WithNonExistentSchedule_ShouldReturnNotFound()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 31);

            _mockScheduleService
                .Setup(s => s.GetByIdAsync(TestScheduleId, TestUserId))
                .ReturnsAsync((ScheduleResource?)null);

            // Act
            var result = await _controller.GetEventsByDateRange(TestScheduleId, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be($"Schedule {TestScheduleId} not found or not accessible");
        }

        [Fact]
        public async Task GetEventsByDateRange_WithServiceException_ShouldReturnInternalServerError()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 31);

            _mockScheduleService
                .Setup(s => s.GetByIdAsync(TestScheduleId, TestUserId))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.GetEventsByDateRange(TestScheduleId, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetEventsByDateRange_WithoutCourseFilter_ShouldReturnAllEvents()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 31);

            var mockSchedule = new ScheduleResource { Id = TestScheduleId, UserId = TestUserId };
            var mockEvents = new List<ScheduleEventResource>
            {
                new() { Id = 1, Date = new DateTime(2024, 1, 5), EventType = "Lesson", Period = 1, CourseId = 1 },
                new() { Id = 2, Date = new DateTime(2024, 1, 10), EventType = "Lesson", Period = 2, CourseId = 2 },
                new() { Id = 3, Date = new DateTime(2024, 1, 15), EventType = "SpecialDay", Period = 3 }
            };

            _mockScheduleService
                .Setup(s => s.GetByIdAsync(TestScheduleId, TestUserId))
                .ReturnsAsync(mockSchedule);

            _mockScheduleService
                .Setup(s => s.GetEventsByDateRangeAsync(TestScheduleId, startDate, endDate, TestUserId, null))
                .ReturnsAsync(mockEvents);

            // Act
            var result = await _controller.GetEventsByDateRange(TestScheduleId, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var events = okResult.Value.Should().BeAssignableTo<List<ScheduleEventResource>>().Subject;
            events.Should().HaveCount(3);
            events.Should().Contain(e => e.CourseId == 1);
            events.Should().Contain(e => e.CourseId == 2);
            events.Should().Contain(e => e.EventType == "SpecialDay");
        }

        #endregion

        #region GetWeekEvents Tests

        [Fact]
        public async Task GetWeekEvents_WithValidWeekStart_ShouldReturnWeekEvents()
        {
            // Arrange
            var weekStartDate = new DateTime(2024, 1, 1); // Monday
            var weekEndDate = weekStartDate.AddDays(6); // Sunday

            var mockSchedule = new ScheduleResource { Id = TestScheduleId, UserId = TestUserId };
            var mockEvents = new List<ScheduleEventResource>
            {
                new() { Id = 1, Date = new DateTime(2024, 1, 1), EventType = "Lesson", Period = 1 },
                new() { Id = 2, Date = new DateTime(2024, 1, 3), EventType = "Lesson", Period = 2 },
                new() { Id = 3, Date = new DateTime(2024, 1, 5), EventType = "SpecialDay", Period = 1 }
            };

            _mockScheduleService
                .Setup(s => s.GetByIdAsync(TestScheduleId, TestUserId))
                .ReturnsAsync(mockSchedule);

            _mockScheduleService
                .Setup(s => s.GetEventsByDateRangeAsync(TestScheduleId, weekStartDate, weekEndDate, TestUserId, null))
                .ReturnsAsync(mockEvents);

            // Act
            var result = await _controller.GetWeekEvents(TestScheduleId, weekStartDate);

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var events = okResult.Value.Should().BeAssignableTo<List<ScheduleEventResource>>().Subject;
            events.Should().HaveCount(3);
            events.All(e => e.Date >= weekStartDate && e.Date <= weekEndDate).Should().BeTrue();
        }

        [Fact]
        public async Task GetWeekEvents_WithServiceException_ShouldReturnInternalServerError()
        {
            // Arrange
            var weekStartDate = new DateTime(2024, 1, 1);

            _mockScheduleService
                .Setup(s => s.GetByIdAsync(TestScheduleId, TestUserId))
                .ThrowsAsync(new Exception("Service unavailable"));

            // Act
            var result = await _controller.GetWeekEvents(TestScheduleId, weekStartDate);

            // Assert
            result.Should().NotBeNull();
            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        #endregion

        #region GetMonthEvents Tests

        [Fact]
        public async Task GetMonthEvents_WithValidYearAndMonth_ShouldReturnMonthEvents()
        {
            // Arrange
            var year = 2024;
            var month = 3;
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var mockSchedule = new ScheduleResource { Id = TestScheduleId, UserId = TestUserId };
            var mockEvents = new List<ScheduleEventResource>
            {
                new() { Id = 1, Date = new DateTime(2024, 3, 5), EventType = "Lesson", Period = 1 },
                new() { Id = 2, Date = new DateTime(2024, 3, 15), EventType = "SpecialDay", Period = 2 },
                new() { Id = 3, Date = new DateTime(2024, 3, 25), EventType = "Lesson", Period = 3 }
            };

            _mockScheduleService
                .Setup(s => s.GetByIdAsync(TestScheduleId, TestUserId))
                .ReturnsAsync(mockSchedule);

            _mockScheduleService
                .Setup(s => s.GetEventsByDateRangeAsync(TestScheduleId, monthStart, monthEnd, TestUserId, null))
                .ReturnsAsync(mockEvents);

            // Act
            var result = await _controller.GetMonthEvents(TestScheduleId, year, month);

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var events = okResult.Value.Should().BeAssignableTo<List<ScheduleEventResource>>().Subject;
            events.Should().HaveCount(3);
            events.All(e => e.Date.Year == year && e.Date.Month == month).Should().BeTrue();
        }

        [Fact]
        public async Task GetMonthEvents_WithInvalidMonth_ShouldHandleException()
        {
            // Arrange
            var year = 2024;
            var month = 13; // Invalid month

            // Act & Assert - DateTime constructor should throw for invalid month
            await _controller.Invoking(c => c.GetMonthEvents(TestScheduleId, year, month))
                .Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        #endregion

        #region AnalyzeSequences Tests

        [Fact]
        public async Task AnalyzeSequences_WithValidScheduleAndDate_ShouldReturnAnalysis()
        {
            // Arrange
            var afterDate = new DateTime(2024, 6, 1);
            var mockAnalysis = new SequenceAnalysisResult
            {
                TotalCoursesInScope = 3,
                TotalLessonsInScope = 45,
                ContinuationPoints = new List<ContinuationPoint>
                {
                    new()
                    {
                        Period = 1,
                        CourseId = 1,
                        CourseTitle = "Mathematics",
                        LastAssignedLessonIndex = 15,
                        TotalLessons = 30,
                        RemainingLessons = 15,
                        ContinuationDate = new DateTime(2024, 6, 15)
                    }
                }
            };

            _mockScheduleService
                .Setup(s => s.AnalyzeSequenceStateAsync(TestScheduleId, afterDate, TestUserId))
                .ReturnsAsync(mockAnalysis);

            // Act
            var result = await _controller.AnalyzeSequences(TestScheduleId, afterDate);

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var analysis = okResult.Value.Should().BeAssignableTo<SequenceAnalysisResult>().Subject;
            analysis.TotalCoursesInScope.Should().Be(3);
            analysis.TotalLessonsInScope.Should().Be(45);
            analysis.ContinuationPoints.Should().HaveCount(1);
            analysis.ContinuationPoints.First().CourseTitle.Should().Be("Mathematics");
        }

        [Fact]
        public async Task AnalyzeSequences_WithNonExistentSchedule_ShouldReturnNotFound()
        {
            // Arrange
            var afterDate = new DateTime(2024, 6, 1);

            _mockScheduleService
                .Setup(s => s.AnalyzeSequenceStateAsync(TestScheduleId, afterDate, TestUserId))
                .ThrowsAsync(new ArgumentException("Schedule not found"));

            // Act
            var result = await _controller.AnalyzeSequences(TestScheduleId, afterDate);

            // Assert
            result.Should().NotBeNull();
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        }

        [Fact]
        public async Task AnalyzeSequences_WithServiceException_ShouldReturnInternalServerError()
        {
            // Arrange
            var afterDate = new DateTime(2024, 6, 1);

            _mockScheduleService
                .Setup(s => s.AnalyzeSequenceStateAsync(TestScheduleId, afterDate, TestUserId))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.AnalyzeSequences(TestScheduleId, afterDate);

            // Assert
            result.Should().NotBeNull();
            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        #endregion

        #region ContinueSequences Tests

        [Fact]
        public async Task ContinueSequences_WithValidRequest_ShouldReturnUpdatedSchedule()
        {
            // Arrange
            var continuationRequest = new SequenceContinuationRequest
            {
                AfterDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 12, 31),
                SpecificCourseIds = new List<int> { 1, 2 },
                SpecificPeriods = new List<int> { 1, 2 },
                SkipCompletedCourses = true,
                MaxEventsToGenerate = 100
            };

            var mockUpdatedSchedule = new ScheduleResource
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                ScheduleEvents = new List<ScheduleEventResource>
                {
                    new() { Id = 1, Date = new DateTime(2024, 6, 5), EventType = "Lesson", Period = 1 },
                    new() { Id = 2, Date = new DateTime(2024, 6, 10), EventType = "Lesson", Period = 2 }
                }
            };

            _mockScheduleService
                .Setup(s => s.ContinueSequencesAsync(TestScheduleId, continuationRequest, TestUserId))
                .ReturnsAsync(mockUpdatedSchedule);

            // Act
            var result = await _controller.ContinueSequences(TestScheduleId, continuationRequest);

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var schedule = okResult.Value.Should().BeAssignableTo<ScheduleResource>().Subject;
            schedule.Id.Should().Be(TestScheduleId);
            schedule.ScheduleEvents.Should().HaveCount(2);
        }

        [Fact]
        public async Task ContinueSequences_WithInvalidSchedule_ShouldReturnNotFound()
        {
            // Arrange
            var continuationRequest = new SequenceContinuationRequest
            {
                AfterDate = new DateTime(2024, 6, 1)
            };

            _mockScheduleService
                .Setup(s => s.ContinueSequencesAsync(TestScheduleId, continuationRequest, TestUserId))
                .ThrowsAsync(new ArgumentException("Schedule not found"));

            // Act
            var result = await _controller.ContinueSequences(TestScheduleId, continuationRequest);

            // Assert
            result.Should().NotBeNull();
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        }

        [Fact]
        public async Task ContinueSequences_WithNullRequest_ShouldThrowArgumentException()
        {
            // Arrange
            SequenceContinuationRequest? nullRequest = null;

            // Act & Assert
            await _controller.Invoking(c => c.ContinueSequences(TestScheduleId, nullRequest!))
                .Should().ThrowAsync<NullReferenceException>();
        }

        [Fact]
        public async Task ContinueSequences_WithServiceException_ShouldReturnInternalServerError()
        {
            // Arrange
            var continuationRequest = new SequenceContinuationRequest
            {
                AfterDate = new DateTime(2024, 6, 1)
            };

            _mockScheduleService
                .Setup(s => s.ContinueSequencesAsync(TestScheduleId, continuationRequest, TestUserId))
                .ThrowsAsync(new Exception("Service unavailable"));

            // Act
            var result = await _controller.ContinueSequences(TestScheduleId, continuationRequest);

            // Assert
            result.Should().NotBeNull();
            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        #endregion

        #region GetScheduleStatistics Tests

        [Fact]
        public async Task GetScheduleStatistics_WithValidSchedule_ShouldReturnStatistics()
        {
            // Arrange
            var mockSchedule = new ScheduleResource
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                ScheduleEvents = new List<ScheduleEventResource>
                {
                    new() { Id = 1, EventType = "Lesson", Period = 1, Date = new DateTime(2024, 1, 5), EventCategory = "Lesson" },
                    new() { Id = 2, EventType = "Lesson", Period = 2, Date = new DateTime(2024, 1, 10), EventCategory = "Lesson" },
                    new() { Id = 3, EventType = "SpecialDay", Period = 1, Date = new DateTime(2024, 1, 15), EventCategory = "SpecialDay" },
                    new() { Id = 4, EventType = "Error", Period = 3, Date = new DateTime(2024, 1, 20), EventCategory = "Error" }
                }
            };

            _mockScheduleService
                .Setup(s => s.GetByIdAsync(TestScheduleId, TestUserId))
                .ReturnsAsync(mockSchedule);

            // Act
            var result = await _controller.GetScheduleStatistics(TestScheduleId);

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var stats = okResult.Value.Should().BeAssignableTo<ScheduleStatistics>().Subject;

            stats.ScheduleId.Should().Be(TestScheduleId);
            stats.TotalEvents.Should().Be(4);
            stats.LessonEvents.Should().Be(2);
            stats.SpecialDayEvents.Should().Be(1);
            stats.ErrorEvents.Should().Be(1);
            stats.EventsByPeriod.Should().ContainKey(1).WhoseValue.Should().Be(2);
            stats.EventsByPeriod.Should().ContainKey(2).WhoseValue.Should().Be(1);
            stats.EventsByPeriod.Should().ContainKey(3).WhoseValue.Should().Be(1);
        }

        [Fact]
        public async Task GetScheduleStatistics_WithNonExistentSchedule_ShouldReturnNotFound()
        {
            // Arrange
            _mockScheduleService
                .Setup(s => s.GetByIdAsync(TestScheduleId, TestUserId))
                .ReturnsAsync((ScheduleResource?)null);

            // Act
            var result = await _controller.GetScheduleStatistics(TestScheduleId);

            // Assert
            result.Should().NotBeNull();
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be($"Schedule {TestScheduleId} not found or not accessible");
        }

        [Fact]
        public async Task GetScheduleStatistics_WithEmptySchedule_ShouldReturnZeroStatistics()
        {
            // Arrange
            var mockSchedule = new ScheduleResource
            {
                Id = TestScheduleId,
                UserId = TestUserId,
                ScheduleEvents = new List<ScheduleEventResource>()
            };

            _mockScheduleService
                .Setup(s => s.GetByIdAsync(TestScheduleId, TestUserId))
                .ReturnsAsync(mockSchedule);

            // Act
            var result = await _controller.GetScheduleStatistics(TestScheduleId);

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var stats = okResult.Value.Should().BeAssignableTo<ScheduleStatistics>().Subject;

            stats.TotalEvents.Should().Be(0);
            stats.LessonEvents.Should().Be(0);
            stats.SpecialDayEvents.Should().Be(0);
            stats.ErrorEvents.Should().Be(0);
        }

        #endregion
    }
}