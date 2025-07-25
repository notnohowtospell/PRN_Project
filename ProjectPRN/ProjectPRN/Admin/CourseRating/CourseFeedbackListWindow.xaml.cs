using System;
using System.Windows;

namespace ProjectPRN.Admin.CourseRating
{
    public partial class CourseFeedbackListWindow : Window
    {
        public CourseFeedbackListWindow(int courseId, string courseName = "")
        {
            InitializeComponent();
            
            // Set window title
            Title = $"Danh Gia Khoa Hoc: {courseName}";
            
            // Initialize the content
            Content = new CourseFeedbackListView(courseId);
            
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