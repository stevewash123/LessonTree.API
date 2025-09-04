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
    public class SubTopicControllerTests : TestBase
    {
        private readonly Mock<ISubTopicService> _mockSubTopicService;
        private readonly SubTopicController _controller;

        public SubTopicControllerTests()
        {
            _mockSubTopicService = new Mock<ISubTopicService>();
            var logger = CreateLogger<SubTopicController>();
            
            _controller = new SubTopicController(_mockSubTopicService.Object, logger);
            
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
        public async Task GetSubTopic_WithExistingId_ShouldReturnOkWithSubTopic()
        {
            // Arrange
            const int subTopicId = 1;
            const int userId = 1;
            var expectedSubTopic = new SubTopicResource { Id = subTopicId, Title = "Test SubTopic", Description = "SubTopic description" };

            _mockSubTopicService
                .Setup(s => s.GetByIdAsync(subTopicId, userId))
                .ReturnsAsync(expectedSubTopic);

            // Act
            var result = await _controller.GetSubTopic(subTopicId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var subTopic = okResult.Value.Should().BeAssignableTo<SubTopicResource>().Subject;
            subTopic.Should().BeEquivalentTo(expectedSubTopic);
            
            _mockSubTopicService.Verify(s => s.GetByIdAsync(subTopicId, userId), Times.Once);
        }

        [Fact]
        public async Task GetSubTopic_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            const int subTopicId = 999;
            const int userId = 1;

            _mockSubTopicService
                .Setup(s => s.GetByIdAsync(subTopicId, userId))
                .ThrowsAsync(new KeyNotFoundException("SubTopic not found"));

            // Act
            var result = await _controller.GetSubTopic(subTopicId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("SubTopic not found");
            
            _mockSubTopicService.Verify(s => s.GetByIdAsync(subTopicId, userId), Times.Once);
        }

        [Fact]
        public async Task GetSubTopics_WithValidRequest_ShouldReturnOkWithSubTopics()
        {
            // Arrange
            const int userId = 1;
            var expectedSubTopics = new List<SubTopicResource>
            {
                new() { Id = 1, Title = "SubTopic 1", Description = "First subtopic", TopicId = 1 },
                new() { Id = 2, Title = "SubTopic 2", Description = "Second subtopic", TopicId = 1 }
            };

            _mockSubTopicService
                .Setup(s => s.GetAllAsync(userId, ArchiveFilter.Active))
                .ReturnsAsync(expectedSubTopics);

            // Act
            var result = await _controller.GetSubTopics(ArchiveFilter.Active);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var subTopics = okResult.Value.Should().BeAssignableTo<IEnumerable<SubTopicResource>>().Subject;
            subTopics.Should().HaveCount(2);
            subTopics.Should().BeEquivalentTo(expectedSubTopics);
            
            _mockSubTopicService.Verify(s => s.GetAllAsync(userId, ArchiveFilter.Active), Times.Once);
        }

        [Fact]
        public async Task GetSubtopicsByTopicId_WithValidTopicId_ShouldReturnOkWithSubTopics()
        {
            // Arrange
            const int topicId = 1;
            const int userId = 1;
            var expectedSubTopics = new List<SubTopicResource>
            {
                new() { Id = 1, Title = "Topic SubTopic 1", TopicId = topicId },
                new() { Id = 2, Title = "Topic SubTopic 2", TopicId = topicId }
            };

            _mockSubTopicService
                .Setup(s => s.GetSubtopicsByTopicIdAsync(topicId, userId, It.IsAny<ArchiveFilter>()))
                .ReturnsAsync(expectedSubTopics);

            // Act
            var result = await _controller.GetSubtopicsByTopicId(topicId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var subTopics = okResult.Value.Should().BeAssignableTo<IEnumerable<SubTopicResource>>().Subject;
            subTopics.Should().HaveCount(2);
            subTopics.Should().BeEquivalentTo(expectedSubTopics);
            
            _mockSubTopicService.Verify(s => s.GetSubtopicsByTopicIdAsync(topicId, userId, It.IsAny<ArchiveFilter>()), Times.Once);
        }

        [Fact]
        public async Task AddSubTopic_WithValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            const int userId = 1;
            const int createdId = 5;
            
            var createResource = new SubTopicCreateResource
            {
                Title = "New SubTopic",
                Description = "SubTopic description",
                TopicId = 1,
                Visibility = "Private"
            };
            
            var createdSubTopic = new SubTopicResource
            {
                Id = createdId,
                Title = createResource.Title,
                Description = createResource.Description,
                TopicId = createResource.TopicId
            };

            _mockSubTopicService
                .Setup(s => s.AddAsync(createResource, userId))
                .ReturnsAsync(createdId);
                
            _mockSubTopicService
                .Setup(s => s.GetByIdAsync(createdId, userId))
                .ReturnsAsync(createdSubTopic);

            // Act
            var result = await _controller.AddSubTopic(createResource);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(SubTopicController.GetSubTopic));
            createdResult.RouteValues!["id"].Should().Be(createdId);
            
            var subTopic = createdResult.Value.Should().BeAssignableTo<SubTopicResource>().Subject;
            subTopic.Should().BeEquivalentTo(createdSubTopic);
            
            _mockSubTopicService.Verify(s => s.AddAsync(createResource, userId), Times.Once);
            _mockSubTopicService.Verify(s => s.GetByIdAsync(createdId, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateSubTopic_WithValidData_ShouldReturnOkWithUpdatedSubTopic()
        {
            // Arrange
            const int subTopicId = 1;
            const int userId = 1;
            
            var updateResource = new SubTopicUpdateResource
            {
                Id = subTopicId,
                Title = "Updated SubTopic",
                Description = "Updated description",
                Visibility = "Private"
            };
            
            var updatedSubTopic = new SubTopicResource
            {
                Id = subTopicId,
                Title = updateResource.Title,
                Description = updateResource.Description
            };

            _mockSubTopicService
                .Setup(s => s.UpdateAsync(updateResource, userId))
                .ReturnsAsync(updatedSubTopic);

            // Act
            var result = await _controller.UpdateSubTopic(subTopicId, updateResource);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var subTopic = okResult.Value.Should().BeAssignableTo<SubTopicResource>().Subject;
            subTopic.Should().BeEquivalentTo(updatedSubTopic);
            
            _mockSubTopicService.Verify(s => s.UpdateAsync(updateResource, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateSubTopic_WithMismatchedIds_ShouldReturnBadRequest()
        {
            // Arrange
            const int subTopicId = 1;
            var updateResource = new SubTopicUpdateResource
            {
                Id = 2, // Different from route parameter
                Title = "Updated SubTopic",
                Description = "Updated description"
            };

            // Act
            var result = await _controller.UpdateSubTopic(subTopicId, updateResource);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
            
            _mockSubTopicService.Verify(s => s.UpdateAsync(It.IsAny<SubTopicUpdateResource>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UpdateSubTopic_WithNonExistentSubTopic_ShouldReturnNotFound()
        {
            // Arrange
            const int subTopicId = 999;
            const int userId = 1;
            
            var updateResource = new SubTopicUpdateResource
            {
                Id = subTopicId,
                Title = "Updated SubTopic",
                Description = "Updated description"
            };

            _mockSubTopicService
                .Setup(s => s.UpdateAsync(updateResource, userId))
                .ThrowsAsync(new KeyNotFoundException("SubTopic not found"));

            // Act
            var result = await _controller.UpdateSubTopic(subTopicId, updateResource);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("SubTopic not found");
            
            _mockSubTopicService.Verify(s => s.UpdateAsync(updateResource, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateSubTopic_WithUnauthorizedAccess_ShouldReturnForbid()
        {
            // Arrange
            const int subTopicId = 1;
            const int userId = 1;
            
            var updateResource = new SubTopicUpdateResource
            {
                Id = subTopicId,
                Title = "Updated SubTopic",
                Description = "Updated description"
            };

            _mockSubTopicService
                .Setup(s => s.UpdateAsync(updateResource, userId))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            // Act
            var result = await _controller.UpdateSubTopic(subTopicId, updateResource);

            // Assert
            result.Should().BeOfType<ForbidResult>();
            
            _mockSubTopicService.Verify(s => s.UpdateAsync(updateResource, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateSubTopic_WithInvalidOperation_ShouldReturnBadRequest()
        {
            // Arrange
            const int subTopicId = 1;
            const int userId = 1;
            
            var updateResource = new SubTopicUpdateResource
            {
                Id = subTopicId,
                Title = "Updated SubTopic",
                Description = "Updated description"
            };

            _mockSubTopicService
                .Setup(s => s.UpdateAsync(updateResource, userId))
                .ThrowsAsync(new InvalidOperationException("Invalid operation"));

            // Act
            var result = await _controller.UpdateSubTopic(subTopicId, updateResource);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid operation");
            
            _mockSubTopicService.Verify(s => s.UpdateAsync(updateResource, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteSubTopic_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            const int subTopicId = 1;
            const int userId = 1;

            _mockSubTopicService
                .Setup(s => s.DeleteAsync(subTopicId, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteSubTopic(subTopicId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            
            _mockSubTopicService.Verify(s => s.DeleteAsync(subTopicId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteSubTopic_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            const int subTopicId = 999;
            const int userId = 1;

            _mockSubTopicService
                .Setup(s => s.DeleteAsync(subTopicId, userId))
                .ThrowsAsync(new ArgumentException("SubTopic not found"));

            // Act
            var result = await _controller.DeleteSubTopic(subTopicId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("SubTopic not found");
            
            _mockSubTopicService.Verify(s => s.DeleteAsync(subTopicId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteSubTopic_WithUnauthorizedAccess_ShouldReturnForbid()
        {
            // Arrange
            const int subTopicId = 1;
            const int userId = 1;

            _mockSubTopicService
                .Setup(s => s.DeleteAsync(subTopicId, userId))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            // Act
            var result = await _controller.DeleteSubTopic(subTopicId);

            // Assert
            result.Should().BeOfType<ForbidResult>();
            
            _mockSubTopicService.Verify(s => s.DeleteAsync(subTopicId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteSubTopic_WithInvalidOperation_ShouldReturnBadRequest()
        {
            // Arrange
            const int subTopicId = 1;
            const int userId = 1;

            _mockSubTopicService
                .Setup(s => s.DeleteAsync(subTopicId, userId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete - has dependencies"));

            // Act
            var result = await _controller.DeleteSubTopic(subTopicId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Cannot delete - has dependencies");
            
            _mockSubTopicService.Verify(s => s.DeleteAsync(subTopicId, userId), Times.Once);
        }

        [Fact]
        public async Task CopySubTopic_WithValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            const int userId = 1;
            const int sourceSubTopicId = 1;
            const int targetTopicId = 2;
            const int newSubTopicId = 5;
            
            var copyResource = new SubTopicMoveResource
            {
                SubTopicId = sourceSubTopicId,
                NewTopicId = targetTopicId
            };
            
            var copiedSubTopic = new SubTopicResource
            {
                Id = newSubTopicId,
                Title = "Copied SubTopic",
                TopicId = targetTopicId
            };

            _mockSubTopicService
                .Setup(s => s.CopySubTopicAsync(sourceSubTopicId, targetTopicId, userId))
                .ReturnsAsync(copiedSubTopic);

            // Act
            var result = await _controller.CopySubTopic(copyResource);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(SubTopicController.GetSubTopic));
            createdResult.RouteValues!["id"].Should().Be(newSubTopicId);
            
            var subTopic = createdResult.Value.Should().BeAssignableTo<SubTopicResource>().Subject;
            subTopic.Should().BeEquivalentTo(copiedSubTopic);
            
            _mockSubTopicService.Verify(s => s.CopySubTopicAsync(sourceSubTopicId, targetTopicId, userId), Times.Once);
        }

        [Fact]
        public async Task CopySubTopic_WithNonExistentSubTopic_ShouldReturnNotFound()
        {
            // Arrange
            const int userId = 1;
            var copyResource = new SubTopicMoveResource
            {
                SubTopicId = 999,
                NewTopicId = 1
            };

            _mockSubTopicService
                .Setup(s => s.CopySubTopicAsync(copyResource.SubTopicId, copyResource.NewTopicId, userId))
                .ThrowsAsync(new ArgumentException("SubTopic not found"));

            // Act
            var result = await _controller.CopySubTopic(copyResource);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("SubTopic not found");
            
            _mockSubTopicService.Verify(s => s.CopySubTopicAsync(copyResource.SubTopicId, copyResource.NewTopicId, userId), Times.Once);
        }

        [Fact]
        public async Task MoveSubTopic_WithValidMove_ShouldReturnOkWithResult()
        {
            // Arrange
            const int userId = 1;
            var moveResource = new SubTopicMoveResource
            {
                SubTopicId = 1,
                NewTopicId = 2,
                AfterSiblingId = null
            };
            
            var movedSubTopic = new SubTopicResource
            {
                Id = moveResource.SubTopicId,
                TopicId = moveResource.NewTopicId,
                SortOrder = 1
            };

            _mockSubTopicService
                .Setup(s => s.MoveSubTopicAsync(moveResource, userId))
                .ReturnsAsync(movedSubTopic);

            // Act
            var result = await _controller.MoveSubTopic(moveResource);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var positioningResult = okResult.Value.Should().BeAssignableTo<SubTopicPositioningResult>().Subject;
            
            positioningResult.IsSuccess.Should().BeTrue();
            positioningResult.SubTopicId.Should().Be(moveResource.SubTopicId);
            positioningResult.NewTopicId.Should().Be(moveResource.NewTopicId);
            
            _mockSubTopicService.Verify(s => s.MoveSubTopicAsync(moveResource, userId), Times.Once);
        }

        [Fact]
        public async Task MoveSubTopic_WithNonExistentSubTopic_ShouldReturnNotFound()
        {
            // Arrange
            const int userId = 1;
            var moveResource = new SubTopicMoveResource
            {
                SubTopicId = 999,
                NewTopicId = 1
            };

            _mockSubTopicService
                .Setup(s => s.MoveSubTopicAsync(moveResource, userId))
                .ThrowsAsync(new ArgumentException("SubTopic not found"));

            // Act
            var result = await _controller.MoveSubTopic(moveResource);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var errorResponse = notFoundResult.Value.Should().BeAssignableTo<object>().Subject;
            
            _mockSubTopicService.Verify(s => s.MoveSubTopicAsync(moveResource, userId), Times.Once);
        }

        [Fact]
        public async Task MoveSubTopic_WithUnauthorizedAccess_ShouldReturnForbid()
        {
            // Arrange
            const int userId = 1;
            var moveResource = new SubTopicMoveResource
            {
                SubTopicId = 1,
                NewTopicId = 2
            };

            _mockSubTopicService
                .Setup(s => s.MoveSubTopicAsync(moveResource, userId))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            // Act
            var result = await _controller.MoveSubTopic(moveResource);

            // Assert
            result.Should().BeOfType<ForbidResult>();
            
            _mockSubTopicService.Verify(s => s.MoveSubTopicAsync(moveResource, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateSubTopicSortOrder_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            const int subTopicId = 1;
            const int userId = 1;
            const int newSortOrder = 3;

            _mockSubTopicService
                .Setup(s => s.UpdateSortOrderAsync(subTopicId, newSortOrder, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateSubTopicSortOrder(subTopicId, newSortOrder);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            
            _mockSubTopicService.Verify(s => s.UpdateSortOrderAsync(subTopicId, newSortOrder, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateSubTopicSortOrder_WithNonExistentSubTopic_ShouldReturnNotFound()
        {
            // Arrange
            const int subTopicId = 999;
            const int userId = 1;
            const int newSortOrder = 3;

            _mockSubTopicService
                .Setup(s => s.UpdateSortOrderAsync(subTopicId, newSortOrder, userId))
                .ThrowsAsync(new ArgumentException("SubTopic not found"));

            // Act
            var result = await _controller.UpdateSubTopicSortOrder(subTopicId, newSortOrder);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().NotBeNull();
            
            _mockSubTopicService.Verify(s => s.UpdateSortOrderAsync(subTopicId, newSortOrder, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateSubTopicSortOrder_WithUnauthorizedAccess_ShouldReturnForbid()
        {
            // Arrange
            const int subTopicId = 1;
            const int userId = 1;
            const int newSortOrder = 3;

            _mockSubTopicService
                .Setup(s => s.UpdateSortOrderAsync(subTopicId, newSortOrder, userId))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            // Act
            var result = await _controller.UpdateSubTopicSortOrder(subTopicId, newSortOrder);

            // Assert
            result.Should().BeOfType<ForbidResult>();
            
            _mockSubTopicService.Verify(s => s.UpdateSortOrderAsync(subTopicId, newSortOrder, userId), Times.Once);
        }
    }
}