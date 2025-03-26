using LessonTree.DAL.Domain;
using System.Linq;

namespace LessonTree.DAL.Repositories
{
    public interface ILessonRepository
    {
        IQueryable<Lesson> GetAll(Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null);
        Task<Lesson?> GetByIdAsync(int id, Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null);
        Task<int> AddAsync(Lesson lesson);
        Task UpdateAsync(Lesson lesson);
        Task DeleteAsync(int id);
        Task AddAttachmentAsync(int lessonId, int attachmentId);
        Task RemoveAttachmentAsync(int lessonId, int attachmentId);
        IQueryable<Lesson> GetByTitle(string title);
        IQueryable<Lesson> GetByTopicId(int topicId, bool includeArchived = false);
        IQueryable<Lesson> GetBySubTopicId(int subTopicId, bool includeArchived = false);
        IQueryable<Lesson> GetByUserId(int userId, bool includeArchived = false);
    }
}