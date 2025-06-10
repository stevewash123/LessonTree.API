// RESPONSIBILITY: AutoMapper configuration for domain entities to DTOs
// DOES NOT: Handle business logic or validation
// CALLED BY: Controllers when mapping between domain and resource models

// File: MappingProfile.cs
using AutoMapper;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

        // **PARTIAL FILE** - Updated User and UserConfiguration mappings for JWT strategy
        // RESPONSIBILITY: AutoMapper configuration aligned with clean JWT DTOs
        // DOES NOT: Map removed properties (FullName, Password, Id/UserId in config)
        // CALLED BY: Controllers when mapping between domain and resource models

        // =============================================================================
        // UPDATED USER MAPPINGS (JWT Strategy)
        // =============================================================================
        CreateMap<User, UserResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.DistrictId))
            .ForMember(dest => dest.Configuration, opt => opt.MapFrom(src => src.Configuration))
            .ForMember(dest => dest.Password, opt => opt.Ignore()); // REMOVED: Security - never map password to DTO

        CreateMap<UserCreateResource, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.DistrictId, opt => opt.MapFrom(src => src.District))
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // =============================================================================
        // UPDATED USER CONFIGURATION MAPPINGS (Clean JWT Strategy)
        // =============================================================================
        CreateMap<UserConfiguration, UserConfigurationResource>()
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated))
            .ForMember(dest => dest.SchoolYear, opt => opt.MapFrom(src => src.SchoolYear))
            .ForMember(dest => dest.PeriodsPerDay, opt => opt.MapFrom(src => src.PeriodsPerDay))
            .ForMember(dest => dest.PeriodAssignments, opt => opt.MapFrom(src => src.PeriodAssignments));
        // REMOVED: Id, UserId mappings - clean 1:1 relationship in JWT strategy

        CreateMap<UserConfigurationResource, UserConfiguration>()
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated))
            .ForMember(dest => dest.SchoolYear, opt => opt.MapFrom(src => src.SchoolYear))
            .ForMember(dest => dest.PeriodsPerDay, opt => opt.MapFrom(src => src.PeriodsPerDay))
            .ForMember(dest => dest.PeriodAssignments, opt => opt.MapFrom(src => src.PeriodAssignments))
            .ForMember(dest => dest.Id, opt => opt.Ignore())       // Set by repository
            .ForMember(dest => dest.UserId, opt => opt.Ignore())   // Set by repository
            .ForMember(dest => dest.SettingsJson, opt => opt.Ignore()); // REMOVED: Using structured properties

        // =============================================================================
        // UPDATED PERIOD ASSIGNMENT MAPPINGS (Cleaned up duplicates)
        // =============================================================================
        // **UPDATED PERIOD ASSIGNMENT MAPPINGS** - Remove SectionName, Add SpecialPeriodType
        // Replace the existing PeriodAssignment mappings in MappingProfile.cs with these:

        // UPDATED PERIOD ASSIGNMENT MAPPINGS (Standardized string[] TeachingDays)
        // =============================================================================
        CreateMap<PeriodAssignment, PeriodAssignmentResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.SpecialPeriodType, opt => opt.MapFrom(src =>
                src.SpecialPeriodType.HasValue ? src.SpecialPeriodType.Value.ToString() : null))
            .ForMember(dest => dest.TeachingDays, opt => opt.MapFrom<PeriodAssignmentTeachingDaysToArrayResolver>())
            .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.BackgroundColor, opt => opt.MapFrom(src => src.BackgroundColor))
            .ForMember(dest => dest.FontColor, opt => opt.MapFrom(src => src.FontColor));

        CreateMap<PeriodAssignmentResource, PeriodAssignment>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.SpecialPeriodType, opt => opt.MapFrom<SpecialPeriodTypeResolver>())
            .ForMember(dest => dest.TeachingDays, opt => opt.MapFrom<PeriodAssignmentTeachingDaysToStringResolver>())
            .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.BackgroundColor, opt => opt.MapFrom(src => src.BackgroundColor))
            .ForMember(dest => dest.FontColor, opt => opt.MapFrom(src => src.FontColor))
            .ForMember(dest => dest.UserConfigurationId, opt => opt.Ignore()); // Set by repository


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
        // UPDATED SCHEDULE MAPPINGS (Standardized string[] TeachingDays)
        // =============================================================================
        CreateMap<Schedule, ScheduleResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLocked))
            .ForMember(dest => dest.TeachingDays, opt => opt.MapFrom<ScheduleTeachingDaysToArrayResolver>())
            .ForMember(dest => dest.ScheduleEvents, opt => opt.MapFrom(src => src.ScheduleEvents));

        CreateMap<ScheduleCreateResource, Schedule>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.TeachingDays, opt => opt.MapFrom<ScheduleCreateTeachingDaysToStringResolver>())
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Set in controller
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsLocked, opt => opt.Ignore());

        CreateMap<ScheduleConfigUpdateResource, Schedule>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.TeachingDays, opt => opt.MapFrom<ScheduleConfigTeachingDaysToStringResolver>())
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLocked))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // Don't update UserId

        // =============================================================================
        // NEW SCHEDULE EVENT MAPPINGS
        // =============================================================================
        CreateMap<ScheduleEvent, ScheduleEventResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src => src.ScheduleId))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId))
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType))
            .ForMember(dest => dest.EventCategory, opt => opt.MapFrom(src => src.EventCategory))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment));

        CreateMap<ScheduleEventCreateResource, ScheduleEvent>()
            .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src => src.ScheduleId))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId))
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType))
            .ForMember(dest => dest.EventCategory, opt => opt.MapFrom(src => src.EventCategory))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Set by repository

        CreateMap<ScheduleEventUpdateResource, ScheduleEvent>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId))
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType))
            .ForMember(dest => dest.EventCategory, opt => opt.MapFrom(src => src.EventCategory))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
            .ForMember(dest => dest.ScheduleId, opt => opt.Ignore()); // Don't update ScheduleId


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





// =============================================================================
// UTILITY MAPPING HELPERS
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



// =============================================================================
// CORRECTED RESOLVERS FOR STANDARDIZED string[] TEACHING DAYS
// =============================================================================

// Convert comma-delimited string (Schedule domain) to string[] (ScheduleResource DTO)
public class ScheduleTeachingDaysToArrayResolver : IValueResolver<Schedule, ScheduleResource, string[]>
{
    public string[] Resolve(Schedule source, ScheduleResource destination, string[] destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.TeachingDays))
            return new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }; // Default

        return source.TeachingDays.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(day => day.Trim())
                                  .Where(day => !string.IsNullOrEmpty(day))
                                  .ToArray();
    }
}

// Convert string[] (ScheduleCreateResource DTO) to comma-delimited string (Schedule domain)
public class ScheduleCreateTeachingDaysToStringResolver : IValueResolver<ScheduleCreateResource, Schedule, string>
{
    public string Resolve(ScheduleCreateResource source, Schedule destination, string destMember, ResolutionContext context)
    {
        if (source.TeachingDays != null && source.TeachingDays.Length > 0)
        {
            return string.Join(",", source.TeachingDays.Where(day => !string.IsNullOrEmpty(day)));
        }

        return "Monday,Tuesday,Wednesday,Thursday,Friday"; // Default
    }
}

// Convert string[] (ScheduleConfigUpdateResource DTO) to comma-delimited string (Schedule domain)
public class ScheduleConfigTeachingDaysToStringResolver : IValueResolver<ScheduleConfigUpdateResource, Schedule, string>
{
    public string Resolve(ScheduleConfigUpdateResource source, Schedule destination, string destMember, ResolutionContext context)
    {
        if (source.TeachingDays != null && source.TeachingDays.Length > 0)
        {
            return string.Join(",", source.TeachingDays.Where(day => !string.IsNullOrEmpty(day)));
        }

        return "Monday,Tuesday,Wednesday,Thursday,Friday"; // Default
    }
}

// Convert comma-delimited string (PeriodAssignment domain) to string[] (PeriodAssignmentResource DTO)
public class PeriodAssignmentTeachingDaysToArrayResolver : IValueResolver<PeriodAssignment, PeriodAssignmentResource, string[]>
{
    public string[] Resolve(PeriodAssignment source, PeriodAssignmentResource destination, string[] destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.TeachingDays))
            return new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }; // Default

        return source.TeachingDays.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(day => day.Trim())
                                  .Where(day => !string.IsNullOrEmpty(day))
                                  .ToArray();
    }
}

// Convert string[] (PeriodAssignmentResource DTO) to comma-delimited string (PeriodAssignment domain)
public class PeriodAssignmentTeachingDaysToStringResolver : IValueResolver<PeriodAssignmentResource, PeriodAssignment, string>
{
    public string Resolve(PeriodAssignmentResource source, PeriodAssignment destination, string destMember, ResolutionContext context)
    {
        if (source.TeachingDays != null && source.TeachingDays.Length > 0)
        {
            return string.Join(",", source.TeachingDays.Where(day => !string.IsNullOrEmpty(day)));
        }

        return "Monday,Tuesday,Wednesday,Thursday,Friday"; // Default
    }
}

// =============================================================================
// RESOLVER FOR SPECIAL PERIOD TYPE CONVERSION (No changes needed)
// =============================================================================
public class SpecialPeriodTypeResolver : IValueResolver<PeriodAssignmentResource, PeriodAssignment, SpecialPeriodType?>
{
    public SpecialPeriodType? Resolve(PeriodAssignmentResource source, PeriodAssignment destination, SpecialPeriodType? destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.SpecialPeriodType))
            return null;

        if (Enum.TryParse<SpecialPeriodType>(source.SpecialPeriodType, true, out var enumValue))
            return enumValue;

        return null;
    }
}
