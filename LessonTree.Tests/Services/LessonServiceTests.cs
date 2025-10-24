using AutoMapper;
using LessonTree.BLL.Service;
using LessonTree.BLL.Services;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using LessonTree.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;

namespace LessonTree.Tests.Services
{
    public class LessonServiceTests : TestBase
    {
        private readonly Mock<ILessonRepository> _mockLessonRepository;
        private readonly Mock<ITopicRepository> _mockTopicRepository;
        private readonly Mock<ISubTopicRepository> _mockSubTopicRepository;
        private readonly Mock<IStandardRepository> _mockStandardRepository;
        private readonly Mock<IScheduleService> _mockScheduleService;
        private readonly Mock<IScheduleConfigurationService> _mockScheduleConfigurationService;
        private readonly Mock<IScheduleGenerationService> _mockScheduleGenerationService;
        private readonly Mock<IBackgroundScheduleService> _mockBackgroundScheduleService;
        private readonly LessonService _service;

        public LessonServiceTests()
        {
            _mockLessonRepository = new Mock<ILessonRepository>();
            _mockTopicRepository = new Mock<ITopicRepository>();
            _mockSubTopicRepository = new Mock<ISubTopicRepository>();
            _mockStandardRepository = new Mock<IStandardRepository>();
            _mockScheduleService = new Mock<IScheduleService>();
            _mockScheduleConfigurationService = new Mock<IScheduleConfigurationService>();
            _mockScheduleGenerationService = new Mock<IScheduleGenerationService>();
            _mockBackgroundScheduleService = new Mock<IBackgroundScheduleService>();
            var logger = CreateLogger<LessonService>();

            _service = new LessonService(
                _mockLessonRepository.Object,
                _mockTopicRepository.Object,
                _mockSubTopicRepository.Object,
                _mockStandardRepository.Object,
                logger,
                Mapper,
                _mockScheduleService.Object,
                _mockScheduleConfigurationService.Object,
                _mockScheduleGenerationService.Object,
                _mockBackgroundScheduleService.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingLesson_ShouldReturnLessonDetailResource()
        {
            // Arrange
            const int lessonId = 1;
            const int userId = 1;
            var lesson = new Lesson
            {
                Id = lessonId,
                Title = "Test Lesson",
                Objective = "Test Objective",
                UserId = userId,
                TopicId = 1,
                SortOrder = 1
            };

            _mockLessonRepository
                .Setup(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .ReturnsAsync(lesson);

            // Act
            var result = await _service.GetByIdAsync(lessonId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(lessonId);
            result.Title.Should().Be("Test Lesson");
            result.Objective.Should().Be("Test Objective");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentLesson_ShouldReturnNull()
        {
            // Arrange
            const int lessonId = 999;
            const int userId = 1;

            _mockLessonRepository
                .Setup(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .ReturnsAsync((Lesson?)null);

            // Act
            var result = await _service.GetByIdAsync(lessonId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithDifferentUser_ShouldReturnNull()
        {
            // Arrange
            const int lessonId = 1;
            const int userId = 1;
            const int differentUserId = 2;

            _mockLessonRepository
                .Setup(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .ReturnsAsync(new Lesson { Id = lessonId, UserId = differentUserId });

            // Act
            var result = await _service.GetByIdAsync(lessonId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_WithActiveFilter_ShouldReturnActiveLessons()
        {
            // Arrange
            const int userId = 1;
            var lessons = CreateTestLessons(userId);
            var mockQueryable = lessons.AsQueryable().BuildMock();

            _mockLessonRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Active);

            // Assert
            result.Should().NotBeEmpty();
            result.All(l => l.UserId == userId).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_WithArchivedFilter_ShouldReturnArchivedLessons()
        {
            // Arrange
            const int userId = 1;
            var lessons = CreateTestLessons(userId);
            var mockQueryable = lessons.AsQueryable().BuildMock();

            _mockLessonRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Archived);

            // Assert
            result.Should().NotBeEmpty();
            result.All(l => l.UserId == userId).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_WithBothFilter_ShouldReturnAllLessons()
        {
            // Arrange
            const int userId = 1;
            var lessons = CreateTestLessons(userId);
            var mockQueryable = lessons.AsQueryable().BuildMock();

            _mockLessonRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Both);

            // Assert
            result.Should().NotBeEmpty();
            result.All(l => l.UserId == userId).Should().BeTrue();
        }

        [Fact]
        public async Task AddAsync_WithValidTopicLesson_ShouldCreateLesson()
        {
            // Arrange
            const int userId = 1;
            const int topicId = 1;
            var lessonCreateResource = new LessonCreateResource
            {
                Title = "New Lesson",
                Objective = "New Objective",
                TopicId = topicId
            };

            var topic = new Topic { Id = topicId, UserId = userId };
            var existingLessons = new List<Lesson>
            {
                new() { Id = 1, TopicId = topicId, SortOrder = 1 },
                new() { Id = 2, TopicId = topicId, SortOrder = 2 }
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync(topic);

            _mockLessonRepository
                .Setup(r => r.GetByTopicId(topicId, It.IsAny<bool>()))
                .Returns(existingLessons.AsQueryable().BuildMock());

            _mockLessonRepository
                .Setup(r => r.AddAsync(It.IsAny<Lesson>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.AddAsync(lessonCreateResource, userId);

            // Assert
            result.Should().BeGreaterThan(0);
            _mockLessonRepository.Verify(r => r.AddAsync(It.IsAny<Lesson>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_WithValidSubTopicLesson_ShouldCreateLesson()
        {
            // Arrange
            const int userId = 1;
            const int subTopicId = 1;
            var lessonCreateResource = new LessonCreateResource
            {
                Title = "New Lesson",
                Objective = "New Objective",
                SubTopicId = subTopicId
            };

            var subTopic = new SubTopic { Id = subTopicId, UserId = userId };
            var existingLessons = new List<Lesson>
            {
                new() { Id = 1, SubTopicId = subTopicId, SortOrder = 1 },
                new() { Id = 2, SubTopicId = subTopicId, SortOrder = 2 }
            };

            _mockSubTopicRepository
                .Setup(r => r.GetByIdAsync(subTopicId, It.IsAny<Func<IQueryable<SubTopic>, IQueryable<SubTopic>>>()))
                .ReturnsAsync(subTopic);

            _mockLessonRepository
                .Setup(r => r.GetBySubTopicId(subTopicId, It.IsAny<bool>()))
                .Returns(existingLessons.AsQueryable().BuildMock());

            _mockLessonRepository
                .Setup(r => r.AddAsync(It.IsAny<Lesson>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.AddAsync(lessonCreateResource, userId);

            // Assert
            result.Should().BeGreaterThan(0);
            _mockLessonRepository.Verify(r => r.AddAsync(It.IsAny<Lesson>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_WithInvalidTopic_ShouldThrowArgumentException()
        {
            // Arrange
            const int userId = 1;
            const int topicId = 999;
            var lessonCreateResource = new LessonCreateResource
            {
                Title = "New Lesson",
                TopicId = topicId
            };

            // Mock empty lessons for sort order calculation
            var emptyLessons = new List<Lesson>();
            _mockLessonRepository
                .Setup(r => r.GetByTopicId(topicId, It.IsAny<bool>()))
                .Returns(emptyLessons.AsQueryable().BuildMock());

            _mockLessonRepository
                .Setup(r => r.AddAsync(It.IsAny<Lesson>()))
                .ReturnsAsync(1);

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync((Topic?)null);

            // Act & Assert
            await _service.Invoking(s => s.AddAsync(lessonCreateResource, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Topic*not found*");
        }

        [Fact]
        public async Task UpdateAsync_WithValidLesson_ShouldUpdateLesson()
        {
            // Arrange
            const int lessonId = 1;
            const int userId = 1;

            var existingLesson = new Lesson
            {
                Id = lessonId,
                Title = "Old Title",
                Objective = "Old Objective",
                UserId = userId,
                TopicId = 1
            };

            var updateResource = new LessonUpdateResource
            {
                Id = lessonId,
                Title = "Updated Title",
                Objective = "Updated Objective"
            };

            _mockLessonRepository
                .Setup(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .ReturnsAsync(existingLesson);

            _mockLessonRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Lesson>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(updateResource, userId);

            // Assert
            result.Should().NotBeNull();
            _mockLessonRepository.Verify(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()), Times.Exactly(2));
            _mockLessonRepository.Verify(r => r.UpdateAsync(It.IsAny<Lesson>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentLesson_ShouldThrowArgumentException()
        {
            // Arrange
            const int lessonId = 999;
            const int userId = 1;

            var updateResource = new LessonUpdateResource
            {
                Id = lessonId,
                Title = "Updated Title"
            };

            _mockLessonRepository
                .Setup(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .ReturnsAsync((Lesson?)null);

            // Act & Assert
            await _service.Invoking(s => s.UpdateAsync(updateResource, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*not found*");

            _mockLessonRepository.Verify(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()), Times.Once);
            _mockLessonRepository.Verify(r => r.UpdateAsync(It.IsAny<Lesson>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            const int lessonId = 1;
            const int userId = 1;
            const int differentUserId = 2;

            var existingLesson = new Lesson
            {
                Id = lessonId,
                Title = "Test Lesson",
                Objective = "Test Objective",
                UserId = differentUserId
            };

            var updateResource = new LessonUpdateResource
            {
                Id = lessonId,
                Title = "Updated Title"
            };

            _mockLessonRepository
                .Setup(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .ReturnsAsync(existingLesson);

            // Act & Assert
            await _service.Invoking(s => s.UpdateAsync(updateResource, userId))
                .Should().ThrowAsync<UnauthorizedAccessException>();

            _mockLessonRepository.Verify(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()), Times.Once);
            _mockLessonRepository.Verify(r => r.UpdateAsync(It.IsAny<Lesson>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithValidLesson_ShouldDeleteLesson()
        {
            // Arrange
            const int lessonId = 1;
            const int userId = 1;

            var existingLesson = new Lesson
            {
                Id = lessonId,
                Title = "Test Lesson",
                Objective = "Test Objective",
                UserId = userId,
                TopicId = 1
            };

            _mockLessonRepository
                .Setup(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .ReturnsAsync(existingLesson);

            _mockLessonRepository
                .Setup(r => r.DeleteAsync(lessonId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(lessonId, userId);

            // Assert
            _mockLessonRepository.Verify(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()), Times.Once);
            _mockLessonRepository.Verify(r => r.DeleteAsync(lessonId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentLesson_ShouldThrowArgumentException()
        {
            // Arrange
            const int lessonId = 999;
            const int userId = 1;

            _mockLessonRepository
                .Setup(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .ReturnsAsync((Lesson?)null);

            // Act & Assert
            await _service.Invoking(s => s.DeleteAsync(lessonId, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*not found*");

            _mockLessonRepository.Verify(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()), Times.Once);
            _mockLessonRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            const int lessonId = 1;
            const int userId = 1;
            const int differentUserId = 2;

            var existingLesson = new Lesson
            {
                Id = lessonId,
                Title = "Test Lesson",
                Objective = "Test Objective",
                UserId = differentUserId
            };

            _mockLessonRepository
                .Setup(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()))
                .ReturnsAsync(existingLesson);

            // Act & Assert
            await _service.Invoking(s => s.DeleteAsync(lessonId, userId))
                .Should().ThrowAsync<UnauthorizedAccessException>();

            _mockLessonRepository.Verify(r => r.GetByIdAsync(lessonId, It.IsAny<Func<IQueryable<Lesson>, IQueryable<Lesson>>>()), Times.Once);
            _mockLessonRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        private static List<Lesson> CreateTestLessons(int userId)
        {
            return new List<Lesson>
            {
                new Lesson { Id = 1, Title = "Lesson 1", UserId = userId, TopicId = 1, Archived = false },
                new Lesson { Id = 2, Title = "Lesson 2", UserId = userId, TopicId = 1, Archived = true },
                new Lesson { Id = 3, Title = "Lesson 3", UserId = userId, TopicId = 2, Archived = false }
            };
        }
    }
}