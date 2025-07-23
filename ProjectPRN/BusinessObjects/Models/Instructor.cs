namespace BusinessObjects.Models;

public partial class Instructor
{
    public int InstructorId { get; set; }

    public string InstructorName { get; set; } = null!;

    public int Experience { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string Password { get; set; } = null!;

    public DateTime? LastLogin { get; set; }

    public virtual ICollection<LifeSkillCourse> LifeSkillCourses { get; set; } = new List<LifeSkillCourse>();
}
