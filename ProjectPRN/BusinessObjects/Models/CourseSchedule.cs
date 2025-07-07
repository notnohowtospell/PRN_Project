using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class CourseSchedule
{
    public int ScheduleId { get; set; }

    public int CourseId { get; set; }

    public DateTime SessionDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Room { get; set; } = null!;

    public virtual LifeSkillCourse Course { get; set; } = null!;
}
