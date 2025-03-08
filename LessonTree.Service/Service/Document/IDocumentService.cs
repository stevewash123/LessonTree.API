using LessonTree.DAL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.BLL.Service
{
    public interface IDocumentService
    {
        int CreateDocument(Document document);
        // Add other methods as needed
    }
}