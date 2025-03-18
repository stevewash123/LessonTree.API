using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.EntityFrameworkCore; // For Include
using AutoMapper;

namespace LessonTree.BLL.Service
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _repository;
        private readonly IMapper _mapper;

        public CourseService(ICourseRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseResource>> GetAllAsync()
        {
            var courses = await _repository.GetAll(q => q
                .Include(c => c.Topics)
                .ThenInclude(t => t.SubTopics)
                .ThenInclude(st => st.Lessons))
                .ToListAsync();

            foreach (var course in courses)
            {
                foreach (var topic in course.Topics)
                {
                    if (!topic.HasSubTopics)
                    {
                        topic.SubTopics = topic.SubTopics.Where(st => !st.IsDefault).ToList();
                    }
                }
            }

            return _mapper.Map<IEnumerable<CourseResource>>(courses);
        }

        public async Task<CourseResource> GetByIdAsync(int id)
        {
            var course = await _repository.GetByIdAsync(id, q => q
                .Include(c => c.Topics)
                .ThenInclude(t => t.SubTopics)
                .ThenInclude(st => st.Lessons));
            return _mapper.Map<CourseResource>(course ?? new Course());
        }

        public async Task AddAsync(CourseCreateResource courseCreateResource)
        {
            var course = _mapper.Map<Course>(courseCreateResource);
            await _repository.AddAsync(course);
        }

        public async Task UpdateAsync(CourseUpdateResource courseUpdateResource)
        {
            var course = _mapper.Map<Course>(courseUpdateResource);
            await _repository.UpdateAsync(course);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}