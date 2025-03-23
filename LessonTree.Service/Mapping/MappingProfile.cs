using AutoMapper;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Course to CourseResource
            CreateMap<Course, CourseResource>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"course_{src.Id}"))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.hasChildren, opt => opt.MapFrom(src => src.Topics.Any()));
            //.ForMember(dest => dest.Topics, opt => opt.MapFrom(src => src.Topics));

            // Mapping from Topic to TopicResource
            CreateMap<Topic, TopicResource>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"topic_{src.Id}"))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.hasChildren, opt => opt.MapFrom(src => src.HasSubTopics ? src.SubTopics.Any() : src.SubTopics.SelectMany(X => X.Lessons).Any()));
            //.ForMember(dest => dest.SubTopics, opt => opt.MapFrom(src => src.HasSubTopics ? src.SubTopics.Where(x => !x.IsDefault) : new List<SubTopic>()))
            //.ForMember(dest => dest.Lessons, opt => opt.MapFrom(src =>
            //src.HasSubTopics
            //    ? new List<Lesson>()
            //    : (src.SubTopics != null
            //        ? src.SubTopics.SelectMany(st => st.Lessons ?? new List<Lesson>()).ToList()
            //        : new List<Lesson>())));

            // SubTopic to SubTopicResource
            CreateMap<SubTopic, SubTopicResource>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"subtopic_{src.Id}"))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
                .ForMember(dest => dest.hasChildren, opt => opt.MapFrom(src => src.Lessons.Any()))
                //.ForMember(dest => dest.Topic, opt => opt.MapFrom(src => src.Topic))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Topic.CourseId))
                //.ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons ?? new List<Lesson>()))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            // Lesson to LessonResource
            CreateMap<Lesson, LessonResource>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"lesson_{src.Id}"))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.SubTopic.Topic.CourseId))
                //.ForMember(dest => dest.SubTopic, opt => opt.MapFrom(src => src.SubTopic))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Objective, opt => opt.MapFrom(src => src.Objective));

            // Lesson to LessonDetailResource
            CreateMap<Lesson, LessonDetailResource>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.SubTopic.Topic.CourseId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level))
                .ForMember(dest => dest.Objective, opt => opt.MapFrom(src => src.Objective))
                .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.Materials))
                .ForMember(dest => dest.ClassTime, opt => opt.MapFrom(src => src.ClassTime))
                .ForMember(dest => dest.Methods, opt => opt.MapFrom(src => src.Methods))
                .ForMember(dest => dest.SpecialNeeds, opt => opt.MapFrom(src => src.SpecialNeeds))
                .ForMember(dest => dest.Assessment, opt => opt.MapFrom(src => src.Assessment))
                .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.LessonAttachments.Select(ld => ld.Attachment).ToList()))
                .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => src.LessonStandards.Select(ls => ls.Standard)));

            // Document to DocumentResource
            CreateMap<Attachment, AttachmentResource>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.ContentType))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.ContentType));

            // Standard to StandardResource
            CreateMap<Standard, StandardResource>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StandardType, opt => opt.MapFrom(src => src.StandardType));

            // Existing Create/Update Mappings (assumed sufficient)
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
}