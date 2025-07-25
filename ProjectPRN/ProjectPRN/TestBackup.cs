using System.Windows;

namespace ProjectPRN
{
    public partial class App : Application
    {
        // Thêm method này ?? test BackupRestore window
        public static void ShowBackupRestoreWindow()
        {
            try
            {
                //var backupWindow = new BackupRestoreView();
                //backupWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi mo Backup/Restore window: {ex.Message}", "Loi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}