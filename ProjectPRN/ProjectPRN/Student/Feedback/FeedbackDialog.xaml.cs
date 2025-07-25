using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using ProjectPRN.Utils;

namespace ProjectPRN.Student.Feedback
{
    public partial class FeedbackDialog : Window
    {
        private readonly LifeSkillCourse _course;
        private readonly BusinessObjects.Models.Student _currentStudent;
        private readonly BusinessObjects.Models.Feedback? _existingFeedback;
        private bool _isEditMode;

        public FeedbackDialog(BusinessObjects.Models.Student student, LifeSkillCourse course, BusinessObjects.Models.Feedback? existingFeedback = null)
        {
            InitializeComponent();
            
            _currentStudent = student ?? throw new ArgumentNullException(nameof(student));
            _course = course ?? throw new ArgumentNullException(nameof(course));
            _existingFeedback = existingFeedback;
            _isEditMode = existingFeedback != null;

            InitializeDialog();
        }

        private void InitializeDialog()
        {
            // Set window title based on mode
            Title = _isEditMode ? "Chỉnh Sửa Đánh Giá" : "Đánh Giá Khóa Học";
            
            // Set course information
            CourseNameTextBlock.Text = _course.CourseName;
            
            // Load instructor name
            LoadInstructorName();

            if (_isEditMode && _existingFeedback != null)
            {
                // Load existing feedback data
                FeedbackTextBox.Text = _existingFeedback.Comment ?? "";
                
                // Set rating
                for (int i = 0; i < RatingComboBox.Items.Count; i++)
                {
                    if (RatingComboBox.Items[i] is ComboBoxItem item && 
                        int.Parse(item.Tag.ToString()) == _existingFeedback.Rating)
                    {
                        RatingComboBox.SelectedIndex = i;
                        break;
                    }
                }
                
                SubmitButton.Content = "Cập Nhật Đánh Giá";
                
                // Show existing feedback info
                var existingInfo = $"Đánh giá hiện tại: {_existingFeedback.Rating}/5 ⭐ " +
                                  $"(Ngày đánh giá: {_existingFeedback.FeedbackDate:dd/MM/yyyy})";
                
                // Add existing feedback info to dialog (you might want to add a TextBlock for this in XAML)
                Title += $" - {existingInfo}";
            }
            else
            {
                // Set default rating for new feedback
                RatingComboBox.SelectedIndex = 4; // 5 stars default
                SubmitButton.Content = "Gửi Đánh Giá";
            }
        }

        private async void LoadInstructorName()
        {
            try
            {
                using var context = new ApplicationDbContext();
                var instructor = await context.LifeSkillCourses
                    .Where(c => c.CourseId == _course.CourseId)
                    .Select(c => c.Instructor!.InstructorName)
                    .FirstOrDefaultAsync();
                
                InstructorNameTextBlock.Text = instructor ?? "Không có thông tin giảng viên";
            }
            catch (Exception ex)
            {
                InstructorNameTextBlock.Text = "Không thể tải thông tin giảng viên";
                System.Diagnostics.Debug.WriteLine($"Error loading instructor: {ex.Message}");
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable button to prevent double-clicks
                SubmitButton.IsEnabled = false;
                
                // Validate input
                if (string.IsNullOrWhiteSpace(FeedbackTextBox.Text))
                {
                    MessageBox.Show("Vui lòng nhập nội dung đánh giá.", "Lỗi xác thực",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (RatingComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn số sao đánh giá.", "Lỗi xác thực",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get rating value
                var selectedRating = (ComboBoxItem)RatingComboBox.SelectedItem;
                int rating = int.Parse(selectedRating.Tag.ToString() ?? "5");

                using var context = new ApplicationDbContext();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    if (_isEditMode && _existingFeedback != null)
                    {
                        // Update existing feedback
                        var feedbackToUpdate = await context.Feedbacks
                            .FirstOrDefaultAsync(f => f.FeedbackId == _existingFeedback.FeedbackId);

                        if (feedbackToUpdate != null)
                        {
                            feedbackToUpdate.Rating = rating;
                            feedbackToUpdate.Comment = FeedbackTextBox.Text.Trim();
                            feedbackToUpdate.FeedbackDate = DateTime.Now; // Update feedback date
                            
                            await context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            
                            MessageBox.Show("Đánh giá đã được cập nhật thành công!", "Cập nhật thành công",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy đánh giá để cập nhật.", "Lỗi",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        // Check if feedback already exists (safety check)
                        var existingCheck = await context.Feedbacks
                            .FirstOrDefaultAsync(f => f.StudentId == _currentStudent.StudentId && 
                                                     f.CourseId == _course.CourseId);

                        if (existingCheck != null)
                        {
                            MessageBox.Show("Bạn đã đánh giá khóa học này rồi. Vui lòng sử dụng chức năng chỉnh sửa.", "Thông báo",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        // Create new feedback
                        var feedback = new BusinessObjects.Models.Feedback
                        {
                            StudentId = _currentStudent.StudentId,
                            CourseId = _course.CourseId,
                            Rating = rating,
                            Comment = FeedbackTextBox.Text.Trim(),
                            FeedbackDate = DateTime.Now
                        };

                        context.Feedbacks.Add(feedback);
                        await context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        
                        MessageBox.Show("Cảm ơn bạn đã đánh giá! Đánh giá của bạn đã được gửi thành công.", "Đánh giá thành công",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Lỗi khi lưu đánh giá: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi khi lưu đánh giá: {ex.Message}", "Lỗi",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable button
                SubmitButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void RatingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RatingComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int rating = int.Parse(selectedItem.Tag.ToString());
                UpdateStarDisplay(rating);
            }
        }

        private void UpdateStarDisplay(int rating)
        {
            // Update star display based on rating
            if (Star1 != null) Star1.Text = rating >= 1 ? "★" : "☆";
            if (Star2 != null) Star2.Text = rating >= 2 ? "★" : "☆";
            if (Star3 != null) Star3.Text = rating >= 3 ? "★" : "☆";
            if (Star4 != null) Star4.Text = rating >= 4 ? "★" : "☆";
            if (Star5 != null) Star5.Text = rating >= 5 ? "★" : "☆";
        }
    }
}