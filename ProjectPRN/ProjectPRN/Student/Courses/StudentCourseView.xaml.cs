using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using Repositories.Interfaces;

namespace ProjectPRN.Student.Courses
{
    public partial class StudentCourseView : Window, INotifyPropertyChanged
    {
        private readonly ILifeSkillCourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IPaymentRepository _paymentRepository;

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

        public StudentCourseView(
            ILifeSkillCourseRepository courseRepository,
            IInstructorRepository instructorRepository,
            IStudentRepository studentRepository,
            IEnrollmentRepository enrollmentRepository,
            IPaymentRepository paymentRepository,
            int currentStudentId)
        {
            InitializeComponent();
            DataContext = this;

            _courseRepository = courseRepository;
            _instructorRepository = instructorRepository;
            _studentRepository = studentRepository;
            _enrollmentRepository = enrollmentRepository;
            _paymentRepository = paymentRepository;

            FilteredCourses = new ObservableCollection<CourseViewModel>();

            _ = InitializeDataAsync(currentStudentId);
        }

        #region Data Loading
        private async Task InitializeDataAsync(int studentId)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tải dữ liệu...";

                await LoadCurrentStudentAsync(studentId);
                await LoadInstructorsAsync();
                await LoadCoursesAsync();

                txtStatus.Text = "Sẵn sàng";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi tải dữ liệu: {ex.Message}";
                MessageBox.Show($"Không thể tải dữ liệu: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCurrentStudentAsync(int studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
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
        }

        private async Task LoadInstructorsAsync()
        {
            try
            {
                var instructors = await _instructorRepository.GetAllAsync();

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
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách giảng viên: {ex.Message}", ex);
            }
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                var courses = await _courseRepository.GetAllAsync();
                var enrollments = await _enrollmentRepository.GetByStudentAsync(CurrentStudent.StudentId);
                var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToHashSet();

                FilteredCourses.Clear();

                foreach (var course in courses.Where(c => c.Status == "Mở đăng ký"))
                {
                    var courseEnrollments = await _enrollmentRepository.GetByCourseAsync(course.CourseId);
                    var currentEnrollments = courseEnrollments.Count(e => e.CompletionStatus != false);

                    var courseViewModel = new CourseViewModel
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        InstructorName = course.Instructor?.InstructorName ?? "N/A",
                        StartDate = course.StartDate,
                        EndDate = course.EndDate,
                        Price = course.Price,
                        Description = course.Description,
                        MaxStudents = course.MaxStudents ?? 0,
                        CurrentEnrollments = currentEnrollments,
                        Status = course.Status,
                        IsAlreadyEnrolled = enrolledCourseIds.Contains(course.CourseId),
                        Instructor = course.Instructor
                    };

                    FilteredCourses.Add(courseViewModel);
                }

                dgCourses.ItemsSource = FilteredCourses;
                UpdateUI();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách khóa học: {ex.Message}", ex);
            }
        }
        #endregion

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
            // TODO: Open My Enrollments window
            MessageBox.Show("Chức năng đang phát triển", "Thông báo",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open Profile window
            MessageBox.Show("Chức năng đang phát triển", "Thông báo",
                           MessageBoxButton.OK, MessageBoxImage.Information);
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

        #region Filtering
        private void ApplyFilter()
        {
            if (FilteredCourses == null) return;

            var allCourses = _courseRepository.GetAll().Where(c => c.Status == "Mở đăng ký");
            var filtered = allCourses.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                var searchTerm = txtSearch.Text.Trim().ToLower();
                filtered = filtered.Where(c =>
                    c.CourseName?.ToLower().Contains(searchTerm) == true ||
                    c.Description?.ToLower().Contains(searchTerm) == true ||
                    c.Instructor?.InstructorName?.ToLower().Contains(searchTerm) == true);
            }

            // Instructor filter
            if (cmbInstructorFilter?.SelectedItem is ComboBoxItem instructorItem &&
                instructorItem.Tag is int instructorId && instructorId != -1)
            {
                filtered = filtered.Where(c => c.InstructorId == instructorId);
            }

            // Refresh the filtered courses display
            _ = RefreshFilteredCoursesAsync(filtered);
        }

        private async Task RefreshFilteredCoursesAsync(IEnumerable<LifeSkillCourse> courses)
        {
            try
            {
                var enrollments = await _enrollmentRepository.GetByStudentAsync(CurrentStudent.StudentId);
                var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToHashSet();

                FilteredCourses.Clear();

                foreach (var course in courses)
                {
                    var courseEnrollments = await _enrollmentRepository.GetByCourseAsync(course.CourseId);
                    var currentEnrollments = courseEnrollments.Count(e => e.CompletionStatus != false);

                    var courseViewModel = new CourseViewModel
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        InstructorName = course.Instructor?.InstructorName ?? "N/A",
                        StartDate = course.StartDate,
                        EndDate = course.EndDate,
                        Price = course.Price,
                        Description = course.Description,
                        MaxStudents = course.MaxStudents ?? 0,
                        CurrentEnrollments = currentEnrollments,
                        Status = course.Status,
                        IsAlreadyEnrolled = enrolledCourseIds.Contains(course.CourseId),
                        Instructor = course.Instructor
                    };

                    FilteredCourses.Add(courseViewModel);
                }

                UpdateUI();
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi lọc khóa học: {ex.Message}";
            }
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
                await DialogHost.Show(dialog, "RootDialog");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị chi tiết khóa học: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ProcessEnrollmentAsync(CourseViewModel course, EnrollmentResult result)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang xử lý đăng ký...";

                // Create enrollment
                var enrollment = new Enrollment
                {
                    StudentId = CurrentStudent.StudentId,
                    CourseId = course.CourseId,
                    CompletionStatus = "Pending", // Waiting for payment confirmation
                    CompletionDate = null
                };

                await _enrollmentRepository.AddAsync(enrollment);

                // Create payment record
                var payment = new Payment
                {
                    StudentId = CurrentStudent.StudentId,
                    CourseId = course.CourseId,
                    Amount = course.Price ?? 0,
                    PaymentDate = DateTime.Now,
                    Status = "Pending" // Will be confirmed by admin
                };

                await _paymentRepository.AddAsync(payment);

                await _enrollmentRepository.SaveChangesAsync();
                await _paymentRepository.SaveChangesAsync();

                // Refresh the courses list
                await LoadCoursesAsync();

                txtStatus.Text = $"Đã đăng ký khóa học '{course.CourseName}' thành công. Chờ xác nhận thanh toán.";
                MessageBox.Show("Đăng ký thành công!\nVui lòng chờ quản trị viên xác nhận thanh toán.", "Thành công",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi đăng ký khóa học: {ex.Message}";
                MessageBox.Show($"Không thể đăng ký khóa học: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
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
        public bool IsAlreadyEnrolled { get; set; }
        public Instructor Instructor { get; set; }

        public bool CanEnroll => Status == "Mở đăng ký" && !IsFullyBooked;
        public bool IsNotAlreadyEnrolled => !IsAlreadyEnrolled;
        public bool CanEnrollAndNotEnrolled => CanEnroll && IsNotAlreadyEnrolled;
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