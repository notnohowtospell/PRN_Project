using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Assessment
{
    public int AssessmentId { get; set; }

    public int CourseId { get; set; }

    public string AssessmentName { get; set; } = null!;

    public int MaxScore { get; set; }

    public DateTime DueDate { get; set; }

    public string? AssessmentType { get; set; }

    public string? Instructions { get; set; }

    public virtual ICollection<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();

    public virtual LifeSkillCourse Course { get; set; } = null!;
}
