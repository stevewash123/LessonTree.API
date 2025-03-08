using System;
using System.Collections.Generic;
using LessonTree.DAL.Domain;

namespace LessonTree.DAL.Repositories
{
    public interface IDocumentRepository
    {
        void Add(Document document);
    }
}
