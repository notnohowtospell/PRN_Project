using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public bool CompletionStatus { get; set; }

    public DateTime? CompletionDate { get; set; }

    public virtual LifeSkillCourse Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
