using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using ProjectPRN.Student.Courses;
using ProjectPRN.Utils;

namespace ProjectPRN.Student
{
    public partial class StudentMainWindow : Window, INotifyPropertyChanged
    {
        private readonly BusinessObjects.Models.Student _currentStudent;
        private DispatcherTimer _timer;

        public StudentMainWindow(BusinessObjects.Models.Student student)
        {
            InitializeComponent();
            _currentStudent = student ?? throw new ArgumentNullException(nameof(student));
            
            DataContext = this;
            InitializeWindow();
            LoadDashboardData();
            StartTimer();
        }

        #region Properties
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
        #endregion

        #region Initialization
        private void InitializeWindow()
        {
            txtWelcome.Text = $"Xin chào, {_currentStudent.StudentName}";
            txtStatus.Text = "Đã tải thành công";
        }

        private void LoadDashboardData()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tải dữ liệu...";

                // Load dashboard statistics
                LoadStudentStatistics();
                LoadRecentActivities();

                txtStatus.Text = "Sẵn sàng";
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

        private async void LoadStudentStatistics()
        {
            try
            {
                using var context = new ApplicationDbContext();
                
                // Load basic counts
                txtEnrolledCourses.Text = _currentStudent.Enrollments?.Count.ToString() ?? "0";
                txtCompletedAssessments.Text = _currentStudent.AssessmentResults?.Count.ToString() ?? "0";
                txtCertificates.Text = _currentStudent.Certificates?.Count.ToString() ?? "0";
                
                // Calculate overall progress using the new service
                var overallProgress = await CourseProgressService.CalculateOverallProgressAsync(context, _currentStudent.StudentId);
                txtOverallProgress.Text = $"{overallProgress:F1}%";
            }
            catch (Exception ex)
            {
                // Fallback to mock data if calculation fails
                txtEnrolledCourses.Text = _currentStudent.Enrollments?.Count.ToString() ?? "0";
                txtCompletedAssessments.Text = _currentStudent.AssessmentResults?.Count.ToString() ?? "0";
                txtCertificates.Text = _currentStudent.Certificates?.Count.ToString() ?? "0";
                txtOverallProgress.Text = "0%";
                
                System.Diagnostics.Debug.WriteLine($"Error calculating progress: {ex.Message}");
            }
        }

        private void LoadRecentActivities()
        {
            RecentActivities.Children.Clear();
            
            // Mock recent activities - replace with actual data
            var activities = new[]
            {
                "Chức năng chưa triển khai ^^"
            };

            foreach (var activity in activities)
            {
                var textBlock = new TextBlock
                {
                    Text = $"• {activity}",
                    Margin = new Thickness(0, 2, 0, 2)
                };
                RecentActivities.Children.Add(textBlock);
            }
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            txtDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
        #endregion

        #region Navigation Event Handlers
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
            HighlightSelectedButton(sender as Button);
        }

        private void BtnCourses_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Đang mở danh sách khóa học...";
            HighlightSelectedButton(sender as Button);

            // Navigate to courses view
            // var coursesWindow = new CourseSearchWindow();
            // coursesWindow.Show();

            var coursesWindow = new StudentCourseView();
            coursesWindow.Show();

            
            //MessageBox.Show("Chức năng xem khóa học sẽ được triển khai", "Thông báo", 
            //               MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMyEnrollments_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Đang mở khóa học đã đăng ký...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Chức năng xem khóa học đã đăng ký sẽ được triển khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAssessments_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Đang mở danh sách bài kiểm tra...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Chức năng bài kiểm tra sẽ được triển khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMaterials_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Đang mở tài liệu học tập...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Chức năng tài liệu học tập sẽ được triển khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSchedule_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Đang mở lịch học...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Chức năng lịch học sẽ được triển khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCertificates_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Đang mở danh sách chứng chỉ...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Chức năng chứng chỉ sẽ được triển khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPayments_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Đang mở thông tin thanh toán...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Chức năng thanh toán sẽ được triển khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnFeedback_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Đang mở form phản hồi...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Chức năng phản hồi sẽ được triển khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnProgress_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Đang mở báo cáo tiến độ...";
            HighlightSelectedButton(sender as Button);
            
            try
            {
                var progressWindow = new ProjectPRN.Student.Progress.StudentProgressView(
                    _currentStudent.StudentId, 
                    _currentStudent.StudentName);
                progressWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở trang tiến độ: {ex.Message}", "Lỗi", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Đang mở thông tin cá nhân...";
            
            MessageBox.Show("Chức năng cập nhật thông tin cá nhân sẽ được triển khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", 
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                txtStatus.Text = "Đang đăng xuất...";
                
                // Navigate back to login
                var loginWindow = new Login();
                loginWindow.Show();
                
                this.Close();
            }
        }
        #endregion

        #region Helper Methods
        private void ShowDashboard()
        {
            DashboardContent.Visibility = Visibility.Visible;
            DynamicContent.Visibility = Visibility.Collapsed;
            LoadDashboardData();
        }

        private void HighlightSelectedButton(Button selectedButton)
        {
            // Reset all navigation buttons
            btnDashboard.Background = null;
            
            // Find all buttons in the navigation menu and reset their backgrounds
            // This is a simplified approach - in a real app you might use styles or data binding
            
            // Highlight the selected button
            if (selectedButton != null && selectedButton != btnDashboard)
            {
                selectedButton.Background = App.Current.Resources["PrimaryHueLightBrush"] as System.Windows.Media.Brush;
            }
            else if (selectedButton == btnDashboard)
            {
                btnDashboard.Background = App.Current.Resources["PrimaryHueLightBrush"] as System.Windows.Media.Brush;
            }
        }
        #endregion

        #region Window Events
        protected override void OnClosing(CancelEventArgs e)
        {
            _timer?.Stop();
            base.OnClosing(e);
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
}