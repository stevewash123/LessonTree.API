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
    /// <summary>
    /// Simplified controller tests focusing on HTTP response patterns
    /// </summary>
    public class LessonControllerTestsSimple : TestBase
    {
        private readonly Mock<ILessonService> _mockLessonService;
        private readonly Mock<IAttachmentService> _mockAttachmentService;
        private readonly LessonController _controller;

        public LessonControllerTestsSimple()
        {
            _mockLessonService = new Mock<ILessonService>();
            _mockAttachmentService = new Mock<IAttachmentService>();
            var logger = CreateLogger<LessonController>();
            
            _controller = new LessonController(_mockLessonService.Object, _mockAttachmentService.Object, logger);
            
            // Setup controller context with authenticated user
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
        public async Task GetLessons_WithValidRequest_ShouldReturnOkWithLessons()
        {
            // Arrange
            const int userId = 1;
            var expectedLessons = new List<LessonResource>
            {
                new() { Id = 1, Title = "Lesson 1", UserId = userId },
                new() { Id = 2, Title = "Lesson 2", UserId = userId }
            };

            _mockLessonService
                .Setup(s => s.GetAllAsync(userId, ArchiveFilter.Active))
                .ReturnsAsync(expectedLessons);

            // Act
            var result = await _controller.GetLessons(ArchiveFilter.Active);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var lessons = okResult.Value.Should().BeAssignableTo<IEnumerable<LessonResource>>().Subject;
            lessons.Should().HaveCount(2);
            lessons.Should().BeEquivalentTo(expectedLessons);
            
            _mockLessonService.Verify(s => s.GetAllAsync(userId, ArchiveFilter.Active), Times.Once);
        }

        [Fact]
        public async Task GetLesson_WithExistingId_ShouldReturnOkWithLesson()
        {
            // Arrange
            const int lessonId = 1;
            const int userId = 1;
            var expectedLesson = new LessonDetailResource { Id = lessonId, Title = "Test Lesson" };

            _mockLessonService
                .Setup(s => s.GetByIdAsync(lessonId, userId))
                .ReturnsAsync(expectedLesson);

            // Act
            var result = await _controller.GetLesson(lessonId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var lesson = okResult.Value.Should().BeAssignableTo<LessonDetailResource>().Subject;
            lesson.Should().BeEquivalentTo(expectedLesson);
            
            _mockLessonService.Verify(s => s.GetByIdAsync(lessonId, userId), Times.Once);
        }

        [Fact]
        public async Task GetLesson_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            const int lessonId = 999;
            const int userId = 1;

            _mockLessonService
                .Setup(s => s.GetByIdAsync(lessonId, userId))
                .ReturnsAsync((LessonDetailResource?)null);

            // Act
            var result = await _controller.GetLesson(lessonId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            
            _mockLessonService.Verify(s => s.GetByIdAsync(lessonId, userId), Times.Once);
        }

        [Fact]
        public async Task AddLesson_WithValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            const int userId = 1;
            const int createdId = 5;
            
            var createResource = new LessonCreateResource
            {
                Title = "New Lesson",
                Objective = "Learn new things",
                TopicId = 1
            };
            
            var createdLesson = new LessonDetailResource
            {
                Id = createdId,
                Title = createResource.Title,
                Objective = createResource.Objective,
                TopicId = createResource.TopicId
            };

            _mockLessonService
                .Setup(s => s.AddAsync(createResource, userId))
                .ReturnsAsync(createdId);
                
            _mockLessonService
                .Setup(s => s.GetByIdAsync(createdId, userId))
                .ReturnsAsync(createdLesson);

            // Act
            var result = await _controller.AddLesson(createResource);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(LessonController.GetLesson));
            createdResult.RouteValues!["id"].Should().Be(createdId);
            
            var lesson = createdResult.Value.Should().BeAssignableTo<LessonDetailResource>().Subject;
            lesson.Should().BeEquivalentTo(createdLesson);
            
            _mockLessonService.Verify(s => s.AddAsync(createResource, userId), Times.Once);
            _mockLessonService.Verify(s => s.GetByIdAsync(createdId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteLesson_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            const int lessonId = 1;
            const int userId = 1;

            _mockLessonService
                .Setup(s => s.DeleteAsync(lessonId, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteLesson(lessonId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            
            _mockLessonService.Verify(s => s.DeleteAsync(lessonId, userId), Times.Once);
        }

        [Fact]
        public async Task MoveLesson_WithValidMove_ShouldReturnOkWithResult()
        {
            // Arrange
            const int userId = 1;
            var moveResource = new LessonMoveResource
            {
                LessonId = 1,
                NewTopicId = 2,
                AfterSiblingId = null
            };
            
            var movedLesson = new LessonResource
            {
                Id = moveResource.LessonId,
                TopicId = moveResource.NewTopicId,
                SortOrder = 1
            };

            _mockLessonService
                .Setup(s => s.MoveLessonAsync(moveResource, userId))
                .ReturnsAsync(movedLesson);

            // Act
            var result = await _controller.MoveLesson(moveResource);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var positioningResult = okResult.Value.Should().BeAssignableTo<LessonPositioningResult>().Subject;
            
            positioningResult.IsSuccess.Should().BeTrue();
            positioningResult.LessonId.Should().Be(moveResource.LessonId);
            positioningResult.NewTopicId.Should().Be(moveResource.NewTopicId);
            
            _mockLessonService.Verify(s => s.MoveLessonAsync(moveResource, userId), Times.Once);
        }
    }
}