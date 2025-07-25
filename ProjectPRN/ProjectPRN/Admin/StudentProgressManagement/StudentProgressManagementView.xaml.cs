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
using DataAccessObjects;
using ProjectPRN.Utils;

namespace ProjectPRN.Admin.StudentProgressManagement
{
    public partial class StudentProgressManagementView : UserControl, INotifyPropertyChanged
    {
        private readonly IStudentDAO _studentDAO;
        private List<StudentProgressSummary> _allStudents = new List<StudentProgressSummary>();
        private ObservableCollection<StudentProgressSummary> _filteredStudents = new ObservableCollection<StudentProgressSummary>();

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

        public StudentProgressManagementView()
        {
            InitializeComponent();
            DataContext = this;
            
            _studentDAO = new StudentDAO();
            dgStudents.ItemsSource = _filteredStudents;
            
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tải danh sách học sinh...";

                var students = await _studentDAO.GetAllAsync();
                _allStudents.Clear();

                foreach (var student in students)
                {
                    var progressSummary = await CalculateStudentProgressSummary(student);
                    _allStudents.Add(progressSummary);
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

        private async Task<StudentProgressSummary> CalculateStudentProgressSummary(BusinessObjects.Models.Student student)
        {
            try
            {
                var overallProgress = await CourseProgressService.CalculateOverallProgressAsync(student.StudentId);
                var allProgress = await CourseProgressService.CalculateAllProgressAsync(student.StudentId);
                
                return new StudentProgressSummary
                {
                    StudentId = student.StudentId,
                    StudentCode = student.StudentCode ?? "N/A",
                    StudentName = student.StudentName ?? "N/A",
                    Email = student.Email ?? "N/A",
                    Status = student.Status ?? "N/A",
                    TotalCourses = allProgress.Count,
                    CompletedCourses = allProgress.Count(p => p.IsCompleted),
                    OverallProgress = overallProgress,
                    OverallProgressText = $"{overallProgress:F1}%"
                };
            }
            catch (Exception)
            {
                return new StudentProgressSummary
                {
                    StudentId = student.StudentId,
                    StudentCode = student.StudentCode ?? "N/A",
                    StudentName = student.StudentName ?? "N/A",
                    Email = student.Email ?? "N/A",
                    Status = student.Status ?? "N/A",
                    TotalCourses = 0,
                    CompletedCourses = 0,
                    OverallProgress = 0,
                    OverallProgressText = "0%"
                };
            }
        }

        private void ApplyFilter()
        {
            var searchText = txtSearch?.Text?.ToLower() ?? "";
            
            var filtered = _allStudents.Where(s => 
                string.IsNullOrEmpty(searchText) ||
                s.StudentName.ToLower().Contains(searchText) ||
                s.StudentCode.ToLower().Contains(searchText) ||
                s.Email.ToLower().Contains(searchText)
            ).OrderBy(s => s.StudentName).ToList();

            _filteredStudents.Clear();
            foreach (var student in filtered)
            {
                _filteredStudents.Add(student);
            }

            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            txtRecordCount.Text = $"Tổng số học sinh: {_filteredStudents.Count}/{_allStudents.Count}";
            txtLastUpdated.Text = $"Cập nhật: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }

        #region Event Handlers
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng xuất Excel sẽ được triển khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int studentId)
            {
                var student = _filteredStudents.FirstOrDefault(s => s.StudentId == studentId);
                if (student != null)
                {
                    var detailWindow = new StudentProgressDetailView(student);
                    detailWindow.ShowDialog();
                }
            }
        }

        private void DgStudents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgStudents.SelectedItem is StudentProgressSummary selectedStudent)
            {
                var detailWindow = new StudentProgressDetailView(selectedStudent);
                detailWindow.ShowDialog();
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

    // Data Model for Student Progress Summary
    public class StudentProgressSummary
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public double OverallProgress { get; set; }
        public string OverallProgressText { get; set; } = string.Empty;
    }
}