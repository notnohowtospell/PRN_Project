using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace ProjectPRN.Student.Courses
{
    public partial class CourseEnrollmentDialog : UserControl
    {
        private readonly CourseViewModel _course;
        private readonly StudentViewModel _student;

        public CourseEnrollmentDialog(CourseViewModel course, StudentViewModel student)
        {
            InitializeComponent();

            _course = course;
            _student = student;

            LoadCourseInformation();
            LoadStudentInformation();
            UpdatePaymentInformation();
        }

        #region Initialization
        private void LoadCourseInformation()
        {
            txtCourseName.Text = _course.CourseName;
            txtInstructor.Text = _course.InstructorName;

            if (_course.StartDate.HasValue && _course.EndDate.HasValue)
            {
                txtDuration.Text = $"{_course.StartDate:dd/MM/yyyy} - {_course.EndDate:dd/MM/yyyy}";
            }
            else
            {
                txtDuration.Text = "Chưa xác định";
            }

            txtPrice.Text = _course.Price?.ToString("C0", CultureInfo.GetCultureInfo("vi-VN")) ?? "Miễn phí";

            var availableSlots = _course.MaxStudents - _course.CurrentEnrollments;
            txtAvailableSlots.Text = $"{availableSlots}/{_course.MaxStudents} chỗ";

            txtDescription.Text = string.IsNullOrWhiteSpace(_course.Description)
                ? "Không có mô tả"
                : _course.Description;
        }

        private void LoadStudentInformation()
        {
            txtStudentName.Text = _student.StudentName;
            txtStudentEmail.Text = _student.Email;
        }

        private void UpdatePaymentInformation()
        {
            var amount = _course.Price?.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) ?? "0";
            var content = $"HOCPHI {_student.StudentName} {_course.CourseName}";

            // Update bank transfer info
            txtBankAmount.Text = $"{amount} VNĐ";
            txtBankContent.Text = content;
        }
        #endregion

        #region Event Handlers

        private void ChkConfirm_Checked(object sender, RoutedEventArgs e)
        {
            btnConfirm.IsEnabled = true;
        }

        private void ChkConfirm_Unchecked(object sender, RoutedEventArgs e)
        {
            btnConfirm.IsEnabled = false;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!chkConfirm.IsChecked == true)
            {
                MessageBox.Show("Vui lòng xác nhận đồng ý với các điều khoản đăng ký.", "Thông báo",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if course is still available
            if (_course.IsFullyBooked)
            {
                MessageBox.Show("Khóa học đã hết chỗ. Vui lòng chọn khóa học khác.", "Thông báo",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn đăng ký khóa học '{_course.CourseName}'?\n\n" +
                $"Học phí: {_course.Price?.ToString("C0", CultureInfo.GetCultureInfo("vi-VN")) ?? "Miễn phí"}\n" +
                $"Phương thức thanh toán: Bank Transfer\n\n" +
                "Sau khi xác nhận, bạn cần thực hiện thanh toán và chờ quản trị viên xác nhận.",
                "Xác nhận đăng ký",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var enrollmentResult = new EnrollmentResult
                {
                    Success = true,
                    PaymentMethod = "Bank Transfer",
                };

                DialogHost.CloseDialogCommand.Execute(enrollmentResult, this);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }
        #endregion

        #region Helper Methods
        #endregion
    }
}