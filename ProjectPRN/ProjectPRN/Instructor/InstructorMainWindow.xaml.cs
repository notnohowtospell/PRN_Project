using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using Repositories.Interfaces;
using Repositories;
using DataAccessObjects;

namespace ProjectPRN.InstructorModule
{
    public partial class InstructorMainWindow : Window, INotifyPropertyChanged
    {
        private readonly BusinessObjects.Models.Instructor _currentInstructor;
        private readonly ILifeSkillCourseRepository _lifeSkillCourseRepository;
        private DispatcherTimer _timer;

        public InstructorMainWindow(BusinessObjects.Models.Instructor instructor)
        {
            InitializeComponent();
            _currentInstructor = instructor ?? throw new ArgumentNullException(nameof(instructor));
            
            // Initialize repositories
            _lifeSkillCourseRepository = new LifeSkillCourseRepository(new LifeSkillCourseDAO());
            
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
            txtWelcome.Text = $"Welcome, {_currentInstructor.InstructorName}";
            txtStatus.Text = "Successfully loaded";
        }

        private void LoadDashboardData()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "?ang t?i d? li?u dashboard...";

                LoadInstructorStatistics();
                LoadRecentActivities();

                txtStatus.Text = "S?n sàng";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"L?i: {ex.Message}";
                MessageBox.Show($"Không th? t?i d? li?u dashboard: {ex.Message}", "L?i", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadInstructorStatistics()
        {
            try
            {
                // Load statistics for this instructor
                var instructorCourses = _lifeSkillCourseRepository.GetAll()
                    .Where(c => c.InstructorId == _currentInstructor.InstructorId).ToList();

                txtMyCourses.Text = instructorCourses.Count.ToString();
                txtActiveCourses.Text = instructorCourses.Count(c => c.Status == "M? ??ng ký").ToString();
                
                // Calculate total students across all courses
                var totalStudents = instructorCourses.Sum(c => c.Enrollments?.Count ?? 0);
                txtTotalStudents.Text = totalStudents.ToString();
                
                // Mock pending assessments
                txtPendingAssessments.Text = "0";
            }
            catch (Exception)
            {
                // Handle errors by showing default values
                txtMyCourses.Text = "0";
                txtActiveCourses.Text = "0";
                txtTotalStudents.Text = "0";
                txtPendingAssessments.Text = "0";
            }
        }

        private void LoadRecentActivities()
        {
            RecentActivities.Children.Clear();
            
            // Mock recent activities - replace with actual data
            var activities = new[]
            {
                "??ng nh?p h? th?ng thành công",
                "C?p nh?t thông tin khóa h?c",
                "Xem danh sách sinh viên"
            };

            foreach (var activity in activities)
            {
                var textBlock = new TextBlock
                {
                    Text = $"• {activity} - {DateTime.Now:HH:mm}",
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

        private void Timer_Tick(object? sender, EventArgs e)
        {
            txtDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
        #endregion

        #region Header Event Handlers
        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "?ang m? thông tin tài kho?n...";
            MessageBox.Show("Ch?c n?ng qu?n lý tài kho?n s? ???c tri?n khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("B?n có ch?c ch?n mu?n ??ng xu?t?", "Xác nh?n ??ng xu?t", 
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                txtStatus.Text = "?ang ??ng xu?t...";
                
                // Navigate back to login
                var loginWindow = new Login();
                loginWindow.Show();
                
                this.Close();
            }
        }
        #endregion

        #region Navigation Event Handlers
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
            HighlightSelectedButton(sender as Button);
        }

        private void BtnMyCourses_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "?ang m? khóa h?c c?a tôi...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Ch?c n?ng qu?n lý khóa h?c c?a gi?ng viên s? ???c tri?n khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnStudents_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "?ang m? danh sách sinh viên...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Ch?c n?ng xem sinh viên s? ???c tri?n khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAssessments_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "?ang m? bài ki?m tra...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Ch?c n?ng qu?n lý bài ki?m tra s? ???c tri?n khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMaterials_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "?ang m? tài li?u gi?ng d?y...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Ch?c n?ng qu?n lý tài li?u gi?ng d?y s? ???c tri?n khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSchedule_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "?ang m? l?ch gi?ng d?y...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Ch?c n?ng xem l?ch gi?ng d?y s? ???c tri?n khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnFeedback_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "?ang m? ph?n h?i sinh viên...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Ch?c n?ng xem ph?n h?i sinh viên s? ???c tri?n khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "?ang m? báo cáo...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Ch?c n?ng báo cáo s? ???c tri?n khai", "Thông báo", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region Helper Methods
        private void ShowDashboard()
        {
            DashboardContent.Visibility = Visibility.Visible;
            DynamicContent.Visibility = Visibility.Collapsed;
            LoadDashboardData();
        }

        private void HighlightSelectedButton(Button? selectedButton)
        {
            // Reset all navigation buttons
            btnDashboard.Background = null;
            
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
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}