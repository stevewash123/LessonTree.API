using System.ComponentModel.DataAnnotations;

namespace LessonTree.DAL.Domain
{
    public class Schedule
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Title { get; set; } // e.g., "Math 101 - 2024"
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int UserId { get; set; } // The user who created the schedule
        public virtual User User { get; set; }
        public DateTime StartDate { get; set; } // School year start date
        public int NumSchoolDays { get; set; } // Number of school days
        public List<ScheduleDay> ScheduleDays { get; set; } = new List<ScheduleDay>();

    }
}