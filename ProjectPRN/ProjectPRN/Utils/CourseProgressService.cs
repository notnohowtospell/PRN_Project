using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectPRN.Utils
{
    public class CourseProgressInfo
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int TotalAssessments { get; set; }
        public int CompletedAssessments { get; set; }
        public double ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }
        public List<AssessmentProgressInfo> AssessmentProgress { get; set; } = new List<AssessmentProgressInfo>();
    }

    public class AssessmentProgressInfo
    {
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public decimal? Score { get; set; }
        public int MaxScore { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public double ContributionPercentage { get; set; }
    }

    public static class CourseProgressService
    {
        public static async Task<double> CalculateOverallProgressAsync(int studentId)
        {
            try
            {
                // Tạo DbContext mới cho mỗi operation
                using var context = new ApplicationDbContext();
                var allProgress = await CalculateAllProgressAsync(studentId);

                if (!allProgress.Any())
                    return 0;

                var averageProgress = allProgress.Average(p => p.ProgressPercentage);
                return Math.Round(averageProgress, 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CalculateOverallProgressAsync: {ex.Message}");
                return 0;
            }
        }

        public static async Task<List<CourseProgressInfo>> CalculateAllProgressAsync(int studentId)
        {
            System.Diagnostics.Debug.WriteLine($"Calculating progress for student: {studentId}");

            var progressList = new List<CourseProgressInfo>();

            try
            {
                // Tạo DbContext mới và đảm bảo dispose đúng cách
                using var context = new ApplicationDbContext();

                // Kiểm tra connection trước khi query
                await context.Database.OpenConnectionAsync();

                var enrollments = await context.Enrollments
                    .Where(e => e.StudentId == studentId)
                    .Select(e => new
                    {
                        e.CourseId,
                        e.StudentId,
                        e.CompletionStatus,
                        e.CompletionDate,
                        CourseName = e.Course != null ? e.Course.CourseName : "N/A",
                        InstructorName = e.Course != null && e.Course.Instructor != null
                            ? e.Course.Instructor.InstructorName : "N/A"
                    })
                    .ToListAsync();

                foreach (var enrollment in enrollments)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Processing course: {enrollment.CourseName}");
                        var progress = await CalculateProgressAsync(enrollment.StudentId, enrollment.CourseId);
                        progressList.Add(progress);
                    }
                    catch (Exception innerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing enrollment for CourseId {enrollment.CourseId}: {innerEx.Message}");
                        // Continue with next enrollment instead of failing completely
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CalculateAllProgressAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            return progressList.OrderByDescending(p => p.ProgressPercentage).ToList();
        }

        public static async Task<CourseProgressInfo> CalculateProgressAsync(int studentId, int courseId)
        {
            System.Diagnostics.Debug.WriteLine($"Calculate Progress Async for Student: {studentId}, Course: {courseId}");

            try
            {
                // Tạo DbContext mới cho method này
                using var context = new ApplicationDbContext();

                var enrollment = await context.Enrollments
                    .Where(e => e.StudentId == studentId && e.CourseId == courseId)
                    .Select(e => new
                    {
                        e.CourseId,
                        e.CompletionStatus,
                        e.CompletionDate,
                        CourseName = e.Course != null ? e.Course.CourseName : "N/A",
                        InstructorName = e.Course != null && e.Course.Instructor != null
                            ? e.Course.Instructor.InstructorName : "N/A"
                    })
                    .FirstOrDefaultAsync();

                if (enrollment == null)
                {
                    throw new ArgumentException("Student not enrolled in this course or course data is missing.");
                }

                var assessments = await context.Assessments
                    .Where(a => a.CourseId == courseId)
                    .ToListAsync();

                var assessmentIds = assessments.Select(a => a.AssessmentId).ToList();
                var studentResults = await context.AssessmentResults
                    .Where(ar => ar.StudentId == studentId && assessmentIds.Contains(ar.AssessmentId))
                    .ToListAsync();

                var totalAssessments = assessments.Count;
                var completedAssessments = studentResults.Count;
                var progressPercentage = totalAssessments > 0 ? (completedAssessments * 100.0 / totalAssessments) : 0;
                if (progressPercentage < 100 && enrollment.CompletionDate != null)
                {
                    var completedEnrollment = await context.Enrollments
                                        .Where(e => e.StudentId == studentId && e.CourseId == courseId).FirstOrDefaultAsync();
                    completedEnrollment.CompletionDate = null;
                }
                if (progressPercentage == 100)
                {
                    var completedEnrollment = await context.Enrollments
                                        .Where(e => e.StudentId == studentId && e.CourseId == courseId).FirstOrDefaultAsync();
                    if (completedEnrollment.CompletionDate == null)
                    {
                        var certificate = new Certificate();
                        if (completedEnrollment != null)
                        {
                            completedEnrollment.CompletionDate = DateTime.Now;
                            //certificate.StudentId = studentId;
                            //certificate.CourseId = courseId;
                            //certificate.IssueDate = DateTime.Now;
                            //certificate.CertificateCode = "CERT-" + (new Random().Next(10000, 99999).ToString());
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine($"Progress: {progressPercentage}%");

                return new CourseProgressInfo
                {
                    CourseId = courseId,
                    CourseName = enrollment.CourseName,
                    InstructorName = enrollment.InstructorName,
                    TotalAssessments = totalAssessments,
                    CompletedAssessments = completedAssessments,
                    ProgressPercentage = Math.Round(progressPercentage, 1),
                    IsCompleted = enrollment.CompletionStatus,
                    CompletionDate = enrollment.CompletionDate,
                    AssessmentProgress = new List<AssessmentProgressInfo>()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CalculateProgressAsync for CourseId {courseId}: {ex.Message}");

                // Return a default progress info if something fails
                return new CourseProgressInfo
                {
                    CourseId = courseId,
                    CourseName = "Error Loading Course",
                    InstructorName = "N/A",
                    TotalAssessments = 0,
                    CompletedAssessments = 0,
                    ProgressPercentage = 0,
                    IsCompleted = false,
                    CompletionDate = null,
                    AssessmentProgress = new List<AssessmentProgressInfo>()
                };
            }
        }
    }
}