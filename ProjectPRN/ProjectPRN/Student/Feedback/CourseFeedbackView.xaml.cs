using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using ProjectPRN.Utils;

namespace ProjectPRN.Student.Feedback
{
    public partial class CourseFeedbackView : UserControl, INotifyPropertyChanged
    {
        private readonly BusinessObjects.Models.Student _currentStudent;
        private List<CourseForFeedback> _allCourses = new List<CourseForFeedback>();
        private ObservableCollection<CourseForFeedback> _filteredCourses = new ObservableCollection<CourseForFeedback>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                LoadingOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public CourseFeedbackView(BusinessObjects.Models.Student student)
        {
            InitializeComponent();
            DataContext = this;
            
            _currentStudent = student ?? throw new ArgumentNullException(nameof(student));
            dgCourses.ItemsSource = _filteredCourses;
            
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tải danh sách khóa học...";

                using var context = new ApplicationDbContext();
                //MessageBox.Show("Bat dau load");

                // Load enrolled courses with their feedback status in one query
                var enrolledCourses = await context.Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                     //.Include(e => e.Course.Instructor)
                    .Where(e => e.StudentId == _currentStudent.StudentId)
                    .ToListAsync();
                //MessageBox.Show("Enrolled course load xong!");
                // Load all feedbacks for this student to avoid multiple queries
                var allFeedbacks = await context.Feedbacks
                    .Where(f => f.StudentId == _currentStudent.StudentId)
                    .ToListAsync();
                //MessageBox.Show("feedback load xong!");

                _allCourses.Clear();

                foreach (var enrollment in enrolledCourses)
                {
                    // Find existing feedback for this course
                    var existingFeedback = allFeedbacks.FirstOrDefault(f => f.CourseId == enrollment.CourseId);

                    var courseForFeedback = new CourseForFeedback
                    {
                        CourseId = enrollment.CourseId,
                        CourseName = enrollment.Course.CourseName,
                        InstructorName = enrollment.Course.Instructor?.InstructorName ?? "N/A",
                        EnrollmentDate = DateTime.Now, // You might want to add this field to Enrollment model
                        EnrollmentDateText = DateTime.Now.ToString("dd/MM/yyyy"),
                        CompletionStatus = enrollment.CompletionStatus,
                        CompletionStatusText = enrollment.CompletionStatus ? "Đã hoàn thành" : "Đang học",
                        HasFeedback = existingFeedback != null,
                        CurrentRating = existingFeedback?.Rating ?? 0,
                        CurrentRatingText = existingFeedback != null ? $"{existingFeedback.Rating}/5 ⭐" : "Chưa đánh giá",
                        FeedbackDate = existingFeedback?.FeedbackDate,
                        FeedbackDateText = existingFeedback?.FeedbackDate.ToString("dd/MM/yyyy") ?? "N/A",
                        FeedbackComment = existingFeedback?.Comment ?? "",
                        FeedbackButtonTooltip = existingFeedback != null ? "Chỉnh sửa đánh giá" : "Tạo đánh giá mới"
                    };

                    _allCourses.Add(courseForFeedback);
                }

                ApplyFilter();
                UpdateStatusBar();
                txtStatus.Text = "Đã tải dữ liệu thành công";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi: {ex.Message}";
                MessageBox.Show($"Không thể tải dữ liệu: {ex.Message}", "Lỗi", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilter()
        {
            var searchText = txtSearch?.Text?.ToLower() ?? "";
            var statusFilter = "All";
            
            var filtered = _allCourses.Where(c => 
                // Search filter
                (string.IsNullOrEmpty(searchText) ||
                 c.CourseName.ToLower().Contains(searchText) ||
                 c.InstructorName.ToLower().Contains(searchText)) &&
                
                // Status filter
                (statusFilter == "All" ||
                 (statusFilter == "Rated" && c.HasFeedback) ||
                 (statusFilter == "NotRated" && !c.HasFeedback))
            ).OrderBy(c => c.CourseName).ToList();

            _filteredCourses.Clear();
            foreach (var course in filtered)
            {
                _filteredCourses.Add(course);
            }

            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            var ratedCount = _filteredCourses.Count(c => c.HasFeedback);
            var notRatedCount = _filteredCourses.Count(c => !c.HasFeedback);
            
            txtRecordCount.Text = $"Tổng số khóa học: {_filteredCourses.Count} " +
                                 $"(Đã đánh giá: {ratedCount}, Chưa đánh giá: {notRatedCount})";
            txtLastUpdated.Text = $"Cập nhật: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }

        #region Event Handlers
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async void BtnFeedback_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int courseId)
            {
                var course = _filteredCourses.FirstOrDefault(c => c.CourseId == courseId);
                if (course != null)
                {
                    await OpenFeedbackDialog(course);
                }
            }
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int courseId)
            {
                MessageBox.Show($"Xem chi tiết khóa học ID: {courseId}\nChức năng sẽ được triển khai.", 
                               "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DgCourses_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCourses.SelectedItem is CourseForFeedback selectedCourse)
            {
                await OpenFeedbackDialog(selectedCourse);
            }
        }

        private async Task OpenFeedbackDialog(CourseForFeedback course)
        {
            try
            {
                txtStatus.Text = "Đang mở dialog đánh giá...";

                // Create course object for FeedbackDialog
                var courseForDialog = new LifeSkillCourse
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName
                };

                // Get existing feedback if it exists (already loaded in the CourseForFeedback model)
                BusinessObjects.Models.Feedback? existingFeedback = null;
                
                if (course.HasFeedback)
                {
                    using var context = new ApplicationDbContext();
                    existingFeedback = await context.Feedbacks
                        .FirstOrDefaultAsync(f => f.StudentId == _currentStudent.StudentId && 
                                                 f.CourseId == course.CourseId);
                }

                var feedbackDialog = new FeedbackDialog(_currentStudent, courseForDialog, existingFeedback);
                var result = feedbackDialog.ShowDialog();

                if (result == true)
                {
                    txtStatus.Text = "Đánh giá đã được lưu thành công";
                    await LoadDataAsync(); // Refresh the list
                    MessageBox.Show("Cảm ơn bạn đã đánh giá khóa học!", "Thành công", 
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    txtStatus.Text = "Đã hủy đánh giá";
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi mở dialog: {ex.Message}";
                MessageBox.Show($"Không thể mở dialog đánh giá: {ex.Message}", "Lỗi", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    // Data model for courses available for feedback
    public class CourseForFeedback
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string EnrollmentDateText { get; set; } = string.Empty;
        public bool CompletionStatus { get; set; }
        public string CompletionStatusText { get; set; } = string.Empty;
        public bool HasFeedback { get; set; }
        public int CurrentRating { get; set; }
        public string CurrentRatingText { get; set; } = string.Empty;
        public DateTime? FeedbackDate { get; set; }
        public string FeedbackDateText { get; set; } = string.Empty;
        public string FeedbackComment { get; set; } = string.Empty;
        public string FeedbackButtonTooltip { get; set; } = string.Empty;
        
        // Additional properties for better UI
        public string StatusColor => HasFeedback ? "#4CAF50" : "#FF9800";
        public string StatusIcon => HasFeedback ? "✓" : "○";
        public string RatingStars => HasFeedback ? new string('★', CurrentRating) + new string('☆', 5 - CurrentRating) : "☆☆☆☆☆";
    }
}