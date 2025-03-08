using AutoMapper;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Course, CourseResource>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"course_{src.Id}")) // Unique nodeId for course
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // Preserve original ID as Id
                .ForMember(dest => dest.Topics, opt => opt.MapFrom(src => src.Topics ?? new List<Topic>()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)); // Map Id back to Id for persistence

            CreateMap<Topic, TopicResource>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"topic_{src.Id}")) // Unique nodeId for topic
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // Preserve original ID as Id
                .ForMember(dest => dest.SubTopics, opt => opt.MapFrom(src => src.SubTopics ?? new List<SubTopic>()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)); // Map Id back to Id for persistence

            CreateMap<SubTopic, SubTopicResource>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"subtopic_{src.Id}")) // Unique nodeId for subtopic
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // Preserve original ID as Id
                .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons ?? new List<Lesson>()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)); // Map Id back to Id for persistence

            CreateMap<Lesson, LessonResource>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"lesson_{src.Id}"));

            CreateMap<Lesson, LessonDetailResource>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // Preserve original ID as Id
                .ForMember(dest => dest.SubTopic, opt => opt.MapFrom(src => src.SubTopic))
                .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.LessonDocuments.Select(ld => ld.Document).ToList()))
                .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => src.LessonStandards.Select(ls => ls.Standard)));

            CreateMap<Document, DocumentResource>();
            CreateMap<Standard, StandardResource>(); 
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