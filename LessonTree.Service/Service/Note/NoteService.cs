using AutoMapper;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.Extensions.Logging;

public class NoteService : INoteService
{
    private readonly INotesRepository _notesRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<NoteService> _logger;

    public NoteService(INotesRepository notesRepository, IUserRepository userRepository, IMapper mapper, ILogger<NoteService> logger)
    {
        _notesRepository = notesRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<NoteResource> CreateNoteAsync(NoteCreateResource noteCreateResource, int userId)
    {
        _logger.LogInformation("CreateNoteAsync: Creating note for User ID: {UserId}", userId);

        // Validate that exactly one parent ID is provided
        ValidateParentIds(noteCreateResource);

        var note = _mapper.Map<Note>(noteCreateResource);
        note.CreatedBy = _userRepository.GetById(userId);

        int noteId = await _notesRepository.AddAsync(note);

        // Get the created note to return complete data
        var createdNote = await _notesRepository.GetByIdAsync(noteId);
        if (createdNote == null)
        {
            _logger.LogError("Failed to retrieve created note with ID: {NoteId}", noteId);
            throw new InvalidOperationException("Failed to retrieve created note");
        }

        _logger.LogInformation("CreateNoteAsync: Note created with ID: {NoteId} for User ID: {UserId}", noteId, userId);
        return _mapper.Map<NoteResource>(createdNote);
    }

    public async Task<NoteResource?> UpdateNoteAsync(int id, NoteUpdateResource noteUpdateResource, int userId)
    {
        _logger.LogInformation("UpdateNoteAsync: Updating note with ID: {NoteId} for User ID: {UserId}", id, userId);

        var existingNote = await _notesRepository.GetByIdAsync(id);
        if (existingNote == null)
        {
            _logger.LogWarning("Note with ID: {NoteId} not found for update", id);
            return null;
        }

        // Note: Could add ownership validation here if needed in the future
        // if (existingNote.CreatedBy?.Id != userId) { throw new UnauthorizedAccessException(); }

        _mapper.Map(noteUpdateResource, existingNote);
        await _notesRepository.UpdateAsync(existingNote);

        _logger.LogInformation("UpdateNoteAsync: Note with ID: {NoteId} updated successfully", id);
        return _mapper.Map<NoteResource>(existingNote);
    }

    public async Task DeleteNoteAsync(int id, int userId)
    {
        _logger.LogInformation("DeleteNoteAsync: Deleting note with ID: {NoteId} for User ID: {UserId}", id, userId);

        var existingNote = await _notesRepository.GetByIdAsync(id);
        if (existingNote == null)
        {
            _logger.LogWarning("Note with ID: {NoteId} not found for deletion", id);
            throw new ArgumentException("Note not found");
        }

        // Note: Could add ownership validation here if needed in the future
        // if (existingNote.CreatedBy?.Id != userId) { throw new UnauthorizedAccessException(); }

        await _notesRepository.DeleteAsync(id);
        _logger.LogInformation("DeleteNoteAsync: Note with ID: {NoteId} deleted successfully", id);
    }

    // Private helper methods

    private void ValidateParentIds(NoteCreateResource noteCreateResource)
    {
        int parentCount = (noteCreateResource.CourseId.HasValue ? 1 : 0) +
                         (noteCreateResource.TopicId.HasValue ? 1 : 0) +
                         (noteCreateResource.SubTopicId.HasValue ? 1 : 0) +
                         (noteCreateResource.LessonId.HasValue ? 1 : 0);

        if (parentCount != 1)
        {
            _logger.LogError("Invalid note creation: Exactly one parent ID must be provided. Parent count: {ParentCount}", parentCount);
            throw new ArgumentException("Exactly one parent ID (CourseId, TopicId, SubTopicId, or LessonId) must be provided.");
        }

        _logger.LogDebug("Note parent validation passed. Parent count: {ParentCount}", parentCount);
    }
}