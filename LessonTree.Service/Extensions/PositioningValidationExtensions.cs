// **NEW FILE** - PositioningValidationExtensions.cs - Domain validation helpers
// RESPONSIBILITY: Domain object validation extensions for positioning operations
// DOES NOT: Handle repository or service logic
// CALLED BY: Services for validation logic

using LessonTree.DAL.Domain;

namespace LessonTree.BLL.Extensions
{
    public static class PositioningValidationExtensions
    {
        /// <summary>
        /// Validates that a Topic can be moved to a specific Course
        /// </summary>
        public static bool CanMoveToCourse(this Topic topic, Course targetCourse, int userId)
        {
            if (topic == null || targetCourse == null)
                return false;

            // User must own both the topic and target course
            if (topic.UserId != userId || targetCourse.UserId != userId)
                return false;

            // Cannot move to same course (no-op)
            if (topic.CourseId == targetCourse.Id)
                return false;

            return true;
        }

        /// <summary>
        /// Validates that a SubTopic can be moved to a specific Topic
        /// </summary>
        public static bool CanMoveToTopic(this SubTopic subTopic, Topic targetTopic, int userId)
        {
            if (subTopic == null || targetTopic == null)
                return false;

            // User must own both the subtopic and target topic
            if (subTopic.UserId != userId || targetTopic.UserId != userId)
                return false;

            // Cannot move to same topic (no-op)
            if (subTopic.TopicId == targetTopic.Id)
                return false;

            return true;
        }

        /// <summary>
        /// Validates position parameters for Topic positioning
        /// </summary>
        public static bool IsValidTopicPosition(string position, string relativeToType)
        {
            if (string.IsNullOrEmpty(position) || string.IsNullOrEmpty(relativeToType))
                return false;

            var validPositions = new[] { "before", "after" };
            var validRelativeTypes = new[] { "Topic" };

            return validPositions.Contains(position.ToLower()) &&
                   validRelativeTypes.Contains(relativeToType);
        }

        /// <summary>
        /// Validates position parameters for SubTopic positioning
        /// </summary>
        public static bool IsValidSubTopicPosition(string position, string relativeToType)
        {
            if (string.IsNullOrEmpty(position) || string.IsNullOrEmpty(relativeToType))
                return false;

            var validPositions = new[] { "before", "after" };
            var validRelativeTypes = new[] { "SubTopic", "Lesson" };

            return validPositions.Contains(position.ToLower()) &&
                   validRelativeTypes.Contains(relativeToType);
        }

        /// <summary>
        /// Checks if a sort order is valid (non-negative)
        /// </summary>
        public static bool IsValidSortOrder(int sortOrder)
        {
            return sortOrder >= 0;
        }
    }
}