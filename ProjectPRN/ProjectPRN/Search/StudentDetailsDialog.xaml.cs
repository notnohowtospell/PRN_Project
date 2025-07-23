using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;

namespace ProjectPRN.Search
{
    /// <summary>
    /// Interaction logic for StudentDetailsDialog.xaml
    /// </summary>
    public partial class StudentDetailsDialog : UserControl
    {
        private readonly StudentSearchViewModel _student;
        private readonly ApplicationDbContext _context;

        public StudentDetailsDialog(StudentSearchViewModel student)
        {
            InitializeComponent();

            _student = student;
            _context = new ApplicationDbContext();

            LoadStudentDetails();
        }

        #region Initialization
        private void LoadStudentDetails()
        {
            // Basic information
            txtStudentName.Text = _student.StudentName;
            txtStudentCode.Text = $"Mã sinh viên: {_student.StudentCode}";
            txtEmail.Text = _student.Email ?? "Không có thông tin";
            txtPhoneNumber.Text = _student.PhoneNumber ?? "Không có thông tin";
            txtDateOfBirth.Text = _student.DateOfBirth?.ToString("dd/MM/yyyy") ?? "Không có thông tin";

            // Set avatar initials
            SetAvatarInitials();

            // Status
            txtStatus.Text = _student.Status;
            UpdateStatusColor();

            // Load detailed statistics
            LoadStudentStatistics();
            LoadEnrolledCourses();
            LoadPaymentHistory();
        }

        private void SetAvatarInitials()
        {
            if (!string.IsNullOrEmpty(_student.StudentName))
            {
                var words = _student.StudentName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                {
                    txtAvatarInitials.Text = $"{words[0][0]}{words[words.Length - 1][0]}".ToUpper();
                }
                else if (words.Length == 1)
                {
                    txtAvatarInitials.Text = words[0].Substring(0, Math.Min(2, words[0].Length)).ToUpper();
                }
            }
        }

        private void UpdateStatusColor()
        {
            var statusColor = _student.Status switch
            {
                "Active" => new SolidColorBrush(Colors.Green),
                "InActive" => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Gray)
            };

            cardStatus.Background = statusColor;
        }

        private void LoadStudentStatistics()
        {
            try
            {
                var student = _context.Students
                    .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                    .Include(s => s.Payments)
                    .FirstOrDefault(s => s.StudentId == _student.StudentId);

                if (student != null)
                {
                    var totalCourses = student.Enrollments?.Count ?? 0;
                    var completedCourses = student.Enrollments?.Count(e => e.CompletionStatus == true) ?? 0;
                    var pendingPayments = student.Payments?.Count(p => p.Status == "Pending") ?? 0;
                    var totalPayments = student.Payments?.Where(p => p.Status == "Đã thanh toán").Sum(p => p.Amount) ?? 0;

                    txtTotalCourses.Text = totalCourses.ToString();
                    txtCompletedCourses.Text = completedCourses.ToString();
                    txtPendingPayments.Text = pendingPayments.ToString();
                    txtTotalPayments.Text = totalPayments.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
                }
                else
                {
                    // Use data from view model if detailed data not found
                    txtTotalCourses.Text = _student.EnrollmentCount.ToString();
                    txtCompletedCourses.Text = "0";
                    txtPendingPayments.Text = _student.HasPendingPayments ? "1" : "0";
                    txtTotalPayments.Text = "0";
                }
            }
            catch (Exception ex)
            {
                // Fallback to view model data
                txtTotalCourses.Text = _student.EnrollmentCount.ToString();
                txtCompletedCourses.Text = "N/A";
                txtPendingPayments.Text = "N/A";
                txtTotalPayments.Text = "N/A";
            }
        }

        private void LoadEnrolledCourses()
        {
            try
            {
                var enrollments = _context.Enrollments
                    .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                    .Include(e => e.Student)
                    .Where(e => e.StudentId == _student.StudentId)
                    .ToList();

                if (enrollments.Any())
                {
                    var courseList = new List<StudentCourseViewModel>();

                    foreach (var enrollment in enrollments)
                    {
                        // Get payment status for this course
                        var payment = _context.Payments
                            .FirstOrDefault(p => p.StudentId == _student.StudentId &&
                                                p.CourseId == enrollment.CourseId);

                        courseList.Add(new StudentCourseViewModel
                        {
                            CourseName = enrollment.Course?.CourseName ?? "N/A",
                            InstructorName = enrollment.Course?.Instructor?.InstructorName ?? "N/A",
                            EnrollmentStatus = enrollment.CompletionStatus == true ? "Hoàn thành" :
                                             enrollment.CompletionStatus == false ? "Đang học" : "Chưa bắt đầu",
                            PaymentStatus = payment?.Status ?? "Chưa thanh toán"
                        });
                    }

                    dgCourses.ItemsSource = courseList;
                    txtNoCoursesMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgCourses.ItemsSource = null;
                    txtNoCoursesMessage.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                dgCourses.ItemsSource = null;
                txtNoCoursesMessage.Visibility = Visibility.Visible;
            }
        }

        private void LoadPaymentHistory()
        {
            try
            {
                var payments = _context.Payments
                    .Include(p => p.Course)
                    .Where(p => p.StudentId == _student.StudentId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToList();

                if (payments.Any())
                {
                    var paymentList = payments.Select(p => new StudentPaymentViewModel
                    {
                        CourseName = p.Course?.CourseName ?? "N/A",
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        Status = p.Status
                    }).ToList();

                    dgPayments.ItemsSource = paymentList;
                    txtNoPaymentsMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgPayments.ItemsSource = null;
                    txtNoPaymentsMessage.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                dgPayments.ItemsSource = null;
                txtNoPaymentsMessage.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Event Handlers
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Close this dialog and return the student for editing
            var result = new StudentDetailsResult
            {
                ShouldEdit = true,
                Student = _student
            };

            DialogHost.CloseDialogCommand.Execute(result, this);
        }

        private void BtnViewAllCourses_Click(object sender, RoutedEventArgs e)
        {
            // Close this dialog and show all courses for this student
            var result = new StudentDetailsResult
            {
                ShouldViewCourses = true,
                Student = _student
            };

            DialogHost.CloseDialogCommand.Execute(result, this);
        }
        #endregion
    }

    // View Models for DataGrid
    public class StudentCourseViewModel
    {
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string EnrollmentStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class StudentPaymentViewModel
    {
        public string CourseName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // Result class for dialog communication
    public class StudentDetailsResult
    {
        public bool ShouldEdit { get; set; }
        public bool ShouldViewCourses { get; set; }
        public StudentSearchViewModel Student { get; set; }
    }
}
