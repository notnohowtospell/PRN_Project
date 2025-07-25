using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using ProjectPRN.Utils;

namespace ProjectPRN.Student.Progress
{
    public partial class StudentProgressView : Window, INotifyPropertyChanged
    {
        private readonly ApplicationDbContext _context;
        private readonly int _studentId;
        private readonly string _studentName;

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

        public StudentProgressView(int studentId, string studentName)
        {
            InitializeComponent();
            DataContext = this;
            
            _context = new ApplicationDbContext();
            _studentId = SessionManager.GetCurrentUserId();
            _studentName = studentName;

            txtStudentName.Text = $"Sinh viên: {_studentName}";

            _ = LoadProgressDataAsync();
        }

        private async Task LoadProgressDataAsync()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tải dữ liệu tiến độ...";

                var progressList = await CourseProgressService.CalculateAllProgressAsync(_studentId);
                var overallProgress = await CourseProgressService.CalculateOverallProgressAsync(_studentId);

                UpdateSummaryStatistics(progressList, overallProgress);
                DisplayCourseProgress(progressList);

                txtStatus.Text = "Đã tải dữ liệu thành công";
                txtLastUpdated.Text = $"Cập nhật lúc: {DateTime.Now:HH:mm:ss dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi: {ex.Message}";
                MessageBox.Show($"Không thể tải dữ liệu tiến độ: {ex.Message}", "Lỗi", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateSummaryStatistics(List<CourseProgressInfo> progressList, double overallProgress)
        {
            var totalCourses = _context.Enrollments.Where(e => e.StudentId == _studentId).Count();
            var completedCourses = progressList.Count(p => p.IsCompleted);
            var inProgressCourses = progressList.Count(p => !p.IsCompleted && p.ProgressPercentage > 0);

            txtTotalCourses.Text = totalCourses.ToString();
            txtCompletedCourses.Text = completedCourses.ToString();
            txtInProgressCourses.Text = inProgressCourses.ToString();
            txtOverallProgress.Text = $"{overallProgress:F1}%";
        }

        private void DisplayCourseProgress(List<CourseProgressInfo> progressList)
        {
            CourseProgressContainer.Children.Clear();



            if (!progressList.Any())
            {
                txtNoCourses.Visibility = Visibility.Visible;
                CourseProgressContainer.Children.Add(txtNoCourses);
                return;
            }

            foreach (var courseProgress in progressList)
            {
                var courseCard = CreateCourseProgressCard(courseProgress);
                CourseProgressContainer.Children.Add(courseCard);
            }
        }

        private Card CreateCourseProgressCard(CourseProgressInfo courseProgress)
        {
            var card = new Card
            {
                Style = (Style)FindResource("ProgressCardStyle"),
                Margin = new Thickness(0, 8, 0, 8)
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Course Header
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var courseInfoStack = new StackPanel();
            
            var courseNameText = new TextBlock
            {
                Text = courseProgress.CourseName,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
            };
            courseInfoStack.Children.Add(courseNameText);

            var instructorText = new TextBlock
            {
                Text = $"Giảng viên: {courseProgress.InstructorName}",
                Style = (Style)FindResource("MaterialDesignBody2TextBlock"),
                Margin = new Thickness(0, 2, 0, 0)
            };
            courseInfoStack.Children.Add(instructorText);

            headerGrid.Children.Add(courseInfoStack);

            // Progress Badge
            var progressBadge = new Border
            {
                Background = GetProgressBrush(courseProgress.ProgressPercentage),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12, 8, 12, 8),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            var progressText = new TextBlock
            {
                Text = $"{courseProgress.ProgressPercentage:F1}%",
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14
            };
            progressBadge.Child = progressText;
            
            Grid.SetColumn(progressBadge, 1);
            headerGrid.Children.Add(progressBadge);

            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);




            // Assessment Details
            var assessmentStack = new StackPanel
            {
                Margin = new Thickness(0, 8, 0, 0)
            };

            var summaryText = new TextBlock
            {
                Text = $"Bài kiểm tra: {courseProgress.CompletedAssessments}/{courseProgress.TotalAssessments} hoàn thành",
                Style = (Style)FindResource("ProgressTextStyle"),
                FontWeight = FontWeights.Medium
            };
            assessmentStack.Children.Add(summaryText);

            // Assessment details (expandable)
            if (courseProgress.AssessmentProgress.Any())
            {
                var expander = new Expander
                {
                    Header = "Chi tiết bài kiểm tra",
                    Margin = new Thickness(0, 8, 0, 0)
                };

                var assessmentList = new StackPanel();

                foreach (var assessment in courseProgress.AssessmentProgress)
                {
                    var assessmentItem = CreateAssessmentItem(assessment);
                    assessmentList.Children.Add(assessmentItem);
                }

                expander.Content = assessmentList;
                assessmentStack.Children.Add(expander);
            }

            // Completion Status
            if (courseProgress.IsCompleted && courseProgress.CompletionDate.HasValue)
            {
                var completionText = new TextBlock
                {
                    Text = $"✅ Hoàn thành vào: {courseProgress.CompletionDate.Value:dd/MM/yyyy}",
                    Style = (Style)FindResource("ProgressTextStyle"),
                    Foreground = new SolidColorBrush(Colors.Green),
                    FontWeight = FontWeights.Medium,
                    Margin = new Thickness(0, 8, 0, 0)
                };
                assessmentStack.Children.Add(completionText);
            }

            Grid.SetRow(assessmentStack, 2);
            mainGrid.Children.Add(assessmentStack);

            card.Content = mainGrid;
            return card;
        }

        private Border CreateAssessmentItem(AssessmentProgressInfo assessment)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 4, 0, 4)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var infoStack = new StackPanel();

            var nameText = new TextBlock
            {
                Text = assessment.AssessmentName,
                FontWeight = FontWeights.Medium,
                FontSize = 14
            };
            infoStack.Children.Add(nameText);

            var detailsText = new TextBlock
            {
                Text = $"Hạn nộp: {assessment.DueDate:dd/MM/yyyy} • Loại: {assessment.AssessmentType}",
                Style = (Style)FindResource("MaterialDesignCaptionTextBlock"),
                Margin = new Thickness(0, 2, 0, 0)
            };
            infoStack.Children.Add(detailsText);

            if (assessment.IsCompleted && assessment.Score.HasValue)
            {
                var scoreText = new TextBlock
                {
                    Text = $"Điểm: {assessment.Score}/{assessment.MaxScore} • Nộp: {assessment.SubmissionDate:dd/MM/yyyy HH:mm}",
                    Style = (Style)FindResource("MaterialDesignCaptionTextBlock"),
                    Foreground = new SolidColorBrush(Colors.Green),
                    Margin = new Thickness(0, 2, 0, 0)
                };
                infoStack.Children.Add(scoreText);
            }

            grid.Children.Add(infoStack);

            // Status Icon
            var statusIcon = new PackIcon
            {
                Kind = assessment.IsCompleted ? PackIconKind.CheckCircle : PackIconKind.Clock,
                Foreground = assessment.IsCompleted ? 
                    new SolidColorBrush(Colors.Green) : 
                    new SolidColorBrush(Colors.Orange),
                Width = 20,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(statusIcon, 1);
            grid.Children.Add(statusIcon);

            border.Child = grid;
            return border;
        }

        private Brush GetProgressBrush(double progress)
        {
            if (progress >= 100)
                return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
            if (progress >= 75)
                return new SolidColorBrush(Color.FromRgb(139, 195, 74)); // Light Green
            if (progress >= 50)
                return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
            if (progress >= 25)
                return new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Amber
            
            return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadProgressDataAsync();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}