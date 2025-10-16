// **CLEANED** - MappingProfile.cs aligned with ScheduleConfiguration architecture
// RESPONSIBILITY: AutoMapper configuration for domain entities to DTOs
// DOES NOT: Map old UserConfiguration period assignments (removed)
// CALLED BY: Controllers when mapping between domain and resource models

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
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Topics, opt => opt.MapFrom(src => src.Topics))
            .ForMember(dest => dest.HasChildren, opt => opt.MapFrom(src => src.Topics.Any()))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => src.Standards))
            .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => "Course"))
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
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.SubTopics, opt => opt.MapFrom(src => src.SubTopics))
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => "Topic"))
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
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Topic.CourseId))
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons ?? new List<Lesson>()))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => "SubTopic"))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));


        CreateMap<SubTopicCreateResource, SubTopic>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => VisibilityConverter.ConvertStringToEnum(src.Visibility)))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Set in controller
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Archived, opt => opt.Ignore());

        CreateMap<SubTopicUpdateResource, SubTopic>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => VisibilityConverter.ConvertStringToEnum(src.Visibility)))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder));


        // =============================================================================
        // LESSON MAPPINGS
        // =============================================================================
        CreateMap<Lesson, LessonResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.SubTopic != null && src.SubTopic.Topic != null ? src.SubTopic.Topic.CourseId : src.Topic != null ? src.Topic.CourseId : (int?)null))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Objective, opt => opt.MapFrom(src => src.Objective))
            .ForMember(dest => dest.SubTopicId, opt => opt.MapFrom(src => src.SubTopicId))
            .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.Archived, opt => opt.MapFrom(src => src.Archived))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility))
            .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => "Lesson"));

        CreateMap<Lesson, LessonDetailResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.SubTopic != null && src.SubTopic.Topic != null ? src.SubTopic.Topic.CourseId : src.Topic != null ? src.Topic.CourseId : (int?)null))
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
            .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => "Lesson"))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
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
        // USER MAPPINGS (SIMPLIFIED - NO PERIOD ASSIGNMENTS)
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
            .ForMember(dest => dest.Password, opt => opt.Ignore()); // Security - never map password

        CreateMap<UserCreateResource, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.DistrictId, opt => opt.MapFrom(src => src.District))
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // UserConfiguration mappings (simplified - no period assignments)
        CreateMap<UserConfiguration, UserConfigurationResource>()
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated));

        CreateMap<UserConfigurationResource, UserConfiguration>()
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated))
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Set by repository
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Set by repository
            .ForMember(dest => dest.SettingsJson, opt => opt.Ignore());

        // =============================================================================
        // NEW: SCHEDULE CONFIGURATION MAPPINGS
        // =============================================================================
        CreateMap<ScheduleConfiguration, ScheduleConfigurationResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.SchoolYear, opt => opt.MapFrom(src => src.SchoolYear))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.PeriodsPerDay, opt => opt.MapFrom(src => src.PeriodsPerDay))
            .ForMember(dest => dest.TeachingDays, opt => opt.MapFrom<ScheduleConfigurationTeachingDaysToArrayResolver>())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status == ScheduleStatus.Active))
            .ForMember(dest => dest.PeriodAssignments, opt => opt.MapFrom(src => src.PeriodAssignments));
        // REMOVED: CreatedDate, LastUpdated (audit properties)

        CreateMap<ScheduleConfigurationCreateResource, ScheduleConfiguration>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.PeriodsPerDay, opt => opt.MapFrom(src => src.PeriodsPerDay))
            .ForMember(dest => dest.TeachingDays, opt => opt.MapFrom<ScheduleConfigurationCreateTeachingDaysToStringResolver>())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ScheduleStatus.Active))
            .ForMember(dest => dest.PeriodAssignments, opt => opt.MapFrom(src => src.PeriodAssignments))
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Set by repository
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Set in controller
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Set by repository
            .ForMember(dest => dest.LastUpdated, opt => opt.Ignore()); // Set by repository

        CreateMap<ScheduleConfigurationUpdateResource, ScheduleConfiguration>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.PeriodsPerDay, opt => opt.MapFrom(src => src.PeriodsPerDay))
            .ForMember(dest => dest.TeachingDays, opt => opt.MapFrom<ScheduleConfigurationUpdateTeachingDaysToStringResolver>())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? ScheduleStatus.Active : ScheduleStatus.Archived))
            .ForMember(dest => dest.PeriodAssignments, opt => opt.MapFrom(src => src.PeriodAssignments))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Don't update UserId
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Don't update CreatedDate
            .ForMember(dest => dest.LastUpdated, opt => opt.Ignore()); // Set by repository

        // =============================================================================
        // PERIOD ASSIGNMENT MAPPINGS (BELONGS TO SCHEDULE CONFIGURATION)
        // =============================================================================
        CreateMap<PeriodAssignment, PeriodAssignmentResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.SpecialPeriodType, opt => opt.MapFrom(src => src.SpecialPeriodType))
            .ForMember(dest => dest.TeachingDays, opt => opt.MapFrom<PeriodAssignmentTeachingDaysToArrayResolver>())
            .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.BackgroundColor, opt => opt.MapFrom(src => src.BackgroundColor))
            .ForMember(dest => dest.FontColor, opt => opt.MapFrom(src => src.FontColor));

        CreateMap<PeriodAssignmentResource, PeriodAssignment>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.SpecialPeriodType, opt => opt.MapFrom(src => src.SpecialPeriodType))
            .ForMember(dest => dest.TeachingDays, opt => opt.MapFrom<PeriodAssignmentTeachingDaysToStringResolver>())
            .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.BackgroundColor, opt => opt.MapFrom(src => src.BackgroundColor))
            .ForMember(dest => dest.FontColor, opt => opt.MapFrom(src => src.FontColor))
            .ForMember(dest => dest.ScheduleConfigurationId, opt => opt.Ignore()); // Set by repository

        // =============================================================================
        // SCHEDULE MAPPINGS (SIMPLIFIED - REFERENCES CONFIGURATION)
        // =============================================================================
        CreateMap<Schedule, ScheduleResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.ScheduleConfiguration, opt => opt.MapFrom(src => src.ScheduleConfiguration))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLocked))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            //.ForMember(dest => dest.ScheduleEvents, opt => opt.MapFrom(src => src.ScheduleEvents))
            .ForMember(dest => dest.SpecialDays, opt => opt.MapFrom(src => src.SpecialDays));

        CreateMap<ScheduleCreateResource, Schedule>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ScheduleConfigurationId, opt => opt.MapFrom(src => src.ScheduleConfigurationId))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Set in controller
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsLocked, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Set by repository
            //.ForMember(dest => dest.ScheduleEvents, opt => opt.MapFrom(src => src.ScheduleEvents ?? new List<ScheduleEventResource>()))
            .ForMember(dest => dest.SpecialDays, opt => opt.MapFrom(src => src.SpecialDays ?? new List<SpecialDayResource>()));

        CreateMap<ScheduleEventResource, ScheduleEvent>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src => src.ScheduleId))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId))
            .ForMember(dest => dest.SpecialDayId, opt => opt.MapFrom(src => src.SpecialDayId))
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType))
            .ForMember(dest => dest.EventCategory, opt => opt.MapFrom(src => src.EventCategory))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
            .ForMember(dest => dest.Schedule, opt => opt.Ignore()) // Navigation property - set by repository
            .ForMember(dest => dest.Lesson, opt => opt.Ignore()) // Navigation property - set by repository
            .ForMember(dest => dest.SpecialDay, opt => opt.Ignore()); // Navigation property - set by repository

        // =============================================================================
        // SCHEDULE EVENT MAPPINGS
        // =============================================================================
        CreateMap<ScheduleEvent, ScheduleEventResource>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src => src.ScheduleId))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId))
            .ForMember(dest => dest.SpecialDayId, opt => opt.MapFrom(src => src.SpecialDayId))
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType))
            .ForMember(dest => dest.EventCategory, opt => opt.MapFrom(src => src.EventCategory))
            .ForMember(dest => dest.LessonSort, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.SortOrder : (int?)null))
            .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Title : null))
            .ForMember(dest => dest.LessonObjective, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Objective : null))
            .ForMember(dest => dest.LessonMethods, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Methods : null))
            .ForMember(dest => dest.LessonMaterials, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Materials : null))
            .ForMember(dest => dest.LessonAssessment, opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Assessment : null))
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

        // =============================================================================
        // SPECIAL DAY MAPPINGS
        // =============================================================================
        CreateMap<SpecialDay, SpecialDayResource>()
            .ForMember(dest => dest.Periods, opt => opt.MapFrom<SpecialDayPeriodsToArrayResolver>());

        CreateMap<SpecialDayCreateResource, SpecialDay>()
            .ForMember(dest => dest.Periods, opt => opt.MapFrom<SpecialDayCreatePeriodsToStringResolver>())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ScheduleId, opt => opt.Ignore())
            .ForMember(dest => dest.Schedule, opt => opt.Ignore());

        CreateMap<SpecialDayUpdateResource, SpecialDay>()
            .ForMember(dest => dest.Periods, opt => opt.MapFrom<SpecialDayUpdatePeriodsToStringResolver>())
            .ForMember(dest => dest.ScheduleId, opt => opt.Ignore())
            .ForMember(dest => dest.Schedule, opt => opt.Ignore());

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
// SCHEDULE CONFIGURATION TEACHING DAYS RESOLVERS
// =============================================================================

// Convert comma-delimited string to string[] for ScheduleConfiguration
public class ScheduleConfigurationTeachingDaysToArrayResolver : IValueResolver<ScheduleConfiguration, ScheduleConfigurationResource, string[]>
{
    public string[] Resolve(ScheduleConfiguration source, ScheduleConfigurationResource destination, string[] destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.TeachingDays))
            return new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        return source.TeachingDays.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(day => day.Trim())
                                  .Where(day => !string.IsNullOrEmpty(day))
                                  .ToArray();
    }
}

// Convert string[] to comma-delimited string for ScheduleConfiguration creation
public class ScheduleConfigurationCreateTeachingDaysToStringResolver : IValueResolver<ScheduleConfigurationCreateResource, ScheduleConfiguration, string>
{
    public string Resolve(ScheduleConfigurationCreateResource source, ScheduleConfiguration destination, string destMember, ResolutionContext context)
    {
        if (source.TeachingDays != null && source.TeachingDays.Length > 0)
        {
            return string.Join(",", source.TeachingDays.Where(day => !string.IsNullOrEmpty(day)));
        }
        return "Monday,Tuesday,Wednesday,Thursday,Friday";
    }
}

// Convert string[] to comma-delimited string for ScheduleConfiguration update
public class ScheduleConfigurationUpdateTeachingDaysToStringResolver : IValueResolver<ScheduleConfigurationUpdateResource, ScheduleConfiguration, string>
{
    public string Resolve(ScheduleConfigurationUpdateResource source, ScheduleConfiguration destination, string destMember, ResolutionContext context)
    {
        if (source.TeachingDays != null && source.TeachingDays.Length > 0)
        {
            return string.Join(",", source.TeachingDays.Where(day => !string.IsNullOrEmpty(day)));
        }
        return "Monday,Tuesday,Wednesday,Thursday,Friday";
    }
}

// =============================================================================
// PERIOD ASSIGNMENT TEACHING DAYS RESOLVERS  
// =============================================================================

// Convert comma-delimited string to string[] for PeriodAssignment
public class PeriodAssignmentTeachingDaysToArrayResolver : IValueResolver<PeriodAssignment, PeriodAssignmentResource, string[]>
{
    public string[] Resolve(PeriodAssignment source, PeriodAssignmentResource destination, string[] destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.TeachingDays))
            return new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        return source.TeachingDays.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(day => day.Trim())
                                  .Where(day => !string.IsNullOrEmpty(day))
                                  .ToArray();
    }
}

// Convert string[] to comma-delimited string for PeriodAssignment
public class PeriodAssignmentTeachingDaysToStringResolver : IValueResolver<PeriodAssignmentResource, PeriodAssignment, string>
{
    public string Resolve(PeriodAssignmentResource source, PeriodAssignment destination, string destMember, ResolutionContext context)
    {
        if (source.TeachingDays != null && source.TeachingDays.Length > 0)
        {
            return string.Join(",", source.TeachingDays.Where(day => !string.IsNullOrEmpty(day)));
        }
        return "Monday,Tuesday,Wednesday,Thursday,Friday";
    }
}

// =============================================================================
// SPECIAL DAY PERIOD RESOLVERS
// =============================================================================

// Convert JSON string to int[] for SpecialDay
public class SpecialDayPeriodsToArrayResolver : IValueResolver<SpecialDay, SpecialDayResource, int[]>
{
    public int[] Resolve(SpecialDay source, SpecialDayResource destination, int[] destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.Periods))
            return new int[0];

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<int[]>(source.Periods) ?? new int[0];
        }
        catch
        {
            return new int[0];
        }
    }
}

// Convert int[] to JSON string for SpecialDay creation
public class SpecialDayCreatePeriodsToStringResolver : IValueResolver<SpecialDayCreateResource, SpecialDay, string>
{
    public string Resolve(SpecialDayCreateResource source, SpecialDay destination, string destMember, ResolutionContext context)
    {
        if (source.Periods == null || source.Periods.Length == 0)
            return "[]";

        return System.Text.Json.JsonSerializer.Serialize(source.Periods);
    }
}

// Convert int[] to JSON string for SpecialDay update
public class SpecialDayUpdatePeriodsToStringResolver : IValueResolver<SpecialDayUpdateResource, SpecialDay, string>
{
    public string Resolve(SpecialDayUpdateResource source, SpecialDay destination, string destMember, ResolutionContext context)
    {
        if (source.Periods == null || source.Periods.Length == 0)
            return "[]";

        return System.Text.Json.JsonSerializer.Serialize(source.Periods);
    }
}