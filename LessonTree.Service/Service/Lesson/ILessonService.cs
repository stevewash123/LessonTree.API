using System.Collections.Generic;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public interface ILessonService
    {
        LessonDetailResource GetById(int id);
        List<LessonResource> GetAll();
        void Add(LessonCreateResource lessonCreateResource);
        void Update(LessonUpdateResource lessonUpdateResource);
        void Delete(int id);
        void AddDocument(int lessonId, int documentId);
        void RemoveDocument(int lessonId, int documentId);
        List<LessonResource> GetByTitle(string title);
        void MoveLesson(int lessonId, int newSubTopicId);
        void AddStandardToLesson(int lessonId, int standardId);
        void RemoveStandardFromLesson(int lessonId, int standardId);
        LessonResource CopyLesson(int lessonId, int newSubTopicId);
    }
}