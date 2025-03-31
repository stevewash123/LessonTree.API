using AutoMapper;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseService> _logger;

    public CourseService(ICourseRepository repository, IMapper mapper, ILogger<CourseService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CourseResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active, int? visibility = null)
    {
        _logger.LogDebug("Fetching courses for User ID: {UserId}, Filter: {Filter}, Visibility: {Visibility}", userId, filter, visibility);

        var query = _repository.GetAll(q => q
            .Include(c => c.Topics)
                .ThenInclude(t => t.Lessons)
            .Include(c => c.Topics)
                .ThenInclude(t => t.SubTopics)
                    .ThenInclude(st => st.Lessons)
            .Include(c => c.User)); // Include User to access SchoolId

        // Apply archive filter
        query = filter switch
        {
            ArchiveFilter.Active => query.Where(c => !c.Archived),
            ArchiveFilter.Archived => query.Where(c => c.Archived),
            ArchiveFilter.Both => query,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
        };

        // Apply visibility filter
        if (visibility.HasValue && visibility.Value == (int)VisibilityType.Team)
        {
            // "My objects + School objects": Include courses where Visibility is Team (School-level)
            query = query.Where(c => c.UserId == userId // User's own courses
                || c.Visibility == VisibilityType.Public // Public courses
                || (c.Visibility == VisibilityType.Team && c.User.SchoolId != null && c.User.SchoolId == _repository.GetUserSchoolId(userId))); // School-shared courses
        }
        else
        {
            // "Only my own objects + public": Exclude Team unless explicitly requested
            query = query.Where(c => c.UserId == userId || c.Visibility == VisibilityType.Public);
        }

        var courses = await query.ToListAsync();
        if (!courses.Any())
        {
            _logger.LogInformation("No courses found for User ID: {UserId} with Filter: {Filter}, Visibility: {Visibility}", userId, filter, visibility);
        }
        else
        {
            _logger.LogDebug("Found {Count} courses for User ID: {UserId}", courses.Count, userId);
        }

        var resource = _mapper.Map<IEnumerable<CourseResource>>(courses ?? new List<Course>());
        return resource;
    }

    public async Task<CourseResource> GetByIdAsync(int id, int userId)
    {
        _logger.LogDebug("Fetching course with ID: {CourseId} for User ID: {UserId}", id, userId);
        var course = await _repository.GetByIdAsync(id, q => q
            .Include(c => c.Topics)
                .ThenInclude(t => t.Lessons)
            .Include(c => c.User)); // Include User for SchoolId check

        if (course == null)
        {
            _logger.LogWarning("Course with ID: {CourseId} not found", id);
            return null;
        }

        // Check visibility: owned, public, or same school for Team visibility
        if (course.UserId != userId
            && course.Visibility != VisibilityType.Public
            && !(course.Visibility == VisibilityType.Team && course.User.SchoolId != null && course.User.SchoolId == _repository.GetUserSchoolId(userId)))
        {
            _logger.LogWarning("Course with ID: {CourseId} not accessible to User ID: {UserId}", id, userId);
            return null;
        }

        _logger.LogDebug("Course with ID: {CourseId} retrieved successfully for User ID: {UserId}", id, userId);
        return _mapper.Map<CourseResource>(course);
    }

    public async Task AddAsync(CourseCreateResource courseCreateResource, int userId)
    {
        _logger.LogDebug("Adding course: {Title} for User ID: {UserId}", courseCreateResource.Title, userId);
        var course = _mapper.Map<Course>(courseCreateResource);
        course.UserId = userId;
        course.Archived = false;
        await _repository.AddAsync(course);
        _logger.LogInformation("Course added with ID: {CourseId}, Title: {Title}", course.Id, course.Title);
    }

    public async Task UpdateAsync(CourseUpdateResource courseUpdateResource, int userId)
    {
        _logger.LogDebug("Updating course with ID: {CourseId} for User ID: {UserId}", courseUpdateResource.Id, userId);
        var existingCourse = await _repository.GetByIdAsync(courseUpdateResource.Id);
        if (existingCourse == null || existingCourse.UserId != userId)
        {
            _logger.LogWarning("Course with ID: {CourseId} not found or not owned by User ID: {UserId}", courseUpdateResource.Id, userId);
            throw new ArgumentException("Course not found or not owned by user");
        }

        _mapper.Map(courseUpdateResource, existingCourse);
        await _repository.UpdateAsync(existingCourse);
        _logger.LogInformation("Course with ID: {CourseId} updated successfully", existingCourse.Id);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        _logger.LogDebug("Deleting course with ID: {CourseId} for User ID: {UserId}", id, userId);
        var course = await _repository.GetByIdAsync(id);
        if (course == null || course.UserId != userId)
        {
            _logger.LogWarning("Course with ID: {CourseId} not found or not owned by User ID: {UserId}", id, userId);
            throw new ArgumentException("Course not found or not owned by user");
        }

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Course with ID: {CourseId} deleted successfully", id);
    }
}