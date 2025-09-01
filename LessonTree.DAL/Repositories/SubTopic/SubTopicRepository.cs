// **COMPLETE FILE** - SubTopicRepository.cs - Standardized to enterprise patterns
// RESPONSIBILITY: SubTopic data access with topic relationships and default handling.  Atomic database transactions, sort order calculations for mixed containers
// DOES NOT: Handle subtopic content validation or business rules (that's in services)
// CALLED BY: SubTopicService for all subtopic operations and positional move operations

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

        // **UPDATED METHOD** - SubTopicRepository.cs - Replace MoveSubTopicToPositionAsync method
        // RESPONSIBILITY: Sibling-based positioning with atomic database transactions
        // DOES NOT: Handle business logic validation (that's in services)
        // CALLED BY: SubTopicService.MoveSubTopicToPositionAsync

        public async Task<SubTopic> MoveSubTopicToPositionAsync(int subTopicId, int targetTopicId, int afterSiblingId, string siblingType)
        {
            _logger.LogInformation($"MoveSubTopicToPositionAsync: Moving subtopic {subTopicId} after {siblingType} {afterSiblingId} in topic {targetTopicId}");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get the subtopic to move
                var subTopic = await GetByIdAsync(subTopicId);
                if (subTopic == null)
                {
                    throw new ArgumentException($"SubTopic {subTopicId} not found");
                }

                // Get all items in target topic (subtopics + direct lessons)
                var topicItems = await GetTopicItemsAsync(targetTopicId);

                // Calculate target position based on sibling
                var targetSortOrder = CalculateTargetSortOrderFromSibling(topicItems, afterSiblingId, siblingType);

                _logger.LogInformation($"MoveSubTopicToPositionAsync: Calculated target sort order {targetSortOrder} for subtopic {subTopicId}");

                // Update subtopic topic and position
                subTopic.TopicId = targetTopicId;
                subTopic.SortOrder = targetSortOrder;

                // Renumber all affected items to prevent collisions
                await RenumberTopicItemsForInsertionAsync(topicItems, subTopicId, targetSortOrder, "SubTopic");

                // Save the moved subtopic
                _context.SubTopics.Update(subTopic);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation($"MoveSubTopicToPositionAsync: Successfully moved subtopic {subTopicId} to position {targetSortOrder}");

                return subTopic;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ✅ NEW: Complete positioning-aware move with position parameter
        public async Task<SubTopic> MoveSubTopicWithPositioningAsync(int subTopicId, int targetTopicId, int relativeToId, string position, string relativeToType)
        {
            _logger.LogInformation($"🔍 MoveSubTopicWithPositioningAsync: Moving subtopic {subTopicId} {position} {relativeToType} {relativeToId} in topic {targetTopicId}");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get the subtopic to move
                var subTopic = await _context.SubTopics.FirstOrDefaultAsync(s => s.Id == subTopicId);
                if (subTopic == null) throw new ArgumentException($"SubTopic {subTopicId} not found");

                // Get all items in the topic and calculate new sort order based on position
                var topicItems = await GetTopicItemsAsync(targetTopicId);
                int targetSortOrder = CalculateTargetSortOrderWithPosition(topicItems, relativeToId, position, relativeToType);

                _logger.LogInformation($"🎯 Calculated target sort order {targetSortOrder} for subtopic {subTopicId} ({position} {relativeToType} {relativeToId})");

                // Update subtopic topic and position
                subTopic.TopicId = targetTopicId;
                subTopic.SortOrder = targetSortOrder;

                // Renumber other items to make space for insertion at target position
                await RenumberTopicItemsForInsertionAsync(topicItems, subTopicId, targetSortOrder, "SubTopic");

                // Save changes
                _context.SubTopics.Update(subTopic);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation($"✅ Successfully moved subtopic {subTopicId} to position {targetSortOrder} ({position} {relativeToType} {relativeToId})");

                return subTopic;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"❌ Failed to move subtopic {subTopicId} with positioning");
                throw;
            }
        }

        private int CalculateTargetSortOrderWithPosition(List<TopicItem> topicItems, int relativeToId, string position, string relativeToType)
        {
            var relativeItem = topicItems.FirstOrDefault(i => i.Id == relativeToId && i.Type == relativeToType);
            if (relativeItem == null)
            {
                _logger.LogWarning($"⚠️ Relative entity {relativeToType} {relativeToId} not found, placing at end");
                return (topicItems.Any() ? topicItems.Max(i => i.SortOrder) : 0) + 1;
            }

            if (position == "before")
            {
                // Position before the relative item (same SortOrder, renumbering will handle the rest)
                var targetOrder = relativeItem.SortOrder;
                _logger.LogInformation($"🔍 Before positioning: targeting SortOrder {targetOrder} (relative item has {relativeItem.SortOrder})");
                return targetOrder;
            }
            else if (position == "after")
            {
                // Position after the relative item
                var targetOrder = relativeItem.SortOrder + 1;
                _logger.LogInformation($"🔍 After positioning: targeting SortOrder {targetOrder} (relative item has {relativeItem.SortOrder})");
                return targetOrder;
            }
            else
            {
                _logger.LogWarning($"⚠️ Unknown position '{position}', placing at end");
                return (topicItems.Any() ? topicItems.Max(i => i.SortOrder) : 0) + 1;
            }
        }

        private int CalculateTargetSortOrderFromSibling(List<TopicItem> topicItems, int afterSiblingId, string siblingType)
        {
            var siblingItem = topicItems.FirstOrDefault(i => i.Id == afterSiblingId && i.Type == siblingType);
            if (siblingItem != null)
            {
                // Position after the sibling
                return siblingItem.SortOrder + 1;
            }

            // Fallback: append to end
            return topicItems.Any() ? topicItems.Max(i => i.SortOrder) + 1 : 0;
        }

        public async Task<int> GetMaxSortOrderInTopicAsync(int topicId)
        {
            // Check both subtopics and direct lessons to get the maximum sort order
            var maxSubTopicSort = await _context.SubTopics
                .Where(st => st.TopicId == topicId && !st.Archived)
                .MaxAsync(st => (int?)st.SortOrder) ?? -1;

            var maxLessonSort = await _context.Lessons
                .Where(l => l.TopicId == topicId && l.SubTopicId == null && !l.Archived)
                .MaxAsync(l => (int?)l.SortOrder) ?? -1;

            return Math.Max(maxSubTopicSort, maxLessonSort);
        }

        private async Task<List<TopicItem>> GetTopicItemsAsync(int topicId)
        {
            var items = new List<TopicItem>();

            // Get subtopics
            var subTopics = await _context.SubTopics
                .Where(st => st.TopicId == topicId && !st.Archived)
                .OrderBy(st => st.SortOrder)
                .ToListAsync();

            items.AddRange(subTopics.Select(st => new TopicItem
            {
                Id = st.Id,
                SortOrder = st.SortOrder,
                Type = "SubTopic"
            }));

            // Get direct lessons (not in subtopics)
            var directLessons = await _context.Lessons
                .Where(l => l.TopicId == topicId && l.SubTopicId == null && !l.Archived)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();

            items.AddRange(directLessons.Select(l => new TopicItem
            {
                Id = l.Id,
                SortOrder = l.SortOrder,
                Type = "Lesson"
            }));

            return items.OrderBy(i => i.SortOrder).ToList();
        }

        private int CalculateTargetSortOrder(List<TopicItem> topicItems, int AfterSiblingId, string position, string relativeToType)
        {
            var relativeItem = topicItems.FirstOrDefault(i => i.Id == AfterSiblingId && i.Type == relativeToType);
            if (relativeItem != null)
            {
                return position == "before" ? relativeItem.SortOrder : relativeItem.SortOrder + 1;
            }

            // Fallback: append to end
            return topicItems.Any() ? topicItems.Max(i => i.SortOrder) + 1 : 0;
        }

        private async Task RenumberTopicItemsForInsertionAsync(List<TopicItem> topicItems, int movedItemId, int targetSortOrder, string movedItemType)
        {
            // Filter out the moved item from existing items
            var otherItems = topicItems.Where(i => !(i.Id == movedItemId && i.Type == movedItemType)).ToList();

            // Sort items and renumber everything to create clean sequential order with gap at target position
            var sortedItems = otherItems.OrderBy(i => i.SortOrder).ToList();
            var newSortOrder = 0;

            foreach (var item in sortedItems)
            {
                // Skip the target position - this creates the gap for the moved item
                if (newSortOrder == targetSortOrder)
                {
                    newSortOrder++;
                }

                // Only update if the sort order actually changed
                if (item.SortOrder != newSortOrder)
                {
                    if (item.Type == "SubTopic")
                    {
                        var subTopic = await _context.SubTopics.FindAsync(item.Id);
                        if (subTopic != null)
                        {
                            subTopic.SortOrder = newSortOrder;
                            _context.SubTopics.Update(subTopic);
                            _logger.LogDebug($"RenumberTopicItemsAsync: Renumbered SubTopic {item.Id} from {item.SortOrder} to {newSortOrder}");
                        }
                    }
                    else if (item.Type == "Lesson")
                    {
                        var lesson = await _context.Lessons.FindAsync(item.Id);
                        if (lesson != null)
                        {
                            lesson.SortOrder = newSortOrder;
                            _context.Lessons.Update(lesson);
                            _logger.LogDebug($"RenumberTopicItemsAsync: Renumbered Lesson {item.Id} from {item.SortOrder} to {newSortOrder}");
                        }
                    }
                }

                newSortOrder++;
            }
        }

        // **PARTIAL FILE** - SubTopicRepository.cs - Add helper methods for positioning support
        // RESPONSIBILITY: Helper methods for atomic database transactions and mixed container support
        // DOES NOT: Handle business logic validation (that's in services) 
        // CALLED BY: SubTopicRepository positioning methods

        // Add these helper methods to SubTopicRepository.cs:

        /// <summary>
        /// Get all subtopics in a topic ordered by sort order
        /// </summary>
        public async Task<List<SubTopic>> GetSubTopicsByTopicIdAsync(int topicId, bool includeArchived = false)
        {
            _logger.LogInformation($"GetSubTopicsByTopicIdAsync: Fetching subtopics for topic {topicId}");

            var query = _context.SubTopics.Where(st => st.TopicId == topicId);

            if (!includeArchived)
            {
                query = query.Where(st => !st.Archived);
            }

            var subTopics = await query
                .OrderBy(st => st.SortOrder)
                .ToListAsync();

            _logger.LogInformation($"GetSubTopicsByTopicIdAsync: Found {subTopics.Count} subtopics for topic {topicId}");
            return subTopics;
        }

        /// <summary>
        /// Check if a subtopic belongs to a specific topic
        /// </summary>
        public async Task<bool> IsSubTopicInTopicAsync(int subTopicId, int topicId)
        {
            return await _context.SubTopics
                .AnyAsync(st => st.Id == subTopicId && st.TopicId == topicId && !st.Archived);
        }

        /// <summary>
        /// Get the next available sort order for a topic (considering both subtopics and direct lessons)
        /// </summary>
        public async Task<int> GetNextSortOrderForTopicAsync(int topicId)
        {
            var maxSortOrder = await GetMaxSortOrderInTopicAsync(topicId);
            return maxSortOrder + 1;
        }

        /// <summary>
        /// Update multiple subtopics' sort orders in a single transaction
        /// </summary>
        public async Task UpdateSubTopicSortOrdersAsync(IEnumerable<SubTopic> subTopics)
        {
            foreach (var subTopic in subTopics)
            {
                _context.SubTopics.Update(subTopic);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Check if a lesson belongs to a specific topic (either directly or through subtopic)
        /// </summary>
        public async Task<bool> IsLessonInTopicAsync(int lessonId, int topicId)
        {
            return await _context.Lessons
                .AnyAsync(l => l.Id == lessonId &&
                              (l.TopicId == topicId || l.SubTopic.TopicId == topicId) &&
                              !l.Archived);
        }

        // Helper class for mixed container management
        private class TopicItem
        {
            public int Id { get; set; }
            public int SortOrder { get; set; }
            public string Type { get; set; } // "SubTopic" or "Lesson"
        }


    }
}