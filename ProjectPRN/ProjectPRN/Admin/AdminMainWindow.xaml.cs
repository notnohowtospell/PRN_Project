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
using ProjectPRN.Admin.BackupRestore;
using ProjectPRN.Admin.CourseManagement;
using ProjectPRN.Admin.InstructorManagement;
using ProjectPRN.Search;

namespace ProjectPRN.Admin
{
    public partial class AdminMainWindow : Window, INotifyPropertyChanged
    {
        private readonly ILifeSkillCourseRepository _lifeSkillCourseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IStudentDAO _studentDAO;
        private DispatcherTimer _timer;

        public AdminMainWindow()
        {
            InitializeComponent();
            
            // Initialize repositories
            _lifeSkillCourseRepository = new LifeSkillCourseRepository(new LifeSkillCourseDAO());
            _instructorRepository = new InstructorRepository(new InstructorDAO());
            _studentDAO = new StudentDAO();
            
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
            txtWelcome.Text = "Welcome, Administrator";
            txtStatus.Text = "Successfully loaded";
        }

        private async void LoadDashboardData()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Loading dashboard data...";

                await LoadAdminStatistics();
                LoadRecentActivities();

                txtStatus.Text = "Ready";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Cannot load dashboard data: {ex.Message}", "Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAdminStatistics()
        {
            try
            {
                // Load system statistics
                var allCourses = await _lifeSkillCourseRepository.GetAllAsync();
                var allInstructors = await _instructorRepository.GetAllAsync();
                var allStudents = await _studentDAO.GetAllAsync();

                txtTotalCourses.Text = allCourses.Count().ToString();
                txtTotalUsers.Text = (allInstructors.Count() + allStudents.Count()).ToString();

                // Calculate active enrollments
                var activeEnrollments = allCourses.Sum(c => c.Enrollments?.Count ?? 0);
                txtActiveEnrollments.Text = activeEnrollments.ToString();

                // Calculate revenue (mock for now)
                var revenue = allCourses.Where(c => c.Price.HasValue).Sum(c => c.Price.Value);
                txtRevenue.Text = $"{revenue:C0}";
            }
            catch (Exception)
            {
                // Handle errors by showing default values
                txtTotalCourses.Text = "0";
                txtTotalUsers.Text = "0";
                txtActiveEnrollments.Text = "0";
                txtRevenue.Text = "$0";
            }
        }

        private void LoadRecentActivities()
        {
            RecentActivities.Children.Clear();
            
            // Mock recent activities - replace with actual data
            var activities = new[]
            {
                "System started successfully",
                "Database connection established",
                "Admin dashboard loaded"
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
            txtStatus.Text = "Opening account information...";
            MessageBox.Show("Account management feature will be implemented", "Notice", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", 
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                txtStatus.Text = "Logging out...";
                
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

        private void BtnUserManagement_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Opening user management...";
            HighlightSelectedButton(sender as Button);
            
            try
            {
                var studentSearchWindow = new StudentSearchView();
                studentSearchWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening user management: {ex.Message}", "Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCourseManagement_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Opening course management...";
            HighlightSelectedButton(sender as Button);
            
            try
            {
                var courseManagementWindow = new CourseManagementView(_lifeSkillCourseRepository, _instructorRepository);
                courseManagementWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening course management: {ex.Message}", "Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnInstructorManagement_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Opening instructor management...";
            HighlightSelectedButton(sender as Button);
            
            try
            {
                var instructorManagementWindow = new InstructorManagementView();
                instructorManagementWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening instructor management: {ex.Message}", "Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPaymentManagement_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Opening payment management...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("Payment management feature will be implemented", "Notice", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSystemReports_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Opening system reports...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("System reports feature will be implemented", "Notice", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBackupRestore_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Opening backup & restore...";
            HighlightSelectedButton(sender as Button);
            
            try
            {
                var backupRestoreWindow = new BackupRestoreView();
                backupRestoreWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening backup & restore: {ex.Message}", "Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSystemSettings_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Opening system settings...";
            HighlightSelectedButton(sender as Button);
            
            MessageBox.Show("System settings feature will be implemented", "Notice", 
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