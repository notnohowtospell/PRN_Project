using System;
using System.Windows;
using BusinessObjects.Models;

namespace ProjectPRN.Student.Feedback
{
    public partial class CourseFeedbackWindow : Window
    {
        private readonly BusinessObjects.Models.Student _currentStudent;

        public CourseFeedbackWindow(BusinessObjects.Models.Student student)
        {
            InitializeComponent();
            
            _currentStudent = student ?? throw new ArgumentNullException(nameof(student));
            
            // Set window properties
            Title = $"?ánh Giá Khóa H?c - {_currentStudent.StudentName}";
            
            // Initialize the content
            Content = new CourseFeedbackView(_currentStudent);
            
            // Set window state
            WindowState = WindowState.Normal;
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up if needed
            base.OnClosed(e);
        }
    }
}