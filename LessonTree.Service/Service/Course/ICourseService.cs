using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public interface ICourseService
    {
        IEnumerable<CourseResource> GetAll();
        CourseResource GetById(int id);
        void Add(CourseCreateResource courseCreateResource); // Updated to use DTO
        void Update(CourseUpdateResource courseUpdateResource); // Updated to use DTO
        void Delete(int id);
    }
}