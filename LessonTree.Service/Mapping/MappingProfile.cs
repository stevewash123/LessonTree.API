using AutoMapper;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Course to CourseResource
        CreateMap<Course, CourseResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Topics, opt => opt.MapFrom(src => src.Topics))
            .ForMember(dest => dest.hasChildren, opt => opt.MapFrom(src => src.Topics.Any()));

        // Mapping from Topic to TopicResource
        CreateMap<Topic, TopicResource>()
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"topic_{src.Id}"))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.SubTopics, opt => opt.MapFrom(src => src.SubTopics ?? new List<SubTopic>())) // Always include all SubTopics
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons ?? new List<Lesson>())) // Always include direct Lessons
            .ForMember(dest => dest.hasChildren, opt => opt.MapFrom(src =>
                (src.SubTopics != null && src.SubTopics.Any()) || (src.Lessons != null && src.Lessons.Any()))); // Children if SubTopics or Lessons exist

        // SubTopic to SubTopicResource
        CreateMap<SubTopic, SubTopicResource>()
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"subtopic_{src.Id}"))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Topic.CourseId))
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons ?? new List<Lesson>()))
            .ForMember(dest => dest.hasChildren, opt => opt.MapFrom(src => src.Lessons != null && src.Lessons.Any()));

        // Lesson to LessonResource
        CreateMap<Lesson, LessonResource>()
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"lesson_{src.Id}"))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId ?? 0)) // Handle nullable SubTopicId
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src =>
                src.SubTopic != null ? src.SubTopic.Topic.CourseId : src.Topic.CourseId)) // Handle both SubTopic and Topic paths
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Objective, opt => opt.MapFrom(src => src.Objective));

        // Lesson to LessonDetailResource
        CreateMap<Lesson, LessonDetailResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId ?? 0)) // Handle nullable SubTopicId
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src =>
                src.SubTopic != null ? src.SubTopic.Topic.CourseId : src.Topic.CourseId)) // Handle both paths
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level))
            .ForMember(dest => dest.Objective, opt => opt.MapFrom(src => src.Objective))
            .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.Materials))
            .ForMember(dest => dest.ClassTime, opt => opt.MapFrom(src => src.ClassTime))
            .ForMember(dest => dest.Methods, opt => opt.MapFrom(src => src.Methods))
            .ForMember(dest => dest.SpecialNeeds, opt => opt.MapFrom(src => src.SpecialNeeds))
            .ForMember(dest => dest.Assessment, opt => opt.MapFrom(src => src.Assessment))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.LessonAttachments.Select(ld => ld.Attachment).ToList()))
            .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => src.LessonStandards.Select(ls => ls.Standard)));

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