using LessonTree.API.Controllers;
using LessonTree.BLL.Service;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using LessonTree.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LessonTree.Tests.Controllers
{
    public class TopicControllerTests : TestBase
    {
        private readonly Mock<ITopicService> _mockTopicService;
        private readonly TopicController _controller;

        public TopicControllerTests()
        {
            _mockTopicService = new Mock<ITopicService>();
            var logger = CreateLogger<TopicController>();
            
            _controller = new TopicController(_mockTopicService.Object, logger);
            
            SetupControllerContext();
        }

        private void SetupControllerContext()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new("UserId", "1")
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

        [Fact]
        public async Task GetTopics_WithValidRequest_ShouldReturnOkWithTopics()
        {
            // Arrange
            const int userId = 1;
            var expectedTopics = new List<TopicResource>
            {
                new() { Id = 1, Title = "Topic 1", Description = "First topic", CourseId = 1 },
                new() { Id = 2, Title = "Topic 2", Description = "Second topic", CourseId = 1 }
            };

            _mockTopicService
                .Setup(s => s.GetAllAsync(userId, ArchiveFilter.Active))
                .ReturnsAsync(expectedTopics);

            // Act
            var result = await _controller.GetTopics(ArchiveFilter.Active);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var topics = okResult.Value.Should().BeAssignableTo<IEnumerable<TopicResource>>().Subject;
            topics.Should().HaveCount(2);
            topics.Should().BeEquivalentTo(expectedTopics);
            
            _mockTopicService.Verify(s => s.GetAllAsync(userId, ArchiveFilter.Active), Times.Once);
        }

        [Fact]
        public async Task GetTopicsByCourseId_WithValidCourseId_ShouldReturnOkWithTopics()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            var expectedTopics = new List<TopicResource>
            {
                new() { Id = 1, Title = "Course Topic 1", CourseId = courseId },
                new() { Id = 2, Title = "Course Topic 2", CourseId = courseId }
            };

            _mockTopicService
                .Setup(s => s.GetTopicsByCourseAsync(courseId, userId, ArchiveFilter.Active))
                .ReturnsAsync(expectedTopics);

            // Act
            var result = await _controller.GetTopicsByCourseId(courseId, ArchiveFilter.Active);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var topics = okResult.Value.Should().BeAssignableTo<IEnumerable<TopicResource>>().Subject;
            topics.Should().HaveCount(2);
            topics.Should().BeEquivalentTo(expectedTopics);
            
            _mockTopicService.Verify(s => s.GetTopicsByCourseAsync(courseId, userId, ArchiveFilter.Active), Times.Once);
        }

        [Fact]
        public async Task GetTopic_WithExistingId_ShouldReturnOkWithTopic()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            var expectedTopic = new TopicResource { Id = topicId, Title = "Test Topic", Description = "Topic description" };

            _mockTopicService
                .Setup(s => s.GetByIdAsync(topicId, userId))
                .ReturnsAsync(expectedTopic);

            // Act
            var result = await _controller.GetTopic(topicId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var topic = okResult.Value.Should().BeAssignableTo<TopicResource>().Subject;
            topic.Should().BeEquivalentTo(expectedTopic);
            
            _mockTopicService.Verify(s => s.GetByIdAsync(topicId, userId), Times.Once);
        }

        [Fact]
        public async Task GetTopic_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            const int topicId = 999;
            const int userId = 1;

            _mockTopicService
                .Setup(s => s.GetByIdAsync(topicId, userId))
                .ReturnsAsync((TopicResource?)null);

            // Act
            var result = await _controller.GetTopic(topicId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            
            _mockTopicService.Verify(s => s.GetByIdAsync(topicId, userId), Times.Once);
        }

        [Fact]
        public async Task AddTopic_WithValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            const int userId = 1;
            const int createdId = 5;
            
            var createResource = new TopicCreateResource
            {
                Title = "New Topic",
                Description = "Topic description",
                CourseId = 1,
                Visibility = "Private"
            };
            
            var createdTopic = new TopicResource
            {
                Id = createdId,
                Title = createResource.Title,
                Description = createResource.Description,
                CourseId = createResource.CourseId
            };

            _mockTopicService
                .Setup(s => s.AddAsync(createResource, userId))
                .ReturnsAsync(createdId);
                
            _mockTopicService
                .Setup(s => s.GetByIdAsync(createdId, userId))
                .ReturnsAsync(createdTopic);

            // Act
            var result = await _controller.AddTopic(createResource);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(TopicController.GetTopic));
            createdResult.RouteValues!["id"].Should().Be(createdId);
            
            var topic = createdResult.Value.Should().BeAssignableTo<TopicResource>().Subject;
            topic.Should().BeEquivalentTo(createdTopic);
            
            _mockTopicService.Verify(s => s.AddAsync(createResource, userId), Times.Once);
            _mockTopicService.Verify(s => s.GetByIdAsync(createdId, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateTopic_WithValidData_ShouldReturnOkWithUpdatedTopic()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            
            var updateResource = new TopicUpdateResource
            {
                Id = topicId,
                Title = "Updated Topic",
                Description = "Updated description",
                Visibility = "Private"
            };
            
            var updatedTopic = new TopicResource
            {
                Id = topicId,
                Title = updateResource.Title,
                Description = updateResource.Description
            };

            _mockTopicService
                .Setup(s => s.UpdateAsync(updateResource, userId))
                .ReturnsAsync(updatedTopic);

            // Act
            var result = await _controller.UpdateTopic(topicId, updateResource);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var topic = okResult.Value.Should().BeAssignableTo<TopicResource>().Subject;
            topic.Should().BeEquivalentTo(updatedTopic);
            
            _mockTopicService.Verify(s => s.UpdateAsync(updateResource, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateTopic_WithMismatchedIds_ShouldReturnBadRequest()
        {
            // Arrange
            const int topicId = 1;
            var updateResource = new TopicUpdateResource
            {
                Id = 2, // Different from route parameter
                Title = "Updated Topic",
                Description = "Updated description"
            };

            // Act
            var result = await _controller.UpdateTopic(topicId, updateResource);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
            
            _mockTopicService.Verify(s => s.UpdateAsync(It.IsAny<TopicUpdateResource>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTopic_WithNonExistentTopic_ShouldReturnNotFound()
        {
            // Arrange
            const int topicId = 999;
            const int userId = 1;
            
            var updateResource = new TopicUpdateResource
            {
                Id = topicId,
                Title = "Updated Topic",
                Description = "Updated description"
            };

            _mockTopicService
                .Setup(s => s.UpdateAsync(updateResource, userId))
                .ThrowsAsync(new ArgumentException("Topic not found"));

            // Act
            var result = await _controller.UpdateTopic(topicId, updateResource);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().NotBeNull();
            
            _mockTopicService.Verify(s => s.UpdateAsync(updateResource, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteTopic_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;

            _mockTopicService
                .Setup(s => s.DeleteAsync(topicId, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteTopic(topicId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            
            _mockTopicService.Verify(s => s.DeleteAsync(topicId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteTopic_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            const int topicId = 999;
            const int userId = 1;

            _mockTopicService
                .Setup(s => s.DeleteAsync(topicId, userId))
                .ThrowsAsync(new ArgumentException("Topic not found"));

            // Act
            var result = await _controller.DeleteTopic(topicId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().NotBeNull();
            
            _mockTopicService.Verify(s => s.DeleteAsync(topicId, userId), Times.Once);
        }

        [Fact]
        public async Task MoveTopic_WithValidMove_ShouldReturnOkWithResult()
        {
            // Arrange
            const int userId = 1;
            var moveResource = new TopicMoveResource
            {
                TopicId = 1,
                NewCourseId = 2,
                AfterSiblingId = null
            };
            
            var movedTopic = new TopicResource
            {
                Id = moveResource.TopicId,
                CourseId = moveResource.NewCourseId,
                SortOrder = 1
            };

            _mockTopicService
                .Setup(s => s.MoveTopicAsync(moveResource, userId))
                .ReturnsAsync(movedTopic);

            // Act
            var result = await _controller.MoveTopic(moveResource);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var positioningResult = okResult.Value.Should().BeAssignableTo<TopicPositioningResult>().Subject;
            
            positioningResult.IsSuccess.Should().BeTrue();
            positioningResult.TopicId.Should().Be(moveResource.TopicId);
            positioningResult.NewCourseId.Should().Be(moveResource.NewCourseId);
            
            _mockTopicService.Verify(s => s.MoveTopicAsync(moveResource, userId), Times.Once);
        }

        [Fact]
        public async Task CopyTopic_WithValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            const int userId = 1;
            const int sourceTopicId = 1;
            const int targetCourseId = 2;
            const int newTopicId = 5;
            
            var copyResource = new TopicMoveResource
            {
                TopicId = sourceTopicId,
                NewCourseId = targetCourseId
            };
            
            var copiedTopic = new TopicResource
            {
                Id = newTopicId,
                Title = "Copied Topic",
                CourseId = targetCourseId
            };

            _mockTopicService
                .Setup(s => s.CopyTopicAsync(sourceTopicId, targetCourseId, userId))
                .ReturnsAsync(copiedTopic);

            // Act
            var result = await _controller.CopyTopic(copyResource);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(TopicController.GetTopic));
            createdResult.RouteValues!["id"].Should().Be(newTopicId);
            
            var topic = createdResult.Value.Should().BeAssignableTo<TopicResource>().Subject;
            topic.Should().BeEquivalentTo(copiedTopic);
            
            _mockTopicService.Verify(s => s.CopyTopicAsync(sourceTopicId, targetCourseId, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateTopicSortOrder_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            const int newSortOrder = 3;

            _mockTopicService
                .Setup(s => s.UpdateSortOrderAsync(topicId, newSortOrder))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateTopicSortOrder(topicId, newSortOrder);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            
            _mockTopicService.Verify(s => s.UpdateSortOrderAsync(topicId, newSortOrder), Times.Once);
        }

        [Fact]
        public async Task UpdateTopicSortOrder_WithNonExistentTopic_ShouldReturnNotFound()
        {
            // Arrange
            const int topicId = 999;
            const int newSortOrder = 3;

            _mockTopicService
                .Setup(s => s.UpdateSortOrderAsync(topicId, newSortOrder))
                .ThrowsAsync(new ArgumentException("Topic not found"));

            // Act
            var result = await _controller.UpdateTopicSortOrder(topicId, newSortOrder);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().NotBeNull();
            
            _mockTopicService.Verify(s => s.UpdateSortOrderAsync(topicId, newSortOrder), Times.Once);
        }
    }
}