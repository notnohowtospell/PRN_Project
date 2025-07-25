using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using ProjectPRN.Utils;

namespace ProjectPRN.Admin.CourseRating
{
    public partial class CourseFeedbackListView : UserControl, INotifyPropertyChanged
    {
        private readonly int _courseId;
        private List<FeedbackViewModel> _feedbacks = new List<FeedbackViewModel>();

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

        public CourseFeedbackListView(int courseId)
        {
            InitializeComponent();
            _courseId = courseId;
            DataContext = this;
            
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Dang tai danh sach danh gia...";

                using var context = new ApplicationDbContext();
                
                // Load course info
                var course = await context.LifeSkillCourses
                    .FirstOrDefaultAsync(c => c.CourseId == _courseId);

                if (course == null)
                {
                    txtStatus.Text = "Khong tim thay khoa hoc";
                    return;
                }

                // Update course info in header
                txtCourseTitle.Text = $"Danh Gia Khoa Hoc: {course.CourseName}";
                txtCourseInfo.Text = $"Ma khoa hoc: {course.CourseId}";

                // Load all feedbacks for this course
                var feedbacks = await context.Feedbacks
                    .Include(f => f.Student)
                    .Where(f => f.CourseId == _courseId)
                    .OrderByDescending(f => f.FeedbackDate)
                    .ToListAsync();

                _feedbacks.Clear();

                foreach (var feedback in feedbacks)
                {
                    _feedbacks.Add(new FeedbackViewModel
                    {
                        FeedbackId = feedback.FeedbackId,
                        StudentName = feedback.Student?.StudentName ?? "Khong ro ten",
                        StudentId = feedback.StudentId,
                        Rating = feedback.Rating,
                        Comment = feedback.Comment ?? "",
                        FeedbackDate = feedback.FeedbackDate,
                        FeedbackDateText = feedback.FeedbackDate.ToString("dd/MM/yyyy HH:mm"),
                        RatingColor = GetRatingColor(feedback.Rating)
                    });
                }

                UpdateUI();
                txtStatus.Text = $"Da tai {_feedbacks.Count} danh gia";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Loi: {ex.Message}";
                MessageBox.Show($"Khong the tai du lieu: {ex.Message}", "Loi", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateUI()
        {
            // Update summary stats
            txtTotalFeedbacks.Text = _feedbacks.Count.ToString();
            
            if (_feedbacks.Any())
            {
                var averageRating = _feedbacks.Average(f => f.Rating);
                txtAverageRating.Text = $"{averageRating:F1}/5";
                //txtStarDisplay.Text = GenerateStarDisplay(averageRating);
            }
            else
            {
                txtAverageRating.Text = "0.0/5";
            }

            UpdateFeedbackList();
            UpdateStatusBar();
        }

        private void UpdateFeedbackList()
        {
            spFeedbackList.Children.Clear();

            if (!_feedbacks.Any())
            {
                var noDataText = new TextBlock
                {
                    Text = "Khoa hoc nay chua co danh gia nao.",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontStyle = FontStyles.Italic,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    Margin = new Thickness(50)
                };
                spFeedbackList.Children.Add(noDataText);
                return;
            }

            foreach (var feedback in _feedbacks)
            {
                var feedbackCard = CreateFeedbackCard(feedback);
                spFeedbackList.Children.Add(feedbackCard);
            }
        }

        private Border CreateFeedbackCard(FeedbackViewModel feedback)
        {
            var card = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                BorderBrush = System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(15)
            };

            var mainStack = new StackPanel();

            // Header with student name and date
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var studentText = new TextBlock
            {
                Text = feedback.StudentName,
                FontWeight = FontWeights.SemiBold,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(studentText, 0);

            var dateText = new TextBlock
            {
                Text = feedback.FeedbackDateText,
                Foreground = System.Windows.Media.Brushes.Gray,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(dateText, 1);

            headerGrid.Children.Add(studentText);
            headerGrid.Children.Add(dateText);

            // Rating display
            var ratingStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 10)
            };

            var ratingText = new TextBlock
            {
                Text = $"{feedback.Rating}/5",
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var starsText = new TextBlock
            {
                Text = feedback.StarDisplay,
                FontSize = 16,
                Foreground = System.Windows.Media.Brushes.Gold,
                VerticalAlignment = VerticalAlignment.Center
            };

            ratingStack.Children.Add(ratingText);
            ratingStack.Children.Add(starsText);

            // Comment
            var commentText = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(feedback.Comment) ? "[Khong co binh luan]" : $"\"{feedback.Comment}\"",
                TextWrapping = TextWrapping.Wrap,
                FontStyle = string.IsNullOrWhiteSpace(feedback.Comment) ? FontStyles.Italic : FontStyles.Normal,
                Foreground = string.IsNullOrWhiteSpace(feedback.Comment) ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.Black,
                Margin = new Thickness(0, 5, 0, 0)
            };

            mainStack.Children.Add(headerGrid);
            mainStack.Children.Add(ratingStack);
            mainStack.Children.Add(commentText);

            card.Child = mainStack;
            return card;
        }

        private void UpdateStatusBar()
        {
            txtRecordCount.Text = $"Tong so danh gia: {_feedbacks.Count}";
            txtLastUpdated.Text = $"Cap nhat: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }

        //private string GenerateStarDisplay(double rating)
        //{
        //    if (rating == 0) return "☆☆☆☆☆";
            
        //    int fullStars = (int)Math.Floor(rating);
        //    bool hasHalfStar = (rating - fullStars) >= 0.5;
            
        //    string stars = new string('★', fullStars);
        //    if (hasHalfStar && fullStars < 5)
        //    {
        //        stars += "⭐";
        //        fullStars++;
        //    }
        //    stars += new string('☆', 5 - fullStars);
            
        //    return stars;
        //}

        private string GetRatingColor(int rating)
        {
            return rating switch
            {
                5 => "#4CAF50",  // Green
                4 => "#8BC34A",  // Light Green
                3 => "#FF9800",  // Orange
                2 => "#FF5722",  // Deep Orange
                1 => "#F44336",  // Red
                _ => "#9E9E9E"   // Gray
            };
        }

        #region Event Handlers
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
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

    // ViewModel for feedback display
    public class FeedbackViewModel
    {
        public int FeedbackId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime FeedbackDate { get; set; }
        public string FeedbackDateText { get; set; } = string.Empty;
        public string StarDisplay { get; set; } = string.Empty;
        public string RatingColor { get; set; } = string.Empty;
    }
}