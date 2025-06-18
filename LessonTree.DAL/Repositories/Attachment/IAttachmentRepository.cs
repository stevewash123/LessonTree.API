using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LessonTree.DAL.Domain;

namespace LessonTree.DAL.Repositories
{
    public interface IAttachmentRepository
    {
        Task<int> AddAsync(Attachment attachment);
    }
}