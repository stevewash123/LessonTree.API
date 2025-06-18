// **COMPLETE FILE** - SubTopicRepository.cs - Standardized to enterprise patterns
// RESPONSIBILITY: SubTopic data access with topic relationships and default handling
// DOES NOT: Handle subtopic content validation or business rules (that's in services)
// CALLED BY: SubTopicService for all subtopic operations

using System;
using System.Linq;
using System.Threading.Tasks;
using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL.Repositories
{
    public class SubTopicRepository : ISubTopicRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<SubTopicRepository> _logger;

        public SubTopicRepository(LessonTreeContext context, ILogger<SubTopicRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<SubTopic> GetAll(Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null)
        {
            _logger.LogInformation("GetAll: Retrieving all subtopics");

            IQueryable<SubTopic> query = _context.SubTopics;
            if (include != null)
            {
                query = include(query);
            }
            return query;
        }

        public async Task<SubTopic?> GetByIdAsync(int id, Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null)
        {
            _logger.LogInformation($"GetByIdAsync: Fetching subtopic {id}");

            IQueryable<SubTopic> query = _context.SubTopics;
            if (include != null)
            {
                query = include(query);
            }

            var subTopic = await query.FirstOrDefaultAsync(st => st.Id == id);

            if (subTopic != null)
            {
                _logger.LogInformation($"GetByIdAsync: Found subtopic {id} for user {subTopic.UserId}");
            }
            else
            {
                _logger.LogInformation($"GetByIdAsync: SubTopic {id} not found");
            }

            return subTopic;
        }

        public async Task<int> AddAsync(SubTopic subTopic)
        {
            _logger.LogInformation($"AddAsync: Creating subtopic '{subTopic.Title}' for user {subTopic.UserId}");

            _context.SubTopics.Add(subTopic);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"AddAsync: Created subtopic {subTopic.Id} for user {subTopic.UserId}");
            return subTopic.Id;
        }

        public async Task UpdateAsync(SubTopic subTopic)
        {
            _logger.LogInformation($"UpdateAsync: Updating subtopic {subTopic.Id}");

            _context.SubTopics.Update(subTopic);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"UpdateAsync: Updated subtopic {subTopic.Id}");
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"DeleteAsync: Deleting subtopic {id}");

            var subTopic = await _context.SubTopics.FindAsync(id);
            if (subTopic == null)
            {
                throw new ArgumentException($"SubTopic {id} not found");
            }

            _context.SubTopics.Remove(subTopic);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"DeleteAsync: Deleted subtopic {id}");
        }
    }
}