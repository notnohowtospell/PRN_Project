using System.Windows;
using System.Windows.Controls;
using ProjectPRN.DTOs;
using MaterialDesignThemes.Wpf;

namespace ProjectPRN.Admin.BackupRestore
{
    public partial class RestoreConfirmDialog : UserControl
    {
        public BackupData? BackupData { get; set; }
        public bool IsConfirmed { get; private set; }

        public RestoreConfirmDialog()
        {
            InitializeComponent();
        }

        public RestoreConfirmDialog(BackupData backupData) : this()
        {
            BackupData = backupData;
            LoadBackupInfo();
        }

        private void LoadBackupInfo()
        {
            if (BackupData == null) return;

            try
            {
                txtBackupDate.Text = BackupData.BackupDate.ToString("dd/MM/yyyy HH:mm:ss");
                txtVersion.Text = BackupData.Version ?? "1.0";
                txtStudentCount.Text = $"{BackupData.Students?.Count ?? 0} hoc vien";
                txtInstructorCount.Text = $"{BackupData.Instructors?.Count ?? 0} giang vien";
                txtCourseCount.Text = $"{BackupData.Courses?.Count ?? 0} khoa hoc";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi tai thong tin file sao luu: {ex.Message}", "Loi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChkConfirm_Checked(object sender, RoutedEventArgs e)
        {
            btnConfirm.IsEnabled = true;
        }

        private void ChkConfirm_Unchecked(object sender, RoutedEventArgs e)
        {
            btnConfirm.IsEnabled = false;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsConfirmed = true;
                DialogHost.CloseDialogCommand.Execute(true, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi xac nhan: {ex.Message}", "Loi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsConfirmed = false;
                DialogHost.CloseDialogCommand.Execute(false, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi huy: {ex.Message}", "Loi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsConfirmed = false;
                DialogHost.CloseDialogCommand.Execute(false, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi dong: {ex.Message}", "Loi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}