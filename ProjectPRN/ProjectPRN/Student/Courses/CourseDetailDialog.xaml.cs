using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace ProjectPRN.Student.Courses
{
    public partial class CourseDetailsDialog : UserControl
    {
        private readonly CourseViewModel _course;

        public CourseDetailsDialog(CourseViewModel course)
        {
            InitializeComponent();

            _course = course;

            LoadCourseDetails();
        }

        #region Initialization
        private void LoadCourseDetails()
        {
            // Course basic information
            txtCourseName.Text = _course.CourseName;
            txtInstructor.Text = _course.InstructorName;

            // Duration
            if (_course.StartDate.HasValue && _course.EndDate.HasValue)
            {
                txtDuration.Text = $"{_course.StartDate:dd/MM/yyyy} - {_course.EndDate:dd/MM/yyyy}";
            }
            else
            {
                txtDuration.Text = "Chưa xác định";
            }

            // Price
            txtPrice.Text = _course.Price?.ToString("C0", CultureInfo.GetCultureInfo("vi-VN")) ?? "Miễn phí";

            // Capacity
            var availableSlots = _course.MaxStudents - _course.CurrentEnrollments;
            txtCapacity.Text = $"{_course.CurrentEnrollments}/{_course.MaxStudents} học viên";
            txtAvailableSlots.Text = $"{availableSlots}/{_course.MaxStudents} chỗ";

            // Status
            txtStatus.Text = _course.Status;
            UpdateStatusCardColor();

            // Description
            txtDescription.Text = string.IsNullOrWhiteSpace(_course.Description)
                ? "Chưa có mô tả chi tiết cho khóa học này."
                : _course.Description;

            // Instructor details
            LoadInstructorDetails();

            // Check if course is full
            UpdateEnrollmentAvailability();
        }

        private void LoadInstructorDetails()
        {
            if (_course.Instructor != null)
            {
                txtInstructorName.Text = _course.Instructor.InstructorName;
                txtInstructorEmail.Text = _course.Instructor.Email ?? "Không có thông tin";
                txtInstructorExperience.Text = _course.Instructor.Experience > 0
                    ? $"{_course.Instructor.Experience} năm"
                    : "Chưa có thông tin";
            }
            else
            {
                txtInstructorName.Text = _course.InstructorName;
                txtInstructorEmail.Text = "Không có thông tin";
                txtInstructorExperience.Text = "Chưa có thông tin";
            }
        }

        private void UpdateStatusCardColor()
        {
            var statusColor = _course.Status switch
            {
                "Mở đăng ký" => new SolidColorBrush(Colors.Green),
                "Đã đóng" => new SolidColorBrush(Colors.Red),
                "Tạm dừng" => new SolidColorBrush(Colors.Orange),
                _ => new SolidColorBrush(Colors.Gray)
            };

            cardStatus.Background = statusColor;
        }

        private void UpdateEnrollmentAvailability()
        {
            if (_course.IsFullyBooked || _course.Status != "Mở đăng ký" || _course.IsAlreadyEnrolled)
            {
                btnEnrollNow.IsEnabled = false;

                if (_course.IsFullyBooked)
                {
                    cardWarning.Visibility = Visibility.Visible;
                    btnEnrollNow.Content = "ĐÃ HẾT CHỖ";
                }
                else if (_course.IsAlreadyEnrolled)
                {
                    btnEnrollNow.Content = "ĐÃ ĐĂNG KÝ";
                    btnEnrollNow.Background = new SolidColorBrush(Colors.Gray);
                }
                else if (_course.Status != "Mở đăng ký")
                {
                    btnEnrollNow.Content = "KHÔNG THỂ ĐĂNG KÝ";
                    btnEnrollNow.Background = new SolidColorBrush(Colors.Gray);
                }
            }
            else
            {
                btnEnrollNow.IsEnabled = true;
                cardWarning.Visibility = Visibility.Collapsed;
                btnEnrollNow.Content = "ĐĂNG KÝ NGAY";
            }
        }
        #endregion

        #region Event Handlers
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }

        private void BtnEnrollNow_Click(object sender, RoutedEventArgs e)
        {
            if (!btnEnrollNow.IsEnabled)
                return;

            // Close this dialog and return the course for enrollment
            var result = new CourseDetailsResult
            {
                ShouldEnroll = true,
                Course = _course
            };

            DialogHost.CloseDialogCommand.Execute(result, this);
        }
        #endregion
    }

    // Result class for dialog communication
    public class CourseDetailsResult
    {
        public bool ShouldEnroll { get; set; }
        public CourseViewModel Course { get; set; }
    }
}