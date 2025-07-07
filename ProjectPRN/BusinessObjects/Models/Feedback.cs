using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime FeedbackDate { get; set; }

    public virtual LifeSkillCourse Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
