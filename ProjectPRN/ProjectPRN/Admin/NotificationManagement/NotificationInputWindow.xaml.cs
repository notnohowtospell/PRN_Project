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
    /// Interaction logic for NotificationInputWindow.xaml
    /// </summary>
    public partial class NotificationInputWindow : Window
    {
        private readonly INotificationRepository _notificationRepository;
        public string NotificationTitle => TitleTextBox.Text;
        public string NotificationContent => ContentTextBox.Text;

        public string StudentNameDisplay { get; set; }

        public NotificationInputWindow(BusinessObjects.Models.Student student = null)
        {
            InitializeComponent();
            _notificationRepository = new NotificationRepository(new NotificationDAO());

            if (student != null)
            {
                StudentNameDisplay = $"Gửi cho: {student.StudentName}";
            }
            else
            {
                StudentNameDisplay = "Gửi thông báo cho nhiều sinh viên";
            }

            DataContext = this;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NotificationTitle) || string.IsNullOrWhiteSpace(NotificationContent))
            {
                MessageBox.Show("Vui lòng nhập tiêu đề và nội dung.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
