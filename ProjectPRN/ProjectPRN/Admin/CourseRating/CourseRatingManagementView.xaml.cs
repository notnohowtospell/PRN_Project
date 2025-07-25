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

namespace ProjectPRN.Admin.CourseRating
{
    public partial class CourseRatingManagementView : UserControl, INotifyPropertyChanged
    {
        private List<CourseRatingViewModel> _allCourses = new List<CourseRatingViewModel>();
        private ObservableCollection<CourseRatingViewModel> _filteredCourses = new ObservableCollection<CourseRatingViewModel>();

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

        public CourseRatingManagementView()
        {
            InitializeComponent();
            DataContext = this;
            
            dgCourseRatings.ItemsSource = _filteredCourses;
            
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Dang tai danh sach khoa hoc va danh gia...";

                using var context = new ApplicationDbContext();
                
                // Load all courses with their ratings
                var courses = await context.LifeSkillCourses
                    //.Include(c => c.Instructor)
                    .Include(c => c.Enrollments)
                    .Include(c => c.Feedbacks)
                    .ToListAsync();

                // Load instructors for filter
                //await LoadInstructorsAsync(context);

                _allCourses.Clear();

                foreach (var course in courses)
                {
                    try
                    {
                        var feedbacks = course.Feedbacks?.ToList() ?? new List<BusinessObjects.Models.Feedback>();
                        var enrollments = course.Enrollments?.ToList() ?? new List<Enrollment>();

                        // Calculate average rating with safety check
                        double averageRating = 0;
                        if (feedbacks.Any())
                        {
                            // Additional safety check for valid ratings
                            var validRatings = feedbacks.Where(f => f.Rating >= 1 && f.Rating <= 5).ToList();
                            if (validRatings.Any())
                            {
                                averageRating = validRatings.Average(f => f.Rating);
                            }
                        }

                        var courseRating = new CourseRatingViewModel
                        {
                            CourseId = course.CourseId,
                            CourseName = course.CourseName ?? "Ten khoa hoc khong ro",
                            InstructorName = course.Instructor?.InstructorName ?? "Chua co giang vien",
                            InstructorId = course.InstructorId,
                            TotalStudents = enrollments.Count,
                            TotalRatings = feedbacks.Count,
                            AverageRating = averageRating,
                            AverageRatingText = averageRating > 0 ? $"{averageRating:F1}/5" : "0/5",
                            StarDisplay = GenerateStarDisplay(averageRating),
                            Status = course.Status ?? "Khong ro",
                            CreatedDate = DateTime.Now, // You might want to add this field to LifeSkillCourse
                            CreatedDateText = DateTime.Now.ToString("dd/MM/yyyy"),
                            HasRatings = feedbacks.Any() && averageRating > 0,
                            RatingCategory = GetRatingCategory(averageRating, feedbacks.Any() && averageRating > 0)
                        };

                        _allCourses.Add(courseRating);
                    }
                    catch (Exception ex)
                    {
                        // Log error for individual course but continue processing others
                        System.Diagnostics.Debug.WriteLine($"Error processing course {course.CourseId}: {ex.Message}");
                        
                        // Add course with default values
                        var safeCourseRating = new CourseRatingViewModel
                        {
                            CourseId = course.CourseId,
                            CourseName = course.CourseName ?? "Ten khoa hoc khong ro",
                            InstructorName = course.Instructor?.InstructorName ?? "Chua co giang vien",
                            InstructorId = course.InstructorId,
                            TotalStudents = 0,
                            TotalRatings = 0,
                            AverageRating = 0,
                            AverageRatingText = "0/5",
                            StarDisplay = "☆☆☆☆☆",
                            Status = course.Status ?? "Khong ro",
                            CreatedDate = DateTime.Now,
                            CreatedDateText = DateTime.Now.ToString("dd/MM/yyyy"),
                            HasRatings = false,
                            RatingCategory = "NoRating"
                        };
                        
                        _allCourses.Add(safeCourseRating);
                    }
                }

                ApplyFilter();
                UpdateStatusBar();
                txtStatus.Text = "Da tai du lieu thanh cong";
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



        private string GenerateStarDisplay(double rating)
        {
            if (rating == 0) return "?????";
            
            int fullStars = (int)Math.Floor(rating);
            bool hasHalfStar = (rating - fullStars) >= 0.5;
            
            string stars = new string('?', fullStars);
            if (hasHalfStar && fullStars < 5)
            {
                stars += "?";
                fullStars++;
            }
            stars += new string('?', 5 - fullStars);
            
            return stars;
        }

        private string GetRatingCategory(double rating, bool hasRatings)
        {
            if (!hasRatings) return "NoRating";
            if (rating <= 0) return "NoRating"; // Additional safety check
            if (rating <= 2) return "Low";
            if (rating <= 3) return "Medium";
            return "High"; // 4-5 stars
        }

        private void ApplyFilter()
        {
            try
            {
                var searchText = txtSearch?.Text?.ToLower() ?? "";
                var ratingFilter = "All"; // Default to "All" since we don't have the ComboBox
                var instructorFilter = "-1";
                
                var filtered = _allCourses.Where(c => 
                    // Search filter
                    (string.IsNullOrEmpty(searchText) ||
                     (c.CourseName?.ToLower().Contains(searchText) == true) ||
                     (c.InstructorName?.ToLower().Contains(searchText) == true)) &&
                    
                    // Rating filter
                    (ratingFilter == "All" || c.RatingCategory == ratingFilter) &&
                    
                    // Instructor filter
                    (instructorFilter == "-1" || c.InstructorId.ToString() == instructorFilter)
                ).OrderByDescending(c => c.AverageRating).ThenBy(c => c.CourseName).ToList();

                _filteredCourses.Clear();
                foreach (var course in filtered)
                {
                    _filteredCourses.Add(course);
                }

                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying filter: {ex.Message}");
                
                // Fallback: show all courses
                _filteredCourses.Clear();
                foreach (var course in _allCourses)
                {
                    _filteredCourses.Add(course);
                }
                
                UpdateStatusBar();
            }
        }

        private void UpdateStatusBar()
        {
            var totalCourses = _filteredCourses.Count;
            var coursesWithRatings = _filteredCourses.Count(c => c.HasRatings);
            var coursesWithoutRatings = totalCourses - coursesWithRatings;
            
            txtRecordCount.Text = $"Tong so khoa hoc: {totalCourses} " +
                                 $"(Co danh gia: {coursesWithRatings}, Chua co: {coursesWithoutRatings})";

            // Calculate overall system average - FIX: Check if any courses have ratings before calling Average
            var coursesWithRatingsForAverage = _allCourses.Where(c => c.HasRatings).ToList();
            if (coursesWithRatingsForAverage.Any())
            {
                var overallAverage = coursesWithRatingsForAverage.Average(c => c.AverageRating);
                txtAverageStats.Text = $"Danh gia TB toan he thong: {overallAverage:F1}/5";
            }
            else
            {
                txtAverageStats.Text = "Danh gia TB toan he thong: 0.0/5";
            }
            
            txtLastUpdated.Text = $"Cap nhat: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }

        #region Event Handlers
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbRatingFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbInstructorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int courseId)
            {
                var course = _filteredCourses.FirstOrDefault(c => c.CourseId == courseId);
                if (course != null)
                {
                    ShowCourseDetailsDialog(course);
                }
            }
        }

        private void BtnViewFeedbacks_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int courseId)
            {
                var course = _filteredCourses.FirstOrDefault(c => c.CourseId == courseId);
                if (course != null)
                {
                    try
                    {
                        // Open new detailed feedback list window
                        var feedbackListWindow = new CourseFeedbackListWindow(courseId, course.CourseName);
                        feedbackListWindow.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Loi khi mo trang danh gia: {ex.Message}", "Loi",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DgCourseRatings_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCourseRatings.SelectedItem is CourseRatingViewModel selectedCourse)
            {
                ShowCourseDetailsDialog(selectedCourse);
            }
        }

        private void ShowCourseDetailsDialog(CourseRatingViewModel course)
        {
            var message = $"Thong tin chi tiet khoa hoc:\n\n" +
                         $"Ten khoa hoc: {course.CourseName}\n" +
                         $"Giang vien: {course.InstructorName}\n" +
                         $"So hoc vien: {course.TotalStudents}\n" +
                         $"So danh gia: {course.TotalRatings}\n" +
                         $"Danh gia trung binh: {course.AverageRatingText}\n" +
                         $"Trang thai: {course.Status}\n" +
                         $"Ngay tao: {course.CreatedDateText}";

            MessageBox.Show(message, "Chi Tiet Khoa Hoc", 
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ShowCourseFeedbacksDialog(CourseRatingViewModel course)
        {
            try
            {
                using var context = new ApplicationDbContext();
                var feedbacks = await context.Feedbacks
                    .Include(f => f.Student)
                    .Where(f => f.CourseId == course.CourseId)
                    .OrderByDescending(f => f.FeedbackDate)
                    .ToListAsync();

                if (!feedbacks.Any())
                {
                    MessageBox.Show("Khoa hoc nay chua co danh gia nao.", "Thong Bao",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var feedbackDetails = $"Danh sach danh gia cho khoa hoc: {course.CourseName}\n\n";
                
                foreach (var feedback in feedbacks.Take(10)) // Show only first 10
                {
                    feedbackDetails += $"• {feedback.Student?.StudentName ?? "Vo danh"}: " +
                                      $"{feedback.Rating}/5 ? ({feedback.FeedbackDate:dd/MM/yyyy})\n" +
                                      $"  \"{feedback.Comment}\"\n\n";
                }

                if (feedbacks.Count > 10)
                {
                    feedbackDetails += $"... va {feedbacks.Count - 10} danh gia khac";
                }

                MessageBox.Show(feedbackDetails, "Danh Gia Khoa Hoc", 
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi tai danh gia: {ex.Message}", "Loi",
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

    // ViewModel for course rating display
    public class CourseRatingViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int InstructorId { get; set; }
        public int TotalStudents { get; set; }
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public string AverageRatingText { get; set; } = string.Empty;
        public string StarDisplay { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedDateText { get; set; } = string.Empty;
        public bool HasRatings { get; set; }
        public string RatingCategory { get; set; } = string.Empty;
        
        // Additional properties for UI
        public string RatingColor => AverageRating >= 4 ? "#4CAF50" : 
                                    AverageRating >= 3 ? "#FF9800" : 
                                    AverageRating >= 1 ? "#F44336" : "#9E9E9E";
        
        public string RatingQuality => AverageRating >= 4 ? "Xuat sac" :
                                      AverageRating >= 3 ? "Tot" :
                                      AverageRating >= 1 ? "Can cai thien" : "Chua danh gia";
    }
}