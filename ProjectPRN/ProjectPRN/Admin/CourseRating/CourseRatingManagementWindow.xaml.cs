using System;
using System.Windows;

namespace ProjectPRN.Admin.CourseRating
{
    public partial class CourseRatingManagementWindow : Window
    {
        public CourseRatingManagementWindow()
        {
            InitializeComponent();
            
            // Set window properties
            Title = "Quan Ly Danh Gia Khoa Hoc - Admin Panel";
            
            // Initialize the content
            Content = new CourseRatingManagementView();
            
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