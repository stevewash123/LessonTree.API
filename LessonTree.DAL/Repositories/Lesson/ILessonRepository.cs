using System;
using System.Collections.Generic;
using System.Linq;
using LessonTree.DAL.Domain;

namespace LessonTree.DAL.Repositories
{
    public interface ILessonRepository
    {
        IQueryable<Lesson> GetAll(Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null);
        Lesson GetById(int id, Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null);
        void Add(Lesson lesson);
        void Update(Lesson lesson);
        void Delete(int id);
        void AddDocument(int lessonId, int documentId);
        void RemoveDocument(int lessonId, int documentId);
        List<Lesson> GetByTitle(string title);
    }
}