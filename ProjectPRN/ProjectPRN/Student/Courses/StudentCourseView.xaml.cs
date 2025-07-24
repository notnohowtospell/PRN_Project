using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using ProjectPRN.Utils;

namespace ProjectPRN.Student.Courses
{
    public partial class StudentCourseView : Window, INotifyPropertyChanged
    {
        private readonly ApplicationDbContext _context;

        public ObservableCollection<CourseViewModel> FilteredCourses { get; set; }

        private StudentViewModel _currentStudent;
        public StudentViewModel CurrentStudent
        {
            get => _currentStudent;
            set
            {
                _currentStudent = value;
                OnPropertyChanged();
                if (value != null)
                {
                    txtStudentName.Text = value.StudentName?.ToUpper() ?? "HỒ SƠ";
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public StudentCourseView()
        {
            InitializeComponent();
            DataContext = this;
            _context = new ApplicationDbContext();

            // Check if user is logged in and is a student
            if (!SessionManager.IsLoggedIn || !SessionManager.IsStudent)
            {
                MessageBox.Show("Bạn cần đăng nhập bằng tài khoản sinh viên để truy cập trang này.", 
                              "Không có quyền truy cập", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                this.Close();
                return;
            }

            int currentStudentId = SessionManager.GetCurrentUserId();
            if (currentStudentId == 0)
            {
                MessageBox.Show("Không thể xác định thông tin sinh viên hiện tại.", 
                              "Lỗi session", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                this.Close();
                return;
            }

            FilteredCourses = new ObservableCollection<CourseViewModel>();

            InitializeData(currentStudentId);
        }

        private void InitializeData(int studentId)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tải dữ liệu...";

                LoadCurrentStudent(studentId);
                LoadInstructors();
                LoadCourses();

                UpdateUI();
                txtStatus.Text = "Sẵn sàng";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi tải dữ liệu: {ex.Message}";
                MessageBox.Show($"Không thể tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadCurrentStudent(int studentId)
        {
            try
            {
                var student = _context.Students.FirstOrDefault(s => s.StudentId == studentId);
                if (student != null)
                {
                    CurrentStudent = new StudentViewModel
                    {
                        StudentId = student.StudentId,
                        StudentName = student.StudentName,
                        Email = student.Email,
                        Status = student.Status
                    };
                }
                else
                {
                    throw new Exception($"Không tìm thấy thông tin sinh viên với ID: {studentId}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải thông tin sinh viên: {ex.Message}", ex);
            }
        }


        private void LoadInstructors()
        {
            var instructors = _context.Instructors.ToList();

            cmbInstructorFilter.Items.Clear();
            cmbInstructorFilter.Items.Add(new ComboBoxItem { Content = "Tất cả giảng viên", Tag = -1 });

            foreach (var instructor in instructors)
            {
                cmbInstructorFilter.Items.Add(new ComboBoxItem
                {
                    Content = instructor.InstructorName,
                    Tag = instructor.InstructorId
                });
            }

            cmbInstructorFilter.SelectedIndex = 0;
        }


        private void LoadCourses()
        {
            try
            {
                if (CurrentStudent == null)
                {
                    throw new Exception("Thông tin sinh viên chưa được tải.");
                }

                var courses = _context.LifeSkillCourses
                    .Include(c => c.Instructor)
                    .Where(c => c.Status == "Mở đăng ký")
                    .ToList();

                var sentRequests = _context.Payments
                   .Where(p => p.StudentId == CurrentStudent.StudentId && p.Status != "Đã thanh toán")
                   .Select(p => p.CourseId)
                   .ToHashSet();

                FilteredCourses.Clear();

                foreach (var course in courses)
                {
                    var courseEnrollments = _context.Enrollments
                        .Where(e => e.CourseId == course.CourseId && e.CompletionStatus != false)
                        .ToList();

                    FilteredCourses.Add(new CourseViewModel
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName ?? "N/A",
                        InstructorName = course.Instructor?.InstructorName ?? "N/A",
                        StartDate = course.StartDate,
                        EndDate = course.EndDate,
                        Price = course.Price,
                        Description = course.Description ?? "",
                        MaxStudents = course.MaxStudents ?? 0,
                        CurrentEnrollments = courseEnrollments.Count,
                        SentRequest = sentRequests.Contains(course.CourseId),
                        Status = course.Status ?? "N/A",
                        Instructor = course.Instructor
                    });
                }

                dgCourses.ItemsSource = FilteredCourses;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách khóa học: {ex.Message}", ex);
            }
        }


        private void ApplyFilter()
        {
            try
            {
                var query = _context.LifeSkillCourses
                    .Include(c => c.Instructor)
                    .Where(c => c.Status == "Mở đăng ký");

                if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
                {
                    var search = txtSearch.Text.Trim().ToLower();
                    query = query.Where(c =>
                        (c.CourseName != null && c.CourseName.ToLower().Contains(search)) ||
                        (c.Description != null && c.Description.ToLower().Contains(search)) ||
                        (c.Instructor != null && c.Instructor.InstructorName != null && c.Instructor.InstructorName.ToLower().Contains(search)));
                }

                if (cmbInstructorFilter?.SelectedItem is ComboBoxItem item &&
                    item.Tag is int instructorId && instructorId != -1)
                {
                    query = query.Where(c => c.InstructorId == instructorId);
                }

                var filteredCourses = query.ToList();
                RefreshFilteredCourses(filteredCourses);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc khóa học: {ex.Message}", "Lỗi", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshFilteredCourses(IEnumerable<LifeSkillCourse> courses)
        {
            try
            {
                if (CurrentStudent == null)
                {
                    throw new Exception("Thông tin sinh viên chưa được tải.");
                }

                var enrollments = _context.Enrollments
                    .Where(e => e.StudentId == CurrentStudent.StudentId)
                    .ToList();

                var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToHashSet();

                FilteredCourses.Clear();

                foreach (var course in courses)
                {
                    var courseEnrollments = _context.Enrollments
                        .Where(e => e.CourseId == course.CourseId && e.CompletionStatus != false)
                        .ToList();

                    FilteredCourses.Add(new CourseViewModel
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName ?? "N/A",
                        InstructorName = course.Instructor?.InstructorName ?? "N/A",
                        StartDate = course.StartDate,
                        EndDate = course.EndDate,
                        Price = course.Price,
                        Description = course.Description ?? "",
                        MaxStudents = course.MaxStudents ?? 0,
                        CurrentEnrollments = courseEnrollments.Count,
                        Status = course.Status ?? "N/A",
                        SentRequest = enrolledCourseIds.Contains(course.CourseId),
                        Instructor = course.Instructor
                    });
                }

                UpdateUI();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi làm mới danh sách khóa học: {ex.Message}", ex);
            }
        }


        private async Task ProcessEnrollmentAsync(CourseViewModel course, EnrollmentResult result)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang xử lý đăng ký...";

                var enrollment = new Enrollment
                {
                    StudentId = CurrentStudent.StudentId,
                    CourseId = course.CourseId,
                    CompletionStatus = false,
                    CompletionDate = null
                };

                await _context.Enrollments.AddAsync(enrollment);

                var payment = new Payment
                {
                    StudentId = CurrentStudent.StudentId,
                    CourseId = course.CourseId,
                    Amount = course.Price ?? 0,
                    PaymentDate = DateTime.Now,
                    Status = "Pending"
                };

                await _context.Payments.AddAsync(payment);

                await _context.SaveChangesAsync();

                txtStatus.Text = $"Đăng ký thành công: {course.CourseName}";
                MessageBox.Show("Đăng ký thành công!\nVui lòng chờ xác nhận.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadCourses();
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi đăng ký khóa học: {ex.Message}";
                MessageBox.Show($"Không thể đăng ký: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }


        #region Event Handlers
        private async void BtnEnroll_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;

            var button = sender as Button;
            if (button?.Tag is int courseId)
            {
                var course = FilteredCourses.FirstOrDefault(c => c.CourseId == courseId);
                if (course != null)
                {
                    await ShowEnrollmentDialogAsync(course);
                }
            }
        }

        private async void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int courseId)
            {
                var course = FilteredCourses.FirstOrDefault(c => c.CourseId == courseId);
                if (course != null)
                {
                    await ShowCourseDetailsDialogAsync(course);
                }
            }
        }

        private void BtnMyEnrollments_Click(object sender, RoutedEventArgs e)
        {
            var enrolledCoursesWindow = new EnrolledCoursesWindow(CurrentStudent);
            enrolledCoursesWindow.ShowDialog();
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbInstructorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbInstructorFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0;
            ApplyFilter();
        }

        private void DgCourses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle selection if needed
        }
        #endregion

        #region Dialog Methods
        private async Task ShowEnrollmentDialogAsync(CourseViewModel course)
        {
            try
            {
                var dialog = new CourseEnrollmentDialog(course, CurrentStudent);
                var result = await DialogHost.Show(dialog, "RootDialog");

                if (result is EnrollmentResult enrollmentResult && enrollmentResult.Success)
                {
                    await ProcessEnrollmentAsync(course, enrollmentResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị dialog đăng ký: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ShowCourseDetailsDialogAsync(CourseViewModel course)
        {
            try
            {
                var dialog = new CourseDetailsDialog(course);
                var result = await DialogHost.Show(dialog, "RootDialog");

                // If user clicked "ĐĂNG KÝ NGAY" in the details dialog
                if (result is CourseDetailsResult detailsResult && detailsResult.ShouldEnroll)
                {
                    await ShowEnrollmentDialogAsync(detailsResult.Course);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị chi tiết khóa học: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateUI()
        {
            if (txtTotalCourses != null)
            {
                txtTotalCourses.Text = FilteredCourses?.Count.ToString() ?? "0";
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
        #endregion
    }

    // View Models
    public class CourseViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public int MaxStudents { get; set; }
        public int CurrentEnrollments { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool SentRequest { get; set; }
        public Instructor Instructor { get; set; }

        public bool CanEnroll => Status == "Mở đăng ký" && !IsFullyBooked;
        public bool DidNotSentRequest => !SentRequest;
        public bool CanEnrollAndNotEnrolled => CanEnroll && DidNotSentRequest;
        public bool IsFullyBooked => CurrentEnrollments >= MaxStudents;
        public bool IsAlmostFull => MaxStudents > 0 && (double)CurrentEnrollments / MaxStudents >= 0.8;
    }

    public class StudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class EnrollmentResult
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}