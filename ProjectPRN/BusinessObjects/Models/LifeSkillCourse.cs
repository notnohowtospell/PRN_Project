using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class LifeSkillCourse
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public int InstructorId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Description { get; set; }

    public int? MaxStudents { get; set; }

    public decimal? Price { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<CourseMaterial> CourseMaterials { get; set; } = new List<CourseMaterial>();

    public virtual ICollection<CourseSchedule> CourseSchedules { get; set; } = new List<CourseSchedule>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual Instructor Instructor { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
