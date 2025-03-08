using System.Collections.Generic;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public interface ITopicService
    {
        TopicResource GetById(int id);
        List<TopicResource> GetAll();
        void Add(TopicCreateResource topicCreateResource);
        void Update(TopicUpdateResource topicUpdateResource);
        void Delete(int id);
        void MoveTopic(int topicId, int newCourseId);
        TopicResource CopyTopic(int topicId, int newCourseId);
    }
}