// Full File
using AutoMapper;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using Microsoft.EntityFrameworkCore;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;
    private readonly IMapper _mapper;

    public CourseService(ICourseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        var query = _repository.GetAll(q => q
            .Where(c => c.UserId == userId) // Filter by userId
            .Include(c => c.Topics)
            .ThenInclude(t => t.Lessons)); // Include direct Lessons under Topics

        query = filter switch
        {
            ArchiveFilter.Active => query.Where(c => !c.Archived),
            ArchiveFilter.Archived => query.Where(c => c.Archived),
            ArchiveFilter.Both => query,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
        };

        var courses = await query.ToListAsync();
        var resource = _mapper.Map<IEnumerable<CourseResource>>(courses ?? new List<Course>());
        return resource;
    }

    public async Task<CourseResource> GetByIdAsync(int id, int userId)
    {
        var course = await _repository.GetByIdAsync(id, q => q
            .Include(c => c.Topics)
            .ThenInclude(t => t.Lessons)); // Include direct Lessons under Topics

        if (course == null || course.UserId != userId)
        {
            return null; // Return null if not found or not owned by user
        }

        return _mapper.Map<CourseResource>(course);
    }

    public async Task AddAsync(CourseCreateResource courseCreateResource, int userId)
    {
        var course = _mapper.Map<Course>(courseCreateResource);
        course.UserId = userId; // Set the userId for the new course
        course.Archived = false; // Default to active (not archived) on creation
        await _repository.AddAsync(course);
    }

    public async Task UpdateAsync(CourseUpdateResource courseUpdateResource, int userId)
    {
        var existingCourse = await _repository.GetByIdAsync(courseUpdateResource.Id);
        if (existingCourse == null || existingCourse.UserId != userId)
        {
            throw new ArgumentException("Course not found or not owned by user");
        }

        _mapper.Map(courseUpdateResource, existingCourse);
        await _repository.UpdateAsync(existingCourse);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var course = await _repository.GetByIdAsync(id);
        if (course == null || course.UserId != userId)
        {
            throw new ArgumentException("Course not found or not owned by user");
        }

        await _repository.DeleteAsync(id);
    }
}