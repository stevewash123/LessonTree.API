using AutoMapper;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using LessonTree.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.Tests.Services
{
    public class TopicServiceTests : TestBase
    {
        private readonly Mock<ITopicRepository> _mockTopicRepository;
        private readonly Mock<ICourseRepository> _mockCourseRepository;
        private readonly Mock<ISubTopicRepository> _mockSubTopicRepository;
        private readonly Mock<ILessonRepository> _mockLessonRepository;
        private readonly TopicService _service;

        public TopicServiceTests()
        {
            _mockTopicRepository = new Mock<ITopicRepository>();
            _mockCourseRepository = new Mock<ICourseRepository>();
            _mockSubTopicRepository = new Mock<ISubTopicRepository>();
            _mockLessonRepository = new Mock<ILessonRepository>();
            var logger = CreateLogger<TopicService>();

            _service = new TopicService(
                _mockTopicRepository.Object,
                _mockCourseRepository.Object,
                _mockSubTopicRepository.Object,
                _mockLessonRepository.Object,
                Mapper,
                logger);
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithExistingTopic_ShouldReturnTopicResource()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            var topic = new Topic
            {
                Id = topicId,
                Title = "Test Topic",
                Description = "Test Description",
                UserId = userId,
                CourseId = 1,
                SortOrder = 1,
                Visibility = VisibilityType.Private,
                SubTopics = new List<SubTopic>
                {
                    new SubTopic { Id = 1, Title = "SubTopic 1", UserId = userId }
                },
                Lessons = new List<Lesson>
                {
                    new Lesson { Id = 1, Title = "Lesson 1", UserId = userId }
                }
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync(topic);

            // Act
            var result = await _service.GetByIdAsync(topicId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(topicId);
            result.Title.Should().Be("Test Topic");
            result.Description.Should().Be("Test Description");
            result.UserId.Should().Be(userId);
            result.SubTopics.Should().HaveCount(1);
            result.Lessons.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentTopic_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            const int topicId = 999;
            const int userId = 1;

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync((Topic?)null);

            // Act & Assert
            await _service.Invoking(s => s.GetByIdAsync(topicId, userId))
                .Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*not found or not owned by user*");
        }

        [Fact]
        public async Task GetByIdAsync_WithDifferentUser_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            const int differentUserId = 2;

            var topic = new Topic
            {
                Id = topicId,
                Title = "Test Topic",
                UserId = differentUserId
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync(topic);

            // Act & Assert
            await _service.Invoking(s => s.GetByIdAsync(topicId, userId))
                .Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*not found or not owned by user*");
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithActiveFilter_ShouldReturnActiveTopics()
        {
            // Arrange
            const int userId = 1;
            var topics = CreateTestTopics(userId);
            
            var mockQuery = new TestAsyncEnumerable<Topic>(topics.AsQueryable());
            
            _mockTopicRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .Returns(mockQuery);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Active);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Only non-archived topics
            result.All(t => t.UserId == userId).Should().BeTrue();
            result.All(t => !t.Archived).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_WithArchivedFilter_ShouldReturnArchivedTopics()
        {
            // Arrange
            const int userId = 1;
            var topics = CreateTestTopics(userId);
            
            var mockQuery = new TestAsyncEnumerable<Topic>(topics.AsQueryable());
            
            _mockTopicRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .Returns(mockQuery);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Archived);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1); // Only archived topic
            result.All(t => t.UserId == userId).Should().BeTrue();
            result.All(t => t.Archived).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_WithBothFilter_ShouldReturnAllTopics()
        {
            // Arrange
            const int userId = 1;
            var topics = CreateTestTopics(userId);
            
            var mockQuery = new TestAsyncEnumerable<Topic>(topics.AsQueryable());
            
            _mockTopicRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .Returns(mockQuery);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Both);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3); // All topics
            result.All(t => t.UserId == userId).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_WithInvalidFilter_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            const int userId = 1;
            const ArchiveFilter invalidFilter = (ArchiveFilter)999;

            // Act & Assert
            await _service.Invoking(s => s.GetAllAsync(userId, invalidFilter))
                .Should().ThrowAsync<ArgumentOutOfRangeException>()
                .WithMessage("*Invalid filter value*");
        }

        #endregion

        #region GetTopicsByCourseAsync Tests

        [Fact]
        public async Task GetTopicsByCourseAsync_WithValidCourse_ShouldReturnSortedTopics()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            var topics = CreateTestTopics(userId).Where(t => t.CourseId == courseId).ToList();
            
            var mockQuery = new TestAsyncEnumerable<Topic>(topics.AsQueryable());
            
            _mockTopicRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .Returns(mockQuery);

            // Act
            var result = await _service.GetTopicsByCourseAsync(courseId, userId, ArchiveFilter.Active);

            // Assert
            result.Should().NotBeNull();
            result.All(t => t.CourseId == courseId).Should().BeTrue();
            result.All(t => t.UserId == userId).Should().BeTrue();
            
            // Verify sorting by SortOrder
            for (int i = 0; i < result.Count - 1; i++)
            {
                result[i].SortOrder.Should().BeLessOrEqualTo(result[i + 1].SortOrder);
            }
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithValidData_ShouldCreateTopic()
        {
            // Arrange
            const int userId = 1;
            const int courseId = 1;
            const int expectedSortOrder = 5;
            var topicCreateResource = new TopicCreateResource
            {
                Title = "New Topic",
                Description = "New Description",
                CourseId = courseId,
                Visibility = "Private"
            };

            _mockTopicRepository
                .Setup(r => r.GetNextSortOrderForCourseAsync(courseId))
                .ReturnsAsync(expectedSortOrder);

            _mockTopicRepository
                .Setup(r => r.AddAsync(It.IsAny<Topic>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.AddAsync(topicCreateResource, userId);

            // Assert
            result.Should().BeGreaterThan(0);
            _mockTopicRepository.Verify(r => r.GetNextSortOrderForCourseAsync(courseId), Times.Once);
            _mockTopicRepository.Verify(r => r.AddAsync(It.Is<Topic>(t => 
                t.Title == "New Topic" &&
                t.Description == "New Description" &&
                t.CourseId == courseId &&
                t.UserId == userId &&
                t.SortOrder == expectedSortOrder)), Times.Once);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidTopic_ShouldUpdateTopic()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;

            var existingTopic = new Topic
            {
                Id = topicId,
                Title = "Old Title",
                Description = "Old Description",
                UserId = userId,
                CourseId = 1,
                Visibility = VisibilityType.Private
            };

            var updateResource = new TopicUpdateResource
            {
                Id = topicId,
                Title = "Updated Title",
                Description = "Updated Description",
                Visibility = "Public",
                Archived = false
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, null))
                .ReturnsAsync(existingTopic);

            _mockTopicRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Topic>()))
                .Returns(Task.CompletedTask);

            // Setup for GetByIdAsync call in UpdateAsync return
            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync(existingTopic);

            // Act
            var result = await _service.UpdateAsync(updateResource, userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(topicId);
            _mockTopicRepository.Verify(r => r.UpdateAsync(It.IsAny<Topic>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentTopic_ShouldThrowArgumentException()
        {
            // Arrange
            const int topicId = 999;
            const int userId = 1;

            var updateResource = new TopicUpdateResource
            {
                Id = topicId,
                Title = "Updated Title"
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, null))
                .ReturnsAsync((Topic?)null);

            // Act & Assert
            await _service.Invoking(s => s.UpdateAsync(updateResource, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*not found*");

            _mockTopicRepository.Verify(r => r.UpdateAsync(It.IsAny<Topic>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            const int differentUserId = 2;

            var existingTopic = new Topic
            {
                Id = topicId,
                Title = "Test Topic",
                UserId = differentUserId
            };

            var updateResource = new TopicUpdateResource
            {
                Id = topicId,
                Title = "Updated Title"
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, null))
                .ReturnsAsync(existingTopic);

            // Act & Assert
            await _service.Invoking(s => s.UpdateAsync(updateResource, userId))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*not owned by user*");

            _mockTopicRepository.Verify(r => r.UpdateAsync(It.IsAny<Topic>()), Times.Never);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidTopic_ShouldDeleteTopic()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;

            var existingTopic = new Topic
            {
                Id = topicId,
                Title = "Test Topic",
                UserId = userId,
                SubTopics = new List<SubTopic>
                {
                    new SubTopic { Id = 1, Lessons = new List<Lesson> { new Lesson { Id = 1 } } }
                },
                Lessons = new List<Lesson>
                {
                    new Lesson { Id = 2 }
                }
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync(existingTopic);

            _mockTopicRepository
                .Setup(r => r.DeleteAsync(topicId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(topicId, userId);

            // Assert
            _mockTopicRepository.Verify(r => r.DeleteAsync(topicId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentTopic_ShouldThrowArgumentException()
        {
            // Arrange
            const int topicId = 999;
            const int userId = 1;

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync((Topic?)null);

            // Act & Assert
            await _service.Invoking(s => s.DeleteAsync(topicId, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*not found*");

            _mockTopicRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            const int differentUserId = 2;

            var existingTopic = new Topic
            {
                Id = topicId,
                Title = "Test Topic",
                UserId = differentUserId,
                SubTopics = new List<SubTopic>(),
                Lessons = new List<Lesson>()
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync(existingTopic);

            // Act & Assert
            await _service.Invoking(s => s.DeleteAsync(topicId, userId))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*not owned by user*");

            _mockTopicRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region UpdateSortOrderAsync Tests

        [Fact]
        public async Task UpdateSortOrderAsync_WithValidTopic_ShouldUpdateSortOrder()
        {
            // Arrange
            const int topicId = 1;
            const int newSortOrder = 10;

            var existingTopic = new Topic
            {
                Id = topicId,
                Title = "Test Topic",
                SortOrder = 5
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, null))
                .ReturnsAsync(existingTopic);

            _mockTopicRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Topic>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateSortOrderAsync(topicId, newSortOrder);

            // Assert
            existingTopic.SortOrder.Should().Be(newSortOrder);
            _mockTopicRepository.Verify(r => r.UpdateAsync(existingTopic), Times.Once);
        }

        [Fact]
        public async Task UpdateSortOrderAsync_WithNonExistentTopic_ShouldThrowArgumentException()
        {
            // Arrange
            const int topicId = 999;
            const int newSortOrder = 10;

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, null))
                .ReturnsAsync((Topic?)null);

            // Act & Assert
            await _service.Invoking(s => s.UpdateSortOrderAsync(topicId, newSortOrder))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Topic not found");

            _mockTopicRepository.Verify(r => r.UpdateAsync(It.IsAny<Topic>()), Times.Never);
        }

        #endregion

        #region MoveTopicAsync Tests

        [Fact]
        public async Task MoveTopicAsync_SimpleMove_ShouldMoveToFirstPosition()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            const int newCourseId = 2;

            var moveResource = new TopicMoveResource
            {
                TopicId = topicId,
                NewCourseId = newCourseId,
                AfterSiblingId = null
            };

            var existingTopic = new Topic
            {
                Id = topicId,
                Title = "Test Topic",
                UserId = userId,
                CourseId = 1,
                SortOrder = 5
            };

            var targetCourse = new Course
            {
                Id = newCourseId,
                UserId = userId
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, null))
                .ReturnsAsync(existingTopic);

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(newCourseId, null))
                .ReturnsAsync(targetCourse);

            _mockTopicRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Topic>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.MoveTopicAsync(moveResource, userId);

            // Assert
            result.Should().NotBeNull();
            result.CourseId.Should().Be(newCourseId);
            result.SortOrder.Should().Be(0); // First position
            _mockTopicRepository.Verify(r => r.UpdateAsync(It.Is<Topic>(t => 
                t.Id == topicId && 
                t.CourseId == newCourseId && 
                t.SortOrder == 0)), Times.Once);
        }

        [Fact]
        public async Task MoveTopicAsync_WithPositioning_ShouldDelegateToRepository()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            const int newCourseId = 2;
            const int afterSiblingId = 3;

            var moveResource = new TopicMoveResource
            {
                TopicId = topicId,
                NewCourseId = newCourseId,
                AfterSiblingId = afterSiblingId
            };

            var existingTopic = new Topic
            {
                Id = topicId,
                Title = "Test Topic",
                UserId = userId,
                CourseId = 1
            };

            var targetCourse = new Course
            {
                Id = newCourseId,
                UserId = userId
            };

            var siblingTopic = new Topic
            {
                Id = afterSiblingId,
                UserId = userId,
                CourseId = newCourseId
            };

            var positionedTopic = new Topic
            {
                Id = topicId,
                UserId = userId,
                CourseId = newCourseId,
                SortOrder = 10
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, null))
                .ReturnsAsync(existingTopic);

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(newCourseId, null))
                .ReturnsAsync(targetCourse);

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(afterSiblingId, null))
                .ReturnsAsync(siblingTopic);

            _mockTopicRepository
                .Setup(r => r.MoveTopicToPositionAsync(topicId, newCourseId, afterSiblingId))
                .ReturnsAsync(positionedTopic);

            // Act
            var result = await _service.MoveTopicAsync(moveResource, userId);

            // Assert
            result.Should().NotBeNull();
            result.CourseId.Should().Be(newCourseId);
            _mockTopicRepository.Verify(r => r.MoveTopicToPositionAsync(topicId, newCourseId, afterSiblingId), Times.Once);
        }

        [Fact]
        public async Task MoveTopicAsync_WithNonExistentTopic_ShouldThrowArgumentException()
        {
            // Arrange
            const int topicId = 999;
            const int userId = 1;
            const int newCourseId = 2;

            var moveResource = new TopicMoveResource
            {
                TopicId = topicId,
                NewCourseId = newCourseId
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, null))
                .ReturnsAsync((Topic?)null);

            // Act & Assert
            await _service.Invoking(s => s.MoveTopicAsync(moveResource, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task MoveTopicAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            const int differentUserId = 2;
            const int newCourseId = 2;

            var moveResource = new TopicMoveResource
            {
                TopicId = topicId,
                NewCourseId = newCourseId
            };

            var existingTopic = new Topic
            {
                Id = topicId,
                UserId = differentUserId
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, null))
                .ReturnsAsync(existingTopic);

            // Act & Assert
            await _service.Invoking(s => s.MoveTopicAsync(moveResource, userId))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*not owned by user*");
        }

        #endregion

        #region CopyTopicAsync Tests

        [Fact]
        public async Task CopyTopicAsync_WithValidTopic_ShouldCreateDeepCopy()
        {
            // Arrange
            const int topicId = 1;
            const int newCourseId = 2;
            const int userId = 1;

            var originalTopic = new Topic
            {
                Id = topicId,
                Title = "Original Topic",
                Description = "Original Description",
                UserId = userId,
                CourseId = 1,
                Visibility = VisibilityType.Private,
                SubTopics = new List<SubTopic>
                {
                    new SubTopic
                    {
                        Id = 1,
                        Title = "SubTopic 1",
                        UserId = userId,
                        Lessons = new List<Lesson>
                        {
                            new Lesson
                            {
                                Id = 1,
                                Title = "Lesson in SubTopic",
                                UserId = userId,
                                LessonAttachments = new List<LessonAttachment>(),
                                LessonStandards = new List<LessonStandard>()
                            }
                        }
                    }
                },
                Lessons = new List<Lesson>
                {
                    new Lesson
                    {
                        Id = 2,
                        Title = "Direct Lesson",
                        UserId = userId,
                        LessonAttachments = new List<LessonAttachment>(),
                        LessonStandards = new List<LessonStandard>()
                    }
                }
            };

            var newTopic = new Topic
            {
                Id = 10,
                Title = "Original Topic",
                Description = "Original Description",
                UserId = userId,
                CourseId = newCourseId
            };

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync(originalTopic);

            _mockTopicRepository
                .Setup(r => r.AddAsync(It.IsAny<Topic>()))
                .ReturnsAsync(10)
                .Callback<Topic>(t => t.Id = 10);

            // Act
            var result = await _service.CopyTopicAsync(topicId, newCourseId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(originalTopic.Title);
            result.Description.Should().Be(originalTopic.Description);
            result.CourseId.Should().Be(newCourseId);
            result.UserId.Should().Be(userId);

            _mockTopicRepository.Verify(r => r.AddAsync(It.Is<Topic>(t =>
                t.Title == originalTopic.Title &&
                t.Description == originalTopic.Description &&
                t.CourseId == newCourseId &&
                t.UserId == userId &&
                t.SubTopics.Count == 1 &&
                t.Lessons.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task CopyTopicAsync_WithNonExistentTopic_ShouldThrowArgumentException()
        {
            // Arrange
            const int topicId = 999;
            const int newCourseId = 2;
            const int userId = 1;

            _mockTopicRepository
                .Setup(r => r.GetByIdAsync(topicId, It.IsAny<Func<IQueryable<Topic>, IQueryable<Topic>>>()))
                .ReturnsAsync((Topic?)null);

            // Act & Assert
            await _service.Invoking(s => s.CopyTopicAsync(topicId, newCourseId, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Topic not found");
        }

        #endregion

        #region Helper Methods

        private static List<Topic> CreateTestTopics(int userId)
        {
            return new List<Topic>
            {
                new Topic 
                { 
                    Id = 1, 
                    Title = "Topic 1", 
                    UserId = userId, 
                    CourseId = 1,
                    Archived = false,
                    SortOrder = 1,
                    SubTopics = new List<SubTopic>(),
                    Lessons = new List<Lesson>()
                },
                new Topic 
                { 
                    Id = 2, 
                    Title = "Topic 2", 
                    UserId = userId, 
                    CourseId = 1,
                    Archived = false,
                    SortOrder = 2,
                    SubTopics = new List<SubTopic>(),
                    Lessons = new List<Lesson>()
                },
                new Topic 
                { 
                    Id = 3, 
                    Title = "Topic 3", 
                    UserId = userId, 
                    CourseId = 2,
                    Archived = true,
                    SortOrder = 3,
                    SubTopics = new List<SubTopic>(),
                    Lessons = new List<Lesson>()
                }
            };
        }

        #endregion
    }
}