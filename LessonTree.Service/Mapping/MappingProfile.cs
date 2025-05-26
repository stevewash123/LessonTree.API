// RESPONSIBILITY: AutoMapper configuration for domain entities to DTOs
// DOES NOT: Handle business logic or validation
// CALLED BY: Controllers when mapping between domain and resource models

// File: MappingProfile.cs
using AutoMapper;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // =============================================================================
        // NOTE MAPPINGS
        // =============================================================================
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
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId))
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId));

        CreateMap<NoteUpdateResource, Note>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => VisibilityConverter.ConvertStringToEnum(src.Visibility)));

        // =============================================================================
        // COURSE MAPPINGS
        // =============================================================================
        CreateMap<Course, CourseResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"course_{src.Id}"))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Topics, opt => opt.MapFrom(src => src.Topics))
            .ForMember(dest => dest.HasChildren, opt => opt.MapFrom(src => src.Topics.Any()))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) // ADDED
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => src.Standards))
            .ForMember(dest => dest.NodeType, opt => opt.MapFrom(src => "Course"))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

        CreateMap<CourseCreateResource, Course>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => VisibilityConverter.ConvertStringToEnum(src.Visibility)))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Set in controller
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Archived, opt => opt.Ignore());

        CreateMap<CourseUpdateResource, Course>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => VisibilityConverter.ConvertStringToEnum(src.Visibility)))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived));


        // =============================================================================
        // TOPIC MAPPINGS
        // =============================================================================
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
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) // Already present
            .ForMember(dest => dest.NodeType, opt => opt.MapFrom(src => "Topic"))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

        CreateMap<TopicCreateResource, Topic>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => VisibilityConverter.ConvertStringToEnum(src.Visibility)))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // Set in controller

        CreateMap<TopicUpdateResource, Topic>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => VisibilityConverter.ConvertStringToEnum(src.Visibility)))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder));

        // =============================================================================
        // SUBTOPIC MAPPINGS
        // =============================================================================
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
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) // ADDED
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.NodeType, opt => opt.MapFrom(src => "SubTopic"))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

        // =============================================================================
        // LESSON MAPPINGS
        // =============================================================================
        CreateMap<Lesson, LessonResource>()
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => $"lesson_{src.Id}"))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.SubTopic != null ? src.SubTopic.Topic.CourseId : src.Topic.CourseId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Objective, opt => opt.MapFrom(src => src.Objective))
            .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) 
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.NodeType, opt => opt.MapFrom(src => "Lesson")); 

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
            .ForMember(dest => dest.NodeType, opt => opt.MapFrom(src => "Lesson"))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

        CreateMap<LessonCreateResource, Lesson>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => VisibilityConverter.ConvertStringToEnum(src.Visibility)))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level))
            .ForMember(dest => dest.Objective, opt => opt.MapFrom(src => src.Objective))
            .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.Materials))
            .ForMember(dest => dest.ClassTime, opt => opt.MapFrom(src => src.ClassTime))
            .ForMember(dest => dest.Methods, opt => opt.MapFrom(src => src.Methods))
            .ForMember(dest => dest.SpecialNeeds, opt => opt.MapFrom(src => src.SpecialNeeds))
            .ForMember(dest => dest.Assessment, opt => opt.MapFrom(src => src.Assessment))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // Set in controller

        CreateMap<LessonUpdateResource, Lesson>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level))
            .ForMember(dest => dest.Objective, opt => opt.MapFrom(src => src.Objective))
            .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.Materials))
            .ForMember(dest => dest.ClassTime, opt => opt.MapFrom(src => src.ClassTime))
            .ForMember(dest => dest.Methods, opt => opt.MapFrom(src => src.Methods))
            .ForMember(dest => dest.SpecialNeeds, opt => opt.MapFrom(src => src.SpecialNeeds))
            .ForMember(dest => dest.Assessment, opt => opt.MapFrom(src => src.Assessment))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => VisibilityConverter.ConvertStringToEnum(src.Visibility)))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder));

        // =============================================================================
        // USER MAPPINGS
        // =============================================================================
        CreateMap<User, UserResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.DistrictId)) // ADDED
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => // ADDED - computed in mapping
                !string.IsNullOrEmpty(src.LastName)
                    ? $"{src.LastName}, {src.FirstName}"
                    : src.FirstName ?? src.UserName))
            .ForMember(dest => dest.Password, opt => opt.Ignore()); // Never map password out

        // =============================================================================
        // SCHEDULE MAPPINGS (NEW)
        // =============================================================================
        CreateMap<Schedule, ScheduleResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.NumSchoolDays, opt => opt.MapFrom(src => src.NumSchoolDays))
            .ForMember(dest => dest.ScheduleDays, opt => opt.MapFrom(src => src.ScheduleDays))
            .ForMember(dest => dest.TeachingDays, opt => opt.Ignore()); // Handle in controller if needed

        CreateMap<ScheduleDay, ScheduleDayResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src => src.ScheduleId))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId))
            .ForMember(dest => dest.SpecialCode, opt => opt.MapFrom(src => src.SpecialCode))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment));

        CreateMap<ScheduleCreateResource, Schedule>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.NumSchoolDays, opt => opt.MapFrom(src => src.NumSchoolDays))
            .ForMember(dest => dest.ScheduleDays, opt => opt.Ignore()) // Handle in service
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // Set in controller

        CreateMap<ScheduleUpdateResource, Schedule>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.NumSchoolDays, opt => opt.MapFrom(src => src.NumSchoolDays));

        // =============================================================================
        // ATTACHMENT AND STANDARD MAPPINGS
        // =============================================================================
        CreateMap<Attachment, AttachmentResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
            .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.ContentType))
            .ReverseMap();

        CreateMap<Standard, StandardResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TopicTitle, opt => opt.MapFrom(src => src.Topic != null ? src.Topic.Title : string.Empty))
            .ForMember(dest => dest.StandardType, opt => opt.MapFrom(src => src.StandardType));

        CreateMap<StandardCreateResource, Standard>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.StandardType, opt => opt.MapFrom(src => src.StandardType));

        CreateMap<StandardUpdateResource, Standard>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.StandardType, opt => opt.MapFrom(src => src.StandardType));

        // =============================================================================
        // REVERSE MAPPINGS
        // =============================================================================
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

// =============================================================================
// VISIBILITY CONVERSION HELPER
// =============================================================================
public static class VisibilityConverter
{
    public static VisibilityType ConvertStringToEnum(string visibility)
    {
        return visibility?.ToLower() switch
        {
            "private" => VisibilityType.Private,
            "team" => VisibilityType.Team,
            "public" => VisibilityType.Public,
            _ => VisibilityType.Private // Default fallback
        };
    }
}