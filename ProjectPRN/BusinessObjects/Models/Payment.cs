using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual LifeSkillCourse Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
