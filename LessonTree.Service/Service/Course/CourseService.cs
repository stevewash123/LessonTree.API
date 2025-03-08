using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.EntityFrameworkCore;
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

        public IEnumerable<CourseResource> GetAll()
        {
            var courses = _repository.GetAll(q => q
                .Include(c => c.Topics)
                .ThenInclude(t => t.SubTopics)
                .ThenInclude(st => st.Lessons))
                .ToList();
            return _mapper.Map<IEnumerable<CourseResource>>(courses ?? new List<Course>());
        }

        public CourseResource GetById(int id)
        {
            var course = _repository.GetById(id, q => q
                .Include(c => c.Topics)
                .ThenInclude(t => t.SubTopics)
                .ThenInclude(st => st.Lessons));
            return _mapper.Map<CourseResource>(course ?? new Course());
        }

        public void Add(CourseCreateResource courseCreateResource)
        {
            var course = _mapper.Map<Course>(courseCreateResource);
            _repository.Add(course);
        }

        public void Update(CourseUpdateResource courseUpdateResource)
        {
            var course = _mapper.Map<Course>(courseUpdateResource);
            _repository.Update(course);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}