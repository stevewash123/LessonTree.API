using System;
using System.Collections.Generic;
using LessonTree.DAL.Domain;

namespace LessonTree.DAL.Repositories
{
    public interface IAttachmentRepository
    {
        void Add(Attachment document);
    }
}
