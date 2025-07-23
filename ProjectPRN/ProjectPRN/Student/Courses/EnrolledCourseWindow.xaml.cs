using System.Windows;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectPRN.Student.Courses
{
    /// <summary>
    /// Interaction logic for EnrolledCourseWindow.xaml
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
            LoadEnrolledCourses();
        }

        private void LoadEnrolledCourses()
        {
            var enrolledCourses = _context.Enrollments
                .Include(p => p.Course)
                    .ThenInclude(c => c.Instructor)
                .Where(p => p.StudentId == CurrentStudent.StudentId)
                .Select(p => new EnrolledCourseViewModel
                {
                    CourseId = p.CourseId,
                    CourseName = p.Course.CourseName,
                    InstructorName = p.Course.Instructor != null ? p.Course.Instructor.InstructorName : "N/A",
                    StartDate = p.Course.StartDate,
                    EndDate = p.Course.EndDate,
                    Price = p.Course.Price ?? 0,
                    Status = p.Course.Status,
                })
                .ToList();

            if (enrolledCourses.Count == 0)
            {
                dgEnrolledCourses.Visibility = Visibility.Collapsed;
                txtNoCourses.Visibility = Visibility.Visible;
            }
            else
            {
                dgEnrolledCourses.ItemsSource = enrolledCourses;
                dgEnrolledCourses.Visibility = Visibility.Visible;
                txtNoCourses.Visibility = Visibility.Collapsed;
            }
        }
    }

    public class EnrolledCourseViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
    }


}
