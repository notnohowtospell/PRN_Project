using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class AssessmentResult
{
    public int ResultId { get; set; }

    public int AssessmentId { get; set; }

    public int StudentId { get; set; }

    public decimal? Score { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public virtual Assessment Assessment { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
