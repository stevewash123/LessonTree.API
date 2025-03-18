using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResource>> GetAllAsync();
        Task<CourseResource> GetByIdAsync(int id);
        Task AddAsync(CourseCreateResource courseCreateResource);
        Task UpdateAsync(CourseUpdateResource courseUpdateResource);
        Task DeleteAsync(int id);
    }
}