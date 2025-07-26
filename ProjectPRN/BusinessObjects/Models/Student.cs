using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string StudentName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Email { get; set; }

    public string? Status { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? AvatarPath { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual ICollection<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
