using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using BusinessObjects.Models;
using ProjectPRN.Utils;

namespace ProjectPRN.Admin.StudentProgressManagement
{
    public partial class StudentProgressDetailView : Window, INotifyPropertyChanged
    {
        private readonly StudentProgressSummary _student;
        private ObservableCollection<CourseProgressDisplay> _courseProgressList = new ObservableCollection<CourseProgressDisplay>();

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

        public StudentProgressDetailView(StudentProgressSummary student)
        {
            InitializeComponent();
            DataContext = this;
            
            _student = student ?? throw new ArgumentNullException(nameof(student));
            
            dgCourseProgress.ItemsSource = _courseProgressList;
            
            InitializeDisplay();
            _ = LoadProgressDataAsync();
        }

        private void InitializeDisplay()
        {
            txtStudentName.Text = $"Chi Tiết Tiến Trình - {_student.StudentName}";
            txtStudentInfo.Text = $"Mã SV: {_student.StudentCode} | Email: {_student.Email} | Trạng thái: {_student.Status}";
            
            // Update summary from existing data
            txtTotalCourses.Text = _student.TotalCourses.ToString();
            txtCompletedCourses.Text = _student.CompletedCourses.ToString();
            txtInProgressCourses.Text = (_student.TotalCourses - _student.CompletedCourses).ToString();
            txtOverallProgress.Text = _student.OverallProgressText;
        }

        private async Task LoadProgressDataAsync()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tải chi tiết tiến trình...";

                var progressList = await CourseProgressService.CalculateAllProgressAsync(_student.StudentId);
                
                _courseProgressList.Clear();
                foreach (var progress in progressList)
                {
                    var displayItem = new CourseProgressDisplay
                    {
                        CourseId = progress.CourseId,
                        CourseName = progress.CourseName,
                        InstructorName = progress.InstructorName,
                        TotalAssessments = progress.TotalAssessments,
                        CompletedAssessments = progress.CompletedAssessments,
                        ProgressPercentage = progress.ProgressPercentage,
                        ProgressPercentageText = $"{progress.ProgressPercentage:F1}%",
                        IsCompleted = progress.IsCompleted,
                        StatusText = progress.IsCompleted ? "Đã hoàn thành" : 
                                   progress.ProgressPercentage > 0 ? "Đang học" : "Chưa bắt đầu",
                        CompletionDate = progress.CompletionDate,
                        CompletionDateText = progress.CompletionDate?.ToString("dd/MM/yyyy") ?? "N/A"
                    };
                    
                    _courseProgressList.Add(displayItem);
                }

                // Update actual summary statistics
                var totalCourses = _courseProgressList.Count;
                var completedCourses = _courseProgressList.Count(p => p.IsCompleted);
                var inProgressCourses = _courseProgressList.Count(p => !p.IsCompleted && p.ProgressPercentage > 0);
                
                txtTotalCourses.Text = totalCourses.ToString();
                txtCompletedCourses.Text = completedCourses.ToString();
                txtInProgressCourses.Text = inProgressCourses.ToString();

                txtStatus.Text = "Đã tải dữ liệu thành công";
                txtLastUpdated.Text = $"Cập nhật: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi: {ex.Message}";
                MessageBox.Show($"Không thể tải chi tiết tiến trình: {ex.Message}", "Lỗi", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    // Display model for course progress in DataGrid
    public class CourseProgressDisplay
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int TotalAssessments { get; set; }
        public int CompletedAssessments { get; set; }
        public double ProgressPercentage { get; set; }
        public string ProgressPercentageText { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTime? CompletionDate { get; set; }
        public string CompletionDateText { get; set; } = string.Empty;
    }
}