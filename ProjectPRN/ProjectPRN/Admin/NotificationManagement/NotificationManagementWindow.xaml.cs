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
    /// Interaction logic for NotificationManagementWindow.xaml
    /// </summary>
    public partial class NotificationManagementWindow : Window
    {
        private readonly INotificationRepository _notificationRepo = new NotificationRepository(new NotificationDAO());
        private Notification? selectedNotification;

        public NotificationManagementWindow()
        {
            InitializeComponent();
            LoadNotifications();
        }

        private async void LoadNotifications()
        {
            var notifications = await _notificationRepo.GetAllAsync();
            NotificationDataGrid.ItemsSource = notifications;
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (selectedNotification == null)
            {
                MessageBox.Show("Vui lòng chọn thông báo cần sửa.");
                return;
            }

            selectedNotification.Title = TitleTextBox.Text;
            selectedNotification.Content = ContentTextBox.Text;

            await _notificationRepo.UpdateAsync(selectedNotification);
            LoadNotifications();
            ClearForm(null, null);
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedNotification == null)
            {
                MessageBox.Show("Vui lòng chọn thông báo cần xóa.");
                return;
            }

            if (MessageBox.Show("Xác nhận xóa?", "Thông báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _notificationRepo.DeleteAsync(selectedNotification.NotificationId);
                LoadNotifications();
                ClearForm(null, null);
            }
        }

        private void NotificationDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedNotification = NotificationDataGrid.SelectedItem as Notification;
            if (selectedNotification != null)
            {
                TitleTextBox.Text = selectedNotification.Title;
                ContentTextBox.Text = selectedNotification.Content;
            }
        }

        private void ClearForm(object sender, RoutedEventArgs e)
        {
            selectedNotification = null;
            TitleTextBox.Clear();
            ContentTextBox.Clear();
            NotificationDataGrid.UnselectAll();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Hoặc điều hướng về MainWindow nếu cần
        }
        private async void FilterByDate_Click(object sender, RoutedEventArgs e)
        {
            var fromDate = FromDatePicker.SelectedDate;
            var toDate = ToDatePicker.SelectedDate;

            var allNotifications = await _notificationRepo.GetAllAsync();

            if (fromDate.HasValue)
            {
                allNotifications = allNotifications
                    .Where(n => n.CreatedDate.Date >= fromDate.Value.Date)
                    .ToList();
            }

            if (toDate.HasValue)
            {
                allNotifications = allNotifications
                    .Where(n => n.CreatedDate.Date <= toDate.Value.Date)
                    .ToList();
            }

            NotificationDataGrid.ItemsSource = allNotifications;
        }
    }
}
