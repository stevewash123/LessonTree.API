// File: MappingProfile.cs
using AutoMapper;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Note to NoteResource
        CreateMap<Note, NoteResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.Id))
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.CreatedBy.LastName)
                        ? $"{src.CreatedBy.LastName}, {src.CreatedBy.FirstName}"
                        : src.CreatedBy.UserName))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId))
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId));


        CreateMap<NoteCreateResource, Note>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility)) // Direct mapping
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId))
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId));

        CreateMap<NoteUpdateResource, Note>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility)); // Direct mapping

        // Course to CourseResource
        CreateMap<Course, CourseResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"course_{src.Id}"))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Topics, opt => opt.MapFrom(src => src.Topics))
            .ForMember(dest => dest.HasChildren, opt => opt.MapFrom(src => src.Topics.Any()))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

        // Topic to TopicResource
        CreateMap<Topic, TopicResource>()
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"topic_{src.Id}"))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.SubTopics, opt => opt.MapFrom(src => src.SubTopics))
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons))
            .ForMember(dest => dest.HasChildren, opt => opt.MapFrom(src => src.SubTopics.Any() || src.Lessons.Any()))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

        // SubTopic to SubTopicResource
        CreateMap<SubTopic, SubTopicResource>()
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"subtopic_{src.Id}"))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Topic.CourseId))
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons ?? new List<Lesson>()))
            .ForMember(dest => dest.HasChildren, opt => opt.MapFrom(src => src.Lessons.Any()))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

        // Lesson to LessonResource
        CreateMap<Lesson, LessonResource>()
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"lesson_{src.Id}"))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.SubTopic != null ? src.SubTopic.Topic.CourseId : src.Topic.CourseId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Objective, opt => opt.MapFrom(src => src.Objective))
            .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId));

        // Lesson to LessonDetailResource
        CreateMap<Lesson, LessonDetailResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.SubTopic != null ? src.SubTopic.Topic.CourseId : src.Topic.CourseId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level))
            .ForMember(dest => dest.Objective, opt => opt.MapFrom(src => src.Objective))
            .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.Materials))
            .ForMember(dest => dest.ClassTime, opt => opt.MapFrom(src => src.ClassTime))
            .ForMember(dest => dest.Methods, opt => opt.MapFrom(src => src.Methods))
            .ForMember(dest => dest.SpecialNeeds, opt => opt.MapFrom(src => src.SpecialNeeds))
            .ForMember(dest => dest.Assessment, opt => opt.MapFrom(src => src.Assessment))
            .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.LessonAttachments.Select(ld => ld.Attachment).ToList()))
            .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => src.LessonStandards.Select(ls => ls.Standard)))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

        // Other mappings unchanged...
        CreateMap<Attachment, AttachmentResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
            .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.ContentType))
            .ReverseMap();

        CreateMap<Standard, StandardResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.StandardType, opt => opt.MapFrom(src => src.StandardType));

        CreateMap<StandardCreateResource, Standard>();
        CreateMap<StandardUpdateResource, Standard>();
        CreateMap<CourseCreateResource, Course>().ReverseMap();
        CreateMap<CourseUpdateResource, Course>().ReverseMap();
        CreateMap<TopicCreateResource, Topic>().ReverseMap();
        CreateMap<TopicUpdateResource, Topic>().ReverseMap();
        CreateMap<SubTopicCreateResource, SubTopic>().ReverseMap();
        CreateMap<SubTopicUpdateResource, SubTopic>().ReverseMap();
        CreateMap<LessonCreateResource, Lesson>().ReverseMap();
        CreateMap<LessonUpdateResource, Lesson>().ReverseMap();
    }
}
public class NoteParentResolver : IValueResolver<Note, NoteResource, string>
{
    public string Resolve(Note source, NoteResource destination, string destMember, ResolutionContext context)
    {
        if (source.CourseId.HasValue)
        {
            return "Course";
        }
        if (source.TopicId.HasValue)
        {
            return "Topic";
        }
        if (source.SubTopicId.HasValue)
        {
            return "SubTopic";
        }
        if (source.LessonId.HasValue)
        {
            return "Lesson";
        }
        throw new Exception("Note must have one and only one parent.");
    }
}