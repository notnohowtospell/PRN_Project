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
        public static async Task<double> CalculateOverallProgressAsync(ApplicationDbContext context, int studentId)
        {
            try
            {
                var allProgress = await CalculateAllProgressAsync(context, studentId);
                
                if (!allProgress.Any())
                    return 0;

                var averageProgress = allProgress.Average(p => p.ProgressPercentage);
                return Math.Round(averageProgress, 1);
            }
            catch
            {
                return 0;
            }
        }

        public static async Task<List<CourseProgressInfo>> CalculateAllProgressAsync(ApplicationDbContext context, int studentId)
        {
            var progressList = new List<CourseProgressInfo>();

            try
            {
                var enrollments = await context.Enrollments
                    .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                    .Where(e => e.StudentId == studentId)
                    .ToListAsync();

                foreach (var enrollment in enrollments)
                {
                    var progress = await CalculateProgressAsync(context, studentId, enrollment.CourseId);
                    progressList.Add(progress);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CalculateAllProgressAsync: {ex.Message}");
            }

            return progressList.OrderByDescending(p => p.ProgressPercentage).ToList();
        }

        public static async Task<CourseProgressInfo> CalculateProgressAsync(ApplicationDbContext context, int studentId, int courseId)
        {
            var enrollment = await context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment == null)
            {
                throw new ArgumentException("Student not enrolled in this course.");
            }

            var assessments = await context.Assessments
                .Where(a => a.CourseId == courseId)
                .ToListAsync();

            var studentResults = await context.AssessmentResults
                .Where(ar => ar.StudentId == studentId && assessments.Select(a => a.AssessmentId).Contains(ar.AssessmentId))
                .ToListAsync();

            var totalAssessments = assessments.Count;
            var completedAssessments = studentResults.Count;
            var progressPercentage = totalAssessments > 0 ? (completedAssessments * 100.0 / totalAssessments) : 0;

            var assessmentProgressList = new List<AssessmentProgressInfo>();
            var contributionPercentage = totalAssessments > 0 ? 100.0 / totalAssessments : 0;

            foreach (var assessment in assessments)
            {
                var result = studentResults.FirstOrDefault(sr => sr.AssessmentId == assessment.AssessmentId);
                
                assessmentProgressList.Add(new AssessmentProgressInfo
                {
                    AssessmentId = assessment.AssessmentId,
                    AssessmentName = assessment.AssessmentName,
                    AssessmentType = assessment.AssessmentType ?? "N/A",
                    DueDate = assessment.DueDate,
                    IsCompleted = result != null,
                    Score = result?.Score,
                    MaxScore = assessment.MaxScore,
                    SubmissionDate = result?.SubmissionDate,
                    ContributionPercentage = contributionPercentage
                });
            }

            return new CourseProgressInfo
            {
                CourseId = courseId,
                CourseName = enrollment.Course.CourseName ?? "N/A",
                InstructorName = enrollment.Course.Instructor?.InstructorName ?? "N/A",
                TotalAssessments = totalAssessments,
                CompletedAssessments = completedAssessments,
                ProgressPercentage = Math.Round(progressPercentage, 1),
                IsCompleted = enrollment.CompletionStatus,
                CompletionDate = enrollment.CompletionDate,
                AssessmentProgress = assessmentProgressList
            };
        }
    }
}