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
    public class CourseServiceTests : TestBase
    {
        private readonly Mock<ICourseRepository> _mockCourseRepository;
        private readonly CourseService _service;

        public CourseServiceTests()
        {
            _mockCourseRepository = new Mock<ICourseRepository>();
            var logger = CreateLogger<CourseService>();

            _service = new CourseService(
                _mockCourseRepository.Object,
                Mapper,
                logger);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithActiveFilter_ShouldReturnActiveCourses()
        {
            // Arrange
            const int userId = 1;
            var courses = CreateTestCourses(userId);
            
            var mockQuery = new TestAsyncEnumerable<Course>(courses.AsQueryable());
            
            _mockCourseRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .Returns(mockQuery);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Active);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Only non-archived courses
            result.All(c => c.UserId == userId).Should().BeTrue();
            result.All(c => !c.Archived).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_WithArchivedFilter_ShouldReturnArchivedCourses()
        {
            // Arrange
            const int userId = 1;
            var courses = CreateTestCourses(userId);
            
            var mockQuery = new TestAsyncEnumerable<Course>(courses.AsQueryable());
            
            _mockCourseRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .Returns(mockQuery);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Archived);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1); // Only archived course
            result.All(c => c.UserId == userId).Should().BeTrue();
            result.All(c => c.Archived).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_WithBothFilter_ShouldReturnAllCourses()
        {
            // Arrange
            const int userId = 1;
            var courses = CreateTestCourses(userId);
            
            var mockQuery = new TestAsyncEnumerable<Course>(courses.AsQueryable());
            
            _mockCourseRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .Returns(mockQuery);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Both);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3); // All courses
            result.All(c => c.UserId == userId).Should().BeTrue();
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

        [Fact]
        public async Task GetAllAsync_WithTeamVisibility_ShouldReturnOwnedAndTeamCourses()
        {
            // Arrange
            const int userId = 1;
            const int schoolId = 100;
            var courses = CreateTestCoursesWithVisibility(userId, schoolId);
            
            var mockQuery = new TestAsyncEnumerable<Course>(courses.AsQueryable());
            
            _mockCourseRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .Returns(mockQuery);

            _mockCourseRepository
                .Setup(r => r.GetUserSchoolId(userId))
                .Returns(schoolId);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Active, (int)VisibilityType.Team);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3); // Owned + Public + Team courses
            var resultList = result.ToList();
            resultList.Should().Contain(c => c.Visibility == "Private" && c.UserId == userId);
            resultList.Should().Contain(c => c.Visibility == "Public");
            resultList.Should().Contain(c => c.Visibility == "Team");
        }

        [Fact]
        public async Task GetAllAsync_WithoutTeamVisibility_ShouldReturnOwnedAndPublicCourses()
        {
            // Arrange
            const int userId = 1;
            const int schoolId = 100;
            var courses = CreateTestCoursesWithVisibility(userId, schoolId);
            
            var mockQuery = new TestAsyncEnumerable<Course>(courses.AsQueryable());
            
            _mockCourseRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .Returns(mockQuery);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Active);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Only owned + public courses (no team)
            var resultList = result.ToList();
            resultList.Should().Contain(c => c.Visibility == "Private" && c.UserId == userId);
            resultList.Should().Contain(c => c.Visibility == "Public");
            resultList.Should().NotContain(c => c.Visibility == "Team" && c.UserId != userId);
        }

        [Fact]
        public async Task GetAllAsync_WithNoCourses_ShouldReturnEmptyCollection()
        {
            // Arrange
            const int userId = 1;
            var emptyCourses = new List<Course>();
            
            var mockQuery = new TestAsyncEnumerable<Course>(emptyCourses.AsQueryable());
            
            _mockCourseRepository
                .Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .Returns(mockQuery);

            // Act
            var result = await _service.GetAllAsync(userId, ArchiveFilter.Active);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithOwnedCourse_ShouldReturnCourseResource()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            var course = new Course
            {
                Id = courseId,
                Title = "Test Course",
                Description = "Test Description",
                UserId = userId,
                Visibility = VisibilityType.Private,
                Archived = false,
                Topics = new List<Topic>
                {
                    new Topic { Id = 1, Title = "Topic 1", UserId = userId }
                },
                User = new User { Id = userId, SchoolId = 100 }
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .ReturnsAsync(course);

            // Act
            var result = await _service.GetByIdAsync(courseId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(courseId);
            result.Title.Should().Be("Test Course");
            result.Description.Should().Be("Test Description");
            result.UserId.Should().Be(userId);
            result.Topics.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_WithPublicCourse_ShouldReturnCourseResource()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            const int differentUserId = 2;
            var course = new Course
            {
                Id = courseId,
                Title = "Public Course",
                Description = "Public Description",
                UserId = differentUserId,
                Visibility = VisibilityType.Public,
                Archived = false,
                Topics = new List<Topic>(),
                User = new User { Id = differentUserId, SchoolId = 200 }
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .ReturnsAsync(course);

            // Act
            var result = await _service.GetByIdAsync(courseId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(courseId);
            result.Title.Should().Be("Public Course");
            result.UserId.Should().Be(differentUserId);
        }

        [Fact]
        public async Task GetByIdAsync_WithTeamCourse_SameSchool_ShouldReturnCourseResource()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            const int differentUserId = 2;
            const int schoolId = 100;
            
            var course = new Course
            {
                Id = courseId,
                Title = "Team Course",
                Description = "Team Description",
                UserId = differentUserId,
                Visibility = VisibilityType.Team,
                Archived = false,
                Topics = new List<Topic>(),
                User = new User { Id = differentUserId, SchoolId = schoolId }
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .ReturnsAsync(course);

            _mockCourseRepository
                .Setup(r => r.GetUserSchoolId(userId))
                .Returns(schoolId);

            // Act
            var result = await _service.GetByIdAsync(courseId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(courseId);
            result.Title.Should().Be("Team Course");
            result.UserId.Should().Be(differentUserId);
        }

        [Fact]
        public async Task GetByIdAsync_WithTeamCourse_DifferentSchool_ShouldReturnNull()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            const int differentUserId = 2;
            const int userSchoolId = 100;
            const int courseSchoolId = 200;
            
            var course = new Course
            {
                Id = courseId,
                Title = "Team Course",
                Description = "Team Description",
                UserId = differentUserId,
                Visibility = VisibilityType.Team,
                Archived = false,
                Topics = new List<Topic>(),
                User = new User { Id = differentUserId, SchoolId = courseSchoolId }
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .ReturnsAsync(course);

            _mockCourseRepository
                .Setup(r => r.GetUserSchoolId(userId))
                .Returns(userSchoolId);

            // Act
            var result = await _service.GetByIdAsync(courseId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithPrivateCourse_DifferentUser_ShouldReturnNull()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            const int differentUserId = 2;
            
            var course = new Course
            {
                Id = courseId,
                Title = "Private Course",
                Description = "Private Description",
                UserId = differentUserId,
                Visibility = VisibilityType.Private,
                Archived = false,
                Topics = new List<Topic>(),
                User = new User { Id = differentUserId, SchoolId = 200 }
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .ReturnsAsync(course);

            // Act
            var result = await _service.GetByIdAsync(courseId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentCourse_ShouldReturnNull()
        {
            // Arrange
            const int courseId = 999;
            const int userId = 1;

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, It.IsAny<Func<IQueryable<Course>, IQueryable<Course>>>()))
                .ReturnsAsync((Course?)null);

            // Act
            var result = await _service.GetByIdAsync(courseId, userId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithValidData_ShouldCreateCourse()
        {
            // Arrange
            const int userId = 1;
            var courseCreateResource = new CourseCreateResource
            {
                Title = "New Course",
                Description = "New Description",
                Visibility = "Private"
            };

            _mockCourseRepository
                .Setup(r => r.AddAsync(It.IsAny<Course>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddAsync(courseCreateResource, userId);

            // Assert
            _mockCourseRepository.Verify(r => r.AddAsync(It.Is<Course>(c => 
                c.Title == "New Course" &&
                c.Description == "New Description" &&
                c.UserId == userId &&
                c.Visibility == VisibilityType.Private &&
                !c.Archived)), Times.Once);
        }

        [Fact]
        public async Task AddAsync_WithPublicVisibility_ShouldCreatePublicCourse()
        {
            // Arrange
            const int userId = 1;
            var courseCreateResource = new CourseCreateResource
            {
                Title = "Public Course",
                Description = "Public Description",
                Visibility = "Public"
            };

            _mockCourseRepository
                .Setup(r => r.AddAsync(It.IsAny<Course>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddAsync(courseCreateResource, userId);

            // Assert
            _mockCourseRepository.Verify(r => r.AddAsync(It.Is<Course>(c => 
                c.Title == "Public Course" &&
                c.Description == "Public Description" &&
                c.UserId == userId &&
                c.Visibility == VisibilityType.Public &&
                !c.Archived)), Times.Once);
        }

        [Fact]
        public async Task AddAsync_WithTeamVisibility_ShouldCreateTeamCourse()
        {
            // Arrange
            const int userId = 1;
            var courseCreateResource = new CourseCreateResource
            {
                Title = "Team Course",
                Description = "Team Description",
                Visibility = "Team"
            };

            _mockCourseRepository
                .Setup(r => r.AddAsync(It.IsAny<Course>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddAsync(courseCreateResource, userId);

            // Assert
            _mockCourseRepository.Verify(r => r.AddAsync(It.Is<Course>(c => 
                c.Title == "Team Course" &&
                c.Description == "Team Description" &&
                c.UserId == userId &&
                c.Visibility == VisibilityType.Team &&
                !c.Archived)), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldAlwaysCreateNonArchivedCourse()
        {
            // Arrange
            const int userId = 1;
            var courseCreateResource = new CourseCreateResource
            {
                Title = "Test Course",
                Description = "Test Description",
                Visibility = "Private"
            };

            _mockCourseRepository
                .Setup(r => r.AddAsync(It.IsAny<Course>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddAsync(courseCreateResource, userId);

            // Assert
            _mockCourseRepository.Verify(r => r.AddAsync(It.Is<Course>(c => 
                !c.Archived)), Times.Once);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidCourse_ShouldUpdateCourse()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;

            var existingCourse = new Course
            {
                Id = courseId,
                Title = "Old Title",
                Description = "Old Description",
                UserId = userId,
                Visibility = VisibilityType.Private,
                Archived = false
            };

            var updateResource = new CourseUpdateResource
            {
                Id = courseId,
                Title = "Updated Title",
                Description = "Updated Description",
                Visibility = "Public",
                Archived = false
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, null))
                .ReturnsAsync(existingCourse);

            _mockCourseRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Course>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateAsync(updateResource, userId);

            // Assert
            _mockCourseRepository.Verify(r => r.UpdateAsync(It.Is<Course>(c => 
                c.Id == courseId &&
                c.Title == "Updated Title" &&
                c.Description == "Updated Description" &&
                c.Visibility == VisibilityType.Public &&
                !c.Archived)), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithArchiveChange_ShouldUpdateArchiveStatus()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;

            var existingCourse = new Course
            {
                Id = courseId,
                Title = "Test Course",
                Description = "Test Description",
                UserId = userId,
                Visibility = VisibilityType.Private,
                Archived = false
            };

            var updateResource = new CourseUpdateResource
            {
                Id = courseId,
                Title = "Test Course",
                Description = "Test Description",
                Visibility = "Private",
                Archived = true
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, null))
                .ReturnsAsync(existingCourse);

            _mockCourseRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Course>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateAsync(updateResource, userId);

            // Assert
            _mockCourseRepository.Verify(r => r.UpdateAsync(It.Is<Course>(c => 
                c.Archived)), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentCourse_ShouldThrowArgumentException()
        {
            // Arrange
            const int courseId = 999;
            const int userId = 1;

            var updateResource = new CourseUpdateResource
            {
                Id = courseId,
                Title = "Updated Title"
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, null))
                .ReturnsAsync((Course?)null);

            // Act & Assert
            await _service.Invoking(s => s.UpdateAsync(updateResource, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*not found or not owned by user*");

            _mockCourseRepository.Verify(r => r.UpdateAsync(It.IsAny<Course>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithUnauthorizedUser_ShouldThrowArgumentException()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            const int differentUserId = 2;

            var existingCourse = new Course
            {
                Id = courseId,
                Title = "Test Course",
                UserId = differentUserId
            };

            var updateResource = new CourseUpdateResource
            {
                Id = courseId,
                Title = "Updated Title"
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, null))
                .ReturnsAsync(existingCourse);

            // Act & Assert
            await _service.Invoking(s => s.UpdateAsync(updateResource, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*not found or not owned by user*");

            _mockCourseRepository.Verify(r => r.UpdateAsync(It.IsAny<Course>()), Times.Never);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidCourse_ShouldDeleteCourse()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;

            var existingCourse = new Course
            {
                Id = courseId,
                Title = "Test Course",
                UserId = userId,
                Topics = new List<Topic>()
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, null))
                .ReturnsAsync(existingCourse);

            _mockCourseRepository
                .Setup(r => r.DeleteAsync(courseId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(courseId, userId);

            // Assert
            _mockCourseRepository.Verify(r => r.DeleteAsync(courseId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentCourse_ShouldThrowArgumentException()
        {
            // Arrange
            const int courseId = 999;
            const int userId = 1;

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, null))
                .ReturnsAsync((Course?)null);

            // Act & Assert
            await _service.Invoking(s => s.DeleteAsync(courseId, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*not found or not owned by user*");

            _mockCourseRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithUnauthorizedUser_ShouldThrowArgumentException()
        {
            // Arrange
            const int courseId = 1;
            const int userId = 1;
            const int differentUserId = 2;

            var existingCourse = new Course
            {
                Id = courseId,
                Title = "Test Course",
                UserId = differentUserId,
                Topics = new List<Topic>()
            };

            _mockCourseRepository
                .Setup(r => r.GetByIdAsync(courseId, null))
                .ReturnsAsync(existingCourse);

            // Act & Assert
            await _service.Invoking(s => s.DeleteAsync(courseId, userId))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*not found or not owned by user*");

            _mockCourseRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region Helper Methods

        private static List<Course> CreateTestCourses(int userId)
        {
            return new List<Course>
            {
                new Course 
                { 
                    Id = 1, 
                    Title = "Course 1", 
                    UserId = userId,
                    Archived = false,
                    Visibility = VisibilityType.Private,
                    Topics = new List<Topic>(),
                    User = new User { Id = userId, SchoolId = 100 }
                },
                new Course 
                { 
                    Id = 2, 
                    Title = "Course 2", 
                    UserId = userId,
                    Archived = false,
                    Visibility = VisibilityType.Private,
                    Topics = new List<Topic>(),
                    User = new User { Id = userId, SchoolId = 100 }
                },
                new Course 
                { 
                    Id = 3, 
                    Title = "Course 3", 
                    UserId = userId,
                    Archived = true,
                    Visibility = VisibilityType.Private,
                    Topics = new List<Topic>(),
                    User = new User { Id = userId, SchoolId = 100 }
                }
            };
        }

        private static List<Course> CreateTestCoursesWithVisibility(int userId, int schoolId)
        {
            return new List<Course>
            {
                new Course 
                { 
                    Id = 1, 
                    Title = "Private Course", 
                    UserId = userId,
                    Archived = false,
                    Visibility = VisibilityType.Private,
                    Topics = new List<Topic>(),
                    User = new User { Id = userId, SchoolId = schoolId }
                },
                new Course 
                { 
                    Id = 2, 
                    Title = "Public Course", 
                    UserId = userId + 1,
                    Archived = false,
                    Visibility = VisibilityType.Public,
                    Topics = new List<Topic>(),
                    User = new User { Id = userId + 1, SchoolId = schoolId + 1 }
                },
                new Course 
                { 
                    Id = 3, 
                    Title = "Team Course", 
                    UserId = userId + 2,
                    Archived = false,
                    Visibility = VisibilityType.Team,
                    Topics = new List<Topic>(),
                    User = new User { Id = userId + 2, SchoolId = schoolId }
                },
                new Course 
                { 
                    Id = 4, 
                    Title = "Other Team Course", 
                    UserId = userId + 3,
                    Archived = false,
                    Visibility = VisibilityType.Team,
                    Topics = new List<Topic>(),
                    User = new User { Id = userId + 3, SchoolId = schoolId + 1 }
                }
            };
        }

        #endregion
    }
}