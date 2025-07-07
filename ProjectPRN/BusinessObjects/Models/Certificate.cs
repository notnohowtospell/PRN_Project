using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Certificate
{
    public int CertificateId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public DateTime IssueDate { get; set; }

    public string CertificateCode { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public virtual LifeSkillCourse Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
