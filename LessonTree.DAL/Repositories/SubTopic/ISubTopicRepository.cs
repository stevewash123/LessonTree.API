using LessonTree.DAL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Repositories
{
    public interface ISubTopicRepository
    {
        // Query operations
        IQueryable<SubTopic> GetAll(Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null);
        Task<SubTopic?> GetByIdAsync(int id, Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null);
        Task<List<SubTopic>> GetSubTopicsByTopicIdAsync(int topicId, bool includeArchived = false);

        // CRUD operations
        Task<int> AddAsync(SubTopic subTopic);
        Task UpdateAsync(SubTopic subTopic);
        Task DeleteAsync(int id);

        // Sort order operations
        Task<int> GetMaxSortOrderInTopicAsync(int topicId);
        Task<int> GetNextSortOrderForTopicAsync(int topicId);
        Task UpdateSubTopicSortOrdersAsync(IEnumerable<SubTopic> subTopics);

        // Positioning operations - UPDATED to sibling-based approach
        Task<SubTopic> MoveSubTopicToPositionAsync(int subTopicId, int targetTopicId, int afterSiblingId, string siblingType);
        
        // ✅ NEW: Positioning with complete positioning contract
        Task<SubTopic> MoveSubTopicWithPositioningAsync(int subTopicId, int targetTopicId, int relativeToId, string position, string relativeToType);

        // Validation helpers
        Task<bool> IsSubTopicInTopicAsync(int subTopicId, int topicId);
        Task<bool> IsLessonInTopicAsync(int lessonId, int topicId);
    }
}
