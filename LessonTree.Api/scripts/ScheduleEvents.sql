select l.id, s. ScheduleSort
from ScheduleEvents s
inner join Lessons l on l.id = s.LessonId
where period = 1