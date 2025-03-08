using System.Collections.Generic;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public interface ISubTopicService
    {
        SubTopicResource GetById(int id);
        List<SubTopicResource> GetAll();
        void Add(SubTopicCreateResource subTopicCreateResource);
        void Update(SubTopicUpdateResource subTopicUpdateResource);
        void Delete(int id);
        void MoveSubTopic(int subTopicId, int newTopicId);
        SubTopicResource CopySubTopic(int subTopicId, int newTopicId);
    }
}