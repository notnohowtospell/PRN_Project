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
    /// Interaction logic for StudentNotificationWindow.xaml
    /// </summary>
    public partial class StudentNotificationWindow : Window
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly BusinessObjects.Models.Student _loggedInStudent;

        public StudentNotificationWindow(BusinessObjects.Models.Student student)
        {
            InitializeComponent();
            _notificationRepository = new NotificationRepository(new NotificationDAO());
            //_loggedInStudent = student;
            _loggedInStudent = student;

            LoadNotifications();
        }

        private async void LoadNotifications()
        {
            var notifications = await _notificationRepository.GetByStudentIdAsync(_loggedInStudent.StudentId);
            // Thêm (NEW) nếu thông báo mới (trong 3 ngày gần đây)
            foreach (var n in notifications)
            {
                if (n.CreatedDate >= DateTime.Now.AddDays(-1))
                {
                    n.Title += "  (NEW)";
                }
            }
            NotificationListView.ItemsSource = notifications;
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //mainWin.Show(); // Hiện lại cửa sổ chính
            this.Close();   // Đóng cửa sổ hiện tại
        }
    }
}
