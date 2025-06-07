using System.ComponentModel.DataAnnotations;

namespace LessonTree.DAL.Domain
{
    public class Schedule
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Title { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsLocked { get; set; } = false;
        public string TeachingDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";

        public List<ScheduleEvent> ScheduleEvents { get; set; } = new List<ScheduleEvent>(); // NEW
    }
}