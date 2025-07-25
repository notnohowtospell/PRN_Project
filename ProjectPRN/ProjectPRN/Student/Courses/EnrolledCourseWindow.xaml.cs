using System.Windows;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectPRN.Student.Courses
{
    /// <summary>
    /// Interaction logic for RegisteredCoursesWindow.xaml
    /// </summary>
    public partial class EnrolledCoursesWindow : Window
    {
        private readonly ApplicationDbContext _context;
        private readonly StudentViewModel CurrentStudent;

        public EnrolledCoursesWindow(StudentViewModel currentStudent)
        {
            InitializeComponent();
            _context = new ApplicationDbContext();
            CurrentStudent = currentStudent;
            LoadRegisteredCourses();
        }

        private void LoadRegisteredCourses()
        {
            // Get only registered courses that are NOT paid yet (Pending status)
            var registeredCourses = _context.Payments
                .Include(p => p.Course)
                    .ThenInclude(c => c.Instructor)
                .Where(p => p.StudentId == CurrentStudent.StudentId &&
                           p.Status != "Đã thanh toán") // Only show unpaid registrations
                .Select(p => new EnrolledCourseViewModel
                {
                    CourseId = p.CourseId,
                    CourseName = p.Course.CourseName ?? "N/A",
                    InstructorName = p.Course.Instructor != null ? p.Course.Instructor.InstructorName : "N/A",
                    StartDate = p.Course.StartDate,
                    EndDate = p.Course.EndDate,
                    Price = p.Course.Price ?? 0,
                    Status = p.Course.Status ?? "N/A",
                    PaymentStatus = p.Status ?? "N/A"
                })
                .ToList();

            if (registeredCourses.Count == 0)
            {
                dgEnrolledCourses.Visibility = Visibility.Collapsed;
                txtNoCourses.Visibility = Visibility.Visible;
            }
            else
            {
                dgEnrolledCourses.ItemsSource = registeredCourses;
                dgEnrolledCourses.Visibility = Visibility.Visible;
                txtNoCourses.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }

    public class EnrolledCourseViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        // Helper properties for UI
        public string PaymentStatusDisplay => PaymentStatus switch
        {
            "Pending" => "Chờ xác nhận",
            _ => PaymentStatus
        };

        public bool NeedsPayment => PaymentStatus != "Đã thanh toán";
    }
}