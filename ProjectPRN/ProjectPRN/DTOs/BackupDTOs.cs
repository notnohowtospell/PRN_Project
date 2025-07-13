using System.Text.Json.Serialization;

namespace ProjectPRN.DTOs
{
    public class BackupData
    {
        public DateTime BackupDate { get; set; }
        public string Version { get; set; } = "1.0";
        public List<StudentBackupDto> Students { get; set; } = new();
        public List<InstructorBackupDto> Instructors { get; set; } = new();
        public List<CourseBackupDto> Courses { get; set; } = new();
        public List<EnrollmentBackupDto> Enrollments { get; set; } = new();
        // Add more entities as needed
    }

    public class StudentBackupDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        // Note: DO NOT include Password in backup for security
        public string? Email { get; set; }
        public string? Status { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarPath { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class InstructorBackupDto
    {
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = null!;
        public int Experience { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        // Note: DO NOT include Password in backup for security
        public DateTime? LastLogin { get; set; }
    }

    public class CourseBackupDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int Duration { get; set; }
        public string? Status { get; set; }
        public int? InstructorId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class EnrollmentBackupDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Grade { get; set; }
    }
}