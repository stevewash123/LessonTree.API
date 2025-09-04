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
    public class CourseControllerTests : TestBase
    {
        private readonly Mock<ICourseService> _mockCourseService;
        private readonly CourseController _controller;

        public CourseControllerTests()
        {
            _mockCourseService = new Mock<ICourseService>();
            var logger = CreateLogger<CourseController>();
            
            _controller = new CourseController(_mockCourseService.Object, logger);
            
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
        public async Task GetCourses_WithValidRequest_ShouldReturnOkWithCourses()
        {
            // Arrange
            const int userId = 1;
            var expectedCourses = new List<CourseResource>
            {
                new() { Id = 1, Title = "Course 1", Description = "First course" },
                new() { Id = 2, Title = "Course 2", Description = "Second course" }
            };

            _mockCourseService
                .Setup(s => s.GetAllAsync(userId, ArchiveFilter.Active, null))
                .ReturnsAsync(expectedCourses);

            // Act
            var result = await _controller.GetCourses(ArchiveFilter.Active, null);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var courses = okResult.Value.Should().BeAssignableTo<IEnumerable<CourseResource>>().Subject;
            courses.Should().HaveCount(2);
            courses.Should().BeEquivalentTo(expectedCourses);
            
            _mockCourseService.Verify(s => s.GetAllAsync(userId, ArchiveFilter.Active, null), Times.Once);
        }

        [Fact]
        public async Task GetCourses_WithVisibilityFilter_ShouldReturnFilteredCourses()
        {
            // Arrange
            const int userId = 1;
            const int visibilityFilter = 1; // Public
            var expectedCourses = new List<CourseResource>
            {
                new() { Id = 1, Title = "Public Course", Description = "Public course", Visibility = "Public" }
            };

            _mockCourseService
                .Setup(s => s.GetAllAsync(userId, ArchiveFilter.Active, visibilityFilter))
                .ReturnsAsync(expectedCourses);

            // Act
            var result = await _controller.GetCourses(ArchiveFilter.Active, visibilityFilter);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var courses = okResult.Value.Should().BeAssignableTo<IEnumerable<CourseResource>>().Subject;
            courses.Should().HaveCount(1);
            courses.Should().BeEquivalentTo(expectedCourses);
            
            _mockCourseService.Verify(s => s.GetAllAsync(userId, ArchiveFilter.Active, visibilityFilter), Times.Once);
        }

        [Fact]
        public async Task GetCourse_WithExistingId_ShouldReturnOkWithCourse()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            var expectedCourse = new CourseResource { Id = courseId, Title = "Test Course", Description = "Course description" };

            _mockCourseService
                .Setup(s => s.GetByIdAsync(courseId, userId))
                .ReturnsAsync(expectedCourse);

            // Act
            var result = await _controller.GetCourse(courseId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var course = okResult.Value.Should().BeAssignableTo<CourseResource>().Subject;
            course.Should().BeEquivalentTo(expectedCourse);
            
            _mockCourseService.Verify(s => s.GetByIdAsync(courseId, userId), Times.Once);
        }

        [Fact]
        public async Task GetCourse_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            const int courseId = 999;
            const int userId = 1;

            _mockCourseService
                .Setup(s => s.GetByIdAsync(courseId, userId))
                .ReturnsAsync((CourseResource?)null);

            // Act
            var result = await _controller.GetCourse(courseId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            
            _mockCourseService.Verify(s => s.GetByIdAsync(courseId, userId), Times.Once);
        }

        [Fact]
        public async Task AddCourse_WithValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            const int userId = 1;
            
            var createResource = new CourseCreateResource
            {
                Title = "New Course",
                Description = "Course description",
                Visibility = "Private"
            };

            // Mock GetAllAsync to return the newly created course as the last item
            var allCourses = new List<CourseResource>
            {
                new() { Id = 5, Title = createResource.Title, Description = createResource.Description }
            };

            var createdCourse = new CourseResource
            {
                Id = 5,
                Title = createResource.Title,
                Description = createResource.Description
            };

            _mockCourseService
                .Setup(s => s.AddAsync(createResource, userId))
                .Returns(Task.CompletedTask);

            _mockCourseService
                .Setup(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<ArchiveFilter>(), It.IsAny<int?>()))
                .ReturnsAsync(allCourses);
                
            _mockCourseService
                .Setup(s => s.GetByIdAsync(5, userId))
                .ReturnsAsync(createdCourse);

            // Act
            var result = await _controller.AddCourse(createResource);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(CourseController.GetCourse));
            createdResult.RouteValues!["id"].Should().Be(5);
            
            var course = createdResult.Value.Should().BeAssignableTo<CourseResource>().Subject;
            course.Should().BeEquivalentTo(createdCourse);
            
            _mockCourseService.Verify(s => s.AddAsync(createResource, userId), Times.Once);
            _mockCourseService.Verify(s => s.GetAllAsync(It.IsAny<int>(), It.IsAny<ArchiveFilter>(), It.IsAny<int?>()), Times.Once);
            _mockCourseService.Verify(s => s.GetByIdAsync(5, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateCourse_WithValidData_ShouldReturnOkWithUpdatedCourse()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            
            var updateResource = new CourseUpdateResource
            {
                Id = courseId,
                Title = "Updated Course",
                Description = "Updated description",
                Visibility = "Private"
            };
            
            var updatedCourse = new CourseResource
            {
                Id = courseId,
                Title = updateResource.Title,
                Description = updateResource.Description
            };

            _mockCourseService
                .Setup(s => s.UpdateAsync(updateResource, userId))
                .Returns(Task.CompletedTask);

            _mockCourseService
                .Setup(s => s.GetByIdAsync(courseId, userId))
                .ReturnsAsync(updatedCourse);

            // Act
            var result = await _controller.UpdateCourse(courseId, updateResource);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var course = okResult.Value.Should().BeAssignableTo<CourseResource>().Subject;
            course.Should().BeEquivalentTo(updatedCourse);
            
            _mockCourseService.Verify(s => s.UpdateAsync(updateResource, userId), Times.Once);
            _mockCourseService.Verify(s => s.GetByIdAsync(courseId, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateCourse_WithMismatchedIds_ShouldReturnBadRequest()
        {
            // Arrange
            const int courseId = 1;
            var updateResource = new CourseUpdateResource
            {
                Id = 2, // Different from route parameter
                Title = "Updated Course",
                Description = "Updated description"
            };

            // Act
            var result = await _controller.UpdateCourse(courseId, updateResource);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;
            problemDetails.Title.Should().Be("ID mismatch");
            
            _mockCourseService.Verify(s => s.UpdateAsync(It.IsAny<CourseUpdateResource>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCourse_WithExistingId_ShouldReturnNoContent()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;

            _mockCourseService
                .Setup(s => s.DeleteAsync(courseId, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteCourse(courseId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            
            _mockCourseService.Verify(s => s.DeleteAsync(courseId, userId), Times.Once);
        }
    }
}