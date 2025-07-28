using LessonTree.Models;
using LessonTree.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.BLL.Service
{
    public interface IEntityPositioningService
    {
        Task<EntityPositionResult> MoveLesson(LessonMoveResource request, int userId);
        Task<EntityPositionResult> MoveSubTopic(SubTopicMoveResource request, int userId);
        Task<EntityPositionResult> MoveTopic(TopicMoveResource request, int userId);
    }
}
