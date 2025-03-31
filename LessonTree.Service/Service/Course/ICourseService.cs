using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;

namespace LessonTree.BLL.Service
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active, int? visibility = null);
        Task<CourseResource> GetByIdAsync(int id, int userId);
        Task AddAsync(CourseCreateResource courseCreateResource, int userId);
        Task UpdateAsync(CourseUpdateResource courseUpdateResource, int userId);
        Task DeleteAsync(int id, int userId);
    }
}