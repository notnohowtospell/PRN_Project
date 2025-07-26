using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BusinessObjects.Models;
using DataAccessObjects;
using Repositories.Interfaces;
using Repositories;

namespace ProjectPRN.NotificationManagement
{
    /// <summary>
    /// Interaction logic for AdminNotificationWindow.xaml
    /// </summary>
    public partial class AdminNotificationWindow : Window
    {
        private readonly IStudentRepository _studentRepo = new StudentRepository(new StudentDAO());
        private readonly INotificationRepository _notificationRepo = new NotificationRepository(new NotificationDAO());
        private readonly ILifeSkillCourseRepository _courseRepo = new LifeSkillCourseRepository(new LifeSkillCourseDAO()); // nếu có
        private List<BusinessObjects.Models.Student> _allStudents = new();

        public AdminNotificationWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            var students = await _studentRepo.GetAllAsync();
            _allStudents = students.ToList();

            // Load courses nếu có
            var courses = await _courseRepo.GetAllAsync(); // nếu bạn có CourseRepo
            CourseComboBox.ItemsSource = courses;

            StudentDataGrid.ItemsSource = _allStudents;
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            string keyword = SearchTextBox.Text.ToLower();
            int? selectedCourseId = CourseComboBox.SelectedValue as int?;

            var filtered = _allStudents.Where(s =>
                (string.IsNullOrEmpty(keyword) || s.StudentName.ToLower().Contains(keyword)) &&
                (!selectedCourseId.HasValue || s.Enrollments.Any(e => e.CourseId == selectedCourseId))
            ).ToList();

            StudentDataGrid.ItemsSource = filtered;
        }

        private async void SendToOne_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var student = button?.Tag as BusinessObjects.Models.Student;
            if (student == null) return;

            var input = new NotificationInputWindow(student); // custom window để nhập tiêu đề và nội dung
            if (input.ShowDialog() == true)
            {
                var noti = new Notification
                {
                    Title = input.Title,
                    Content = input.ContentTextBox.Text,
                    StudentId = student.StudentId,
                    CreatedDate = DateTime.Now
                };
                await _notificationRepo.AddAsync(noti);
                MessageBox.Show($"Đã gửi thông báo cho {student.StudentName}");
            }
        }

        private async void SendToAll_Click(object sender, RoutedEventArgs e)
        {
            var students = StudentDataGrid.ItemsSource as IEnumerable<BusinessObjects.Models.Student>;
            if (students == null || !students.Any())
            {
                MessageBox.Show("Không có sinh viên để gửi.");
                return;
            }

            var input = new NotificationInputWindow(); // không có Student cụ thể
            if (input.ShowDialog() == true)
            {
                foreach (var s in students)
                {
                    var noti = new Notification
                    {
                        Title = input.NotificationTitle,
                        Content = input.NotificationContent,
                        StudentId = s.StudentId,
                        CreatedDate = DateTime.Now
                    };
                    await _notificationRepo.AddAsync(noti);
                }
                MessageBox.Show("Đã gửi thông báo đến tất cả sinh viên trong danh sách.");
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Đóng cửa sổ hiện tại
            this.Close();
        }
    }
}
