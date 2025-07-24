using System.Windows;
using System.Windows.Controls;
using BusinessObjects.Models;
using DataAccessObjects;
using ProjectPRN.Utils;
using Repositories;

namespace ProjectPRN.Dialogs
{
    public partial class FeedbackDialog : Window
    {
        private readonly IFeedbackDAO _feedbackDAO;
        private readonly LifeSkillCourse _course;
        private readonly BusinessObjects.Models.Student _currentStudent;
        private IStudentDAO _studentDAO;

        public FeedbackDialog(LifeSkillCourse course)
        {
            InitializeComponent();
            _studentDAO = new StudentDAO();
            _feedbackDAO = new FeedbackDAO();
            _course = course;
            var currentID = SessionManager.GetCurrentUserId();
            _currentStudent =_studentDAO.GetByIdAsync(currentID).Result;

            InitializeDialog();
        }

        private void InitializeDialog()
        {
            // Set course information
            CourseNameTextBlock.Text = _course.CourseName;
            InstructorNameTextBlock.Text = _course.Instructor?.InstructorName ?? "N/A";

            // Set default rating
            RatingComboBox.SelectedIndex = 4; // 5 stars default
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(FeedbackTextBox.Text))
                {
                    MessageBox.Show("Please enter your feedback.", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (RatingComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a rating.", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get rating value
                var selectedRating = (ComboBoxItem)RatingComboBox.SelectedItem;
                int rating = int.Parse(selectedRating.Tag.ToString());

                // Create feedback object
                var feedback = new Feedback
                {
                    StudentId = _currentStudent.StudentId,
                    CourseId = _course.CourseId,
                    Rating = rating,
                    Comment = FeedbackTextBox.Text.Trim(),
                    FeedbackDate = DateTime.Now,

                };

                // Save feedback
                await _feedbackDAO.AddAsync(feedback);

                MessageBox.Show("Thank you for your feedback! Your review has been submitted successfully.",
                              "Feedback Submitted", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while submitting feedback: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Star1.Text = rating >= 1 ? "★" : "☆";
            Star2.Text = rating >= 2 ? "★" : "☆";
            Star3.Text = rating >= 3 ? "★" : "☆";
            Star4.Text = rating >= 4 ? "★" : "☆";
            Star5.Text = rating >= 5 ? "★" : "☆";
        }
    }
}