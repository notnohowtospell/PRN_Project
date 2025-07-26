using Microsoft.EntityFrameworkCore;

namespace BusinessObjects.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Assessment> Assessments { get; set; }

    public virtual DbSet<AssessmentResult> AssessmentResults { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<CourseMaterial> CourseMaterials { get; set; }

    public virtual DbSet<CourseSchedule> CourseSchedules { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<LifeSkillCourse> LifeSkillCourses { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server =(local); database = PRN212SkillsHoannn6;uid=sa;pwd=123;TrustServerCertificate=True;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId);
        });

        modelBuilder.Entity<Assessment>(entity =>
        {
            entity.Property(e => e.AssessmentType).HasMaxLength(50);
            entity.Property(e => e.InstructionFilePath).HasMaxLength(255);

            entity.HasOne(d => d.Course).WithMany(p => p.Assessments).HasForeignKey(d => d.CourseId);
        });

        modelBuilder.Entity<AssessmentResult>(entity =>
        {
            entity.HasKey(e => e.ResultId);

            entity.Property(e => e.Score).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.SubmissionFilePath).HasMaxLength(255);

            entity.HasOne(d => d.Assessment).WithMany(p => p.AssessmentResults).HasForeignKey(d => d.AssessmentId);

            entity.HasOne(d => d.Student).WithMany(p => p.AssessmentResults).HasForeignKey(d => d.StudentId);
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasIndex(e => e.CertificateCode, "UK_Certificates_CertificateCode").IsUnique();

            entity.Property(e => e.CertificateCode).HasMaxLength(50);

            entity.HasOne(d => d.Course).WithMany(p => p.Certificates).HasForeignKey(d => d.CourseId);

            entity.HasOne(d => d.Student).WithMany(p => p.Certificates).HasForeignKey(d => d.StudentId);
        });

        modelBuilder.Entity<CourseMaterial>(entity =>
        {
            entity.HasKey(e => e.MaterialId);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseMaterials).HasForeignKey(d => d.CourseId);
        });

        modelBuilder.Entity<CourseSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId);

            entity.Property(e => e.Room).HasMaxLength(50);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSchedules).HasForeignKey(d => d.CourseId);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments).HasForeignKey(d => d.CourseId);

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments).HasForeignKey(d => d.StudentId);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasOne(d => d.Course).WithMany(p => p.Feedbacks).HasForeignKey(d => d.CourseId);

            entity.HasOne(d => d.Student).WithMany(p => p.Feedbacks).HasForeignKey(d => d.StudentId);
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<LifeSkillCourse>(entity =>
        {
            entity.HasKey(e => e.CourseId);

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Instructor).WithMany(p => p.LifeSkillCourses).HasForeignKey(d => d.InstructorId);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(d => d.Student).WithMany(p => p.Notifications).HasForeignKey(d => d.StudentId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Course).WithMany(p => p.Payments).HasForeignKey(d => d.CourseId);

            entity.HasOne(d => d.Student).WithMany(p => p.Payments).HasForeignKey(d => d.StudentId);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
