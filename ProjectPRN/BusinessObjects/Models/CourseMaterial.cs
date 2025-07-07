using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class CourseMaterial
{
    public int MaterialId { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime UploadDate { get; set; }

    public virtual LifeSkillCourse Course { get; set; } = null!;
}
