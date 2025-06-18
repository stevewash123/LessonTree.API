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
        _logger.LogInformation($"GetAllAsync: Fetching courses for user {userId}, filter: {filter}, visibility: {visibility}");

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
            _logger.LogInformation($"GetAllAsync: No courses found for user {userId} with filter: {filter}, visibility: {visibility}");
        }
        else
        {
            _logger.LogInformation($"GetAllAsync: Found {courses.Count} courses for user {userId}");
        }

        var resource = _mapper.Map<IEnumerable<CourseResource>>(courses ?? new List<Course>());
        return resource;
    }

    public async Task<CourseResource> GetByIdAsync(int id, int userId)
    {
        _logger.LogInformation($"GetByIdAsync: Fetching course {id} for user {userId}");

        var course = await _repository.GetByIdAsync(id, q => q
            .Include(c => c.Topics)
                .ThenInclude(t => t.Lessons)
            .Include(c => c.User)); // Include User for SchoolId check

        if (course == null)
        {
            _logger.LogInformation($"GetByIdAsync: Course {id} not found");
            return null;
        }

        // Check visibility: owned, public, or same school for Team visibility
        if (course.UserId != userId
            && course.Visibility != VisibilityType.Public
            && !(course.Visibility == VisibilityType.Team && course.User.SchoolId != null && course.User.SchoolId == _repository.GetUserSchoolId(userId)))
        {
            _logger.LogWarning($"GetByIdAsync: Course {id} not accessible to user {userId}");
            return null;
        }

        _logger.LogInformation($"GetByIdAsync: Found course {id} for user {userId}");
        return _mapper.Map<CourseResource>(course);
    }

    public async Task AddAsync(CourseCreateResource courseCreateResource, int userId)
    {
        _logger.LogInformation($"AddAsync: Creating course '{courseCreateResource.Title}' for user {userId}");

        var course = _mapper.Map<Course>(courseCreateResource);
        course.UserId = userId;
        course.Archived = false;
        await _repository.AddAsync(course);

        _logger.LogInformation($"AddAsync: Created course {course.Id} '{course.Title}' for user {userId}");
    }

    public async Task UpdateAsync(CourseUpdateResource courseUpdateResource, int userId)
    {
        _logger.LogInformation($"UpdateAsync: Updating course {courseUpdateResource.Id} for user {userId}");

        var existingCourse = await _repository.GetByIdAsync(courseUpdateResource.Id);
        if (existingCourse == null || existingCourse.UserId != userId)
        {
            _logger.LogWarning($"UpdateAsync: Course {courseUpdateResource.Id} not found or not owned by user {userId}");
            throw new ArgumentException($"Course {courseUpdateResource.Id} not found or not owned by user");
        }

        _mapper.Map(courseUpdateResource, existingCourse);
        await _repository.UpdateAsync(existingCourse);

        _logger.LogInformation($"UpdateAsync: Updated course {existingCourse.Id} for user {userId}");
    }

    public async Task DeleteAsync(int id, int userId)
    {
        _logger.LogInformation($"DeleteAsync: Deleting course {id} for user {userId}");

        var course = await _repository.GetByIdAsync(id);
        if (course == null || course.UserId != userId)
        {
            _logger.LogWarning($"DeleteAsync: Course {id} not found or not owned by user {userId}");
            throw new ArgumentException($"Course {id} not found or not owned by user");
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation($"DeleteAsync: Deleted course {id} for user {userId}");
    }
}