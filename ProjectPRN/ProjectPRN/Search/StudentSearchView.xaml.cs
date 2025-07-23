using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;

namespace ProjectPRN.Search
{
    public partial class StudentSearchView : Window, INotifyPropertyChanged
    {
        private readonly ApplicationDbContext _context;
        public ObservableCollection<StudentSearchViewModel> FilteredStudents { get; set; }

        private StudentSearchViewModel _selectedStudent;
        public StudentSearchViewModel SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                OnPropertyChanged();
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

        public StudentSearchView()
        {
            InitializeComponent();
            DataContext = this;
            _context = new ApplicationDbContext();

            FilteredStudents = new ObservableCollection<StudentSearchViewModel>();

            InitializeData();
        }

        #region Data Loading
        private void InitializeData()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tải dữ liệu...";

                LoadCourses();
                LoadStudents();
                UpdateUI();

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

        private void LoadCourses()
        {
            var courses = _context.LifeSkillCourses.ToList();

            cmbCourseFilter.Items.Clear();
            cmbCourseFilter.Items.Add(new ComboBoxItem { Content = "Tất cả khóa học", Tag = -1 });

            foreach (var course in courses)
            {
                cmbCourseFilter.Items.Add(new ComboBoxItem
                {
                    Content = course.CourseName,
                    Tag = course.CourseId
                });
            }

            cmbCourseFilter.SelectedIndex = 0;
        }

        private void LoadStudents()
        {
            var students = _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                .Include(s => s.Payments)
                .ToList();

            FilteredStudents.Clear();

            foreach (var student in students)
            {
                var enrollmentCount = student.Enrollments?.Count ?? 0;
                var hasPendingPayments = student.Payments?.Any(p => p.Status == "Pending") ?? false;

                FilteredStudents.Add(new StudentSearchViewModel
                {
                    StudentId = student.StudentId,
                    StudentCode = student.StudentCode,
                    StudentName = student.StudentName,
                    Email = student.Email,
                    PhoneNumber = student.PhoneNumber,
                    DateOfBirth = student.DateOfBirth,
                    Status = student.Status,
                    EnrollmentCount = enrollmentCount,
                    RegistrationDate = student.DateOfBirth, // Assuming this is registration date
                    HasPendingPayments = hasPendingPayments,
                    Enrollments = student.Enrollments?.ToList() ?? new List<Enrollment>()
                });
            }

            dgStudents.ItemsSource = FilteredStudents;
        }
        #endregion

        #region Event Handlers
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbEnrollmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbCourseFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbStatusFilter.SelectedIndex = 0;
            cmbEnrollmentFilter.SelectedIndex = 0;
            cmbCourseFilter.SelectedIndex = 0;
            dpDateFrom.SelectedDate = null;
            dpDateTo.SelectedDate = null;
            ApplyFilter();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO: Implement Excel export functionality
                MessageBox.Show("Chức năng xuất Excel đang được phát triển.", "Thông báo",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int studentId)
            {
                var student = FilteredStudents.FirstOrDefault(s => s.StudentId == studentId);
                if (student != null)
                {
                    await ShowStudentDetailsDialogAsync(student);
                }
            }
        }

        private void DgStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedStudent = dgStudents.SelectedItem as StudentSearchViewModel;
        }
        #endregion

        #region Filtering
        private void ApplyFilter()
        {
            var query = _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                .Include(s => s.Payments)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                var searchTerm = txtSearch.Text.Trim().ToLower();
                query = query.Where(s =>
                    s.StudentName.ToLower().Contains(searchTerm) ||
                    s.Email.ToLower().Contains(searchTerm) ||
                    s.StudentCode.ToLower().Contains(searchTerm) ||
                    s.PhoneNumber.ToLower().Contains(searchTerm));
            }

            // Status filter
            if (cmbStatusFilter?.SelectedItem is ComboBoxItem statusItem &&
                statusItem.Content.ToString() != "Tất cả trạng thái")
            {
                var status = statusItem.Content.ToString();
                query = query.Where(s => s.Status == status);
            }

            // Enrollment filter
            if (cmbEnrollmentFilter?.SelectedItem is ComboBoxItem enrollmentItem &&
                enrollmentItem.Content.ToString() != "Tất cả")
            {
                var enrollmentType = enrollmentItem.Content.ToString();
                switch (enrollmentType)
                {
                    case "Có khóa học":
                        query = query.Where(s => s.Enrollments.Any());
                        break;
                    case "Không có khóa học":
                        query = query.Where(s => !s.Enrollments.Any());
                        break;
                    case "Đang chờ thanh toán":
                        query = query.Where(s => s.Payments.Any(p => p.Status == "Pending"));
                        break;
                }
            }

            // Course filter
            if (cmbCourseFilter?.SelectedItem is ComboBoxItem courseItem &&
                courseItem.Tag is int courseId && courseId != -1)
            {
                query = query.Where(s => s.Enrollments.Any(e => e.CourseId == courseId));
            }

            // Date range filter
            if (dpDateFrom?.SelectedDate.HasValue == true)
            {
                query = query.Where(s => s.DateOfBirth >= dpDateFrom.SelectedDate.Value);
            }

            if (dpDateTo?.SelectedDate.HasValue == true)
            {
                query = query.Where(s => s.DateOfBirth <= dpDateTo.SelectedDate.Value);
            }

            var filteredStudents = query.ToList();
            RefreshFilteredStudents(filteredStudents);
        }

        private void RefreshFilteredStudents(IEnumerable<BusinessObjects.Models.Student> students)
        {
            FilteredStudents.Clear();

            foreach (var student in students)
            {
                var enrollmentCount = student.Enrollments?.Count ?? 0;
                var hasPendingPayments = student.Payments?.Any(p => p.Status == "Pending") ?? false;

                FilteredStudents.Add(new StudentSearchViewModel
                {
                    StudentId = student.StudentId,
                    StudentCode = student.StudentCode,
                    StudentName = student.StudentName,
                    Email = student.Email,
                    PhoneNumber = student.PhoneNumber,
                    DateOfBirth = student.DateOfBirth,
                    Status = student.Status,
                    EnrollmentCount = enrollmentCount,
                    RegistrationDate = student.DateOfBirth,
                    HasPendingPayments = hasPendingPayments,
                    Enrollments = student.Enrollments?.ToList() ?? new List<Enrollment>()
                });
            }

            UpdateUI();
        }
        #endregion

        #region Dialog Methods
        private async Task ShowStudentDetailsDialogAsync(StudentSearchViewModel student)
        {
            try
            {
                var dialog = new StudentDetailsDialog(student);
                await DialogHost.Show(dialog, "RootDialog");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị chi tiết sinh viên: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateUI()
        {
            if (txtTotalStudents != null)
            {
                txtTotalStudents.Text = FilteredStudents?.Count.ToString() ?? "0";
            }

            if (txtResultCount != null)
            {
                txtResultCount.Text = FilteredStudents?.Count.ToString() ?? "0";
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

    // View Model
    public class StudentSearchViewModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Status { get; set; } = string.Empty;
        public int EnrollmentCount { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool HasPendingPayments { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
