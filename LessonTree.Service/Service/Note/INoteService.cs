using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public interface INoteService
    {
        Task<NoteResource> CreateNoteAsync(NoteCreateResource noteCreateResource, int userId);
        Task<NoteResource?> UpdateNoteAsync(int id, NoteUpdateResource noteUpdateResource, int userId);
        Task DeleteNoteAsync(int id, int userId);
    }
}