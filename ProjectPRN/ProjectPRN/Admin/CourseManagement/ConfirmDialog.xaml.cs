using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace ProjectPRN.Admin.CourseManagement
{
    public partial class ConfirmDialog : UserControl
    {
        public ConfirmDialog(string title, string message)
        {
            InitializeComponent();

            txtTitle.Text = title;
            txtMessage.Text = message;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(true, this);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(false, this);
        }
    }
}