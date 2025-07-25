using System;
using System.Collections.Generic;
using System.IO;
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
using ProjectPRN.Admin;
using Repositories;
using Repositories.Interfaces;
using Path = System.IO.Path;

namespace ProjectPRN.AssessmentManagement
{
    /// <summary>
    /// Interaction logic for AssessmentManagementWindow.xaml
    /// </summary>
    public partial class AssessmentManagementWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private readonly IAssessmentRepository _assessmentRepository;
        private List<Assessment> _allAssessments; // Danh sách gốc để lọc
        public AssessmentManagementWindow()
        {
            InitializeComponent();

            // Khởi tạo Repository
            _assessmentRepository = new AssessmentRepository(new AssessmentDAO());
            LoadAssessments();
        }
        private async void LoadAssessments()
        {
            try
            {
                // Lấy toàn bộ bài kiểm tra (có bao gồm khóa học)
                var assessments = await _assessmentRepository.GetAllAsync();
                _allAssessments = assessments.ToList(); // Lưu gốc
                // Hiển thị lên DataGrid
                AssessmentGrid.ItemsSource = assessments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu bài kiểm tra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddAssessment_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddAssessmentWindow();
            var result = window.ShowDialog(); // ← lấy kết quả
            if (result == true)
            {
                LoadAssessments(); // ← load lại danh sách sau khi thêm
            }
        }
        private async void EditAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (AssessmentGrid.SelectedItem is Assessment selectedAssessment)
            {
                var window = new AddAssessmentWindow(selectedAssessment); // mở ở chế độ sửa
                var result = window.ShowDialog();
                if (result == true)
                {
                    LoadAssessments(); // reload sau khi sửa
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một bài kiểm tra để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (AssessmentGrid.SelectedItem is Assessment selectedAssessment)
            {
                var confirm = MessageBox.Show($"Bạn có chắc muốn xóa bài kiểm tra '{selectedAssessment.AssessmentName}' không?",
                                               "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _assessmentRepository.DeleteAsync(selectedAssessment.AssessmentId);
                        LoadAssessments();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một bài kiểm tra để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SubmitAssessment_Click(object sender, RoutedEventArgs e)
        {
            // Tính năng cho học viên nộp bài (nếu làm)
            MessageBox.Show("Tính năng nộp bài chưa được triển khai.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InstructionFileButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Assessment assessment)
            {
                if (string.IsNullOrEmpty(assessment.InstructionFilePath))
                {
                    btn.Content = "Nộp file";
                }
                else
                {
                    btn.Content = "Tải file";
                }
            }
        }
        private void HandleInstructionFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var assessment = button?.DataContext as Assessment;

            if (assessment == null) return;

            string fullPath = string.IsNullOrEmpty(assessment.InstructionFilePath)
                ? string.Empty
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assessment.InstructionFilePath);

            if (string.IsNullOrEmpty(assessment.InstructionFilePath) || !File.Exists(fullPath))
            {
                UploadInstructionFile(assessment);
            }
            else
            {
                DownloadInstructionFile(assessment);
            }
        }
        private void UploadInstructionFile(Assessment assessment)
        {
            Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn file hướng dẫn",
                Filter = "All Files (*.*)|*.*"
            };

            if (openDlg.ShowDialog() == true)
            {
                try
                {
                    string selectedFile = openDlg.FileName;
                    string extension = Path.GetExtension(selectedFile);
                    string fileName = $"Instruction_Assignment{assessment.AssessmentId}_{assessment.AssessmentName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

                    string destDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InstructionFiles");
                    Directory.CreateDirectory(destDirectory);

                    string destPath = Path.Combine(destDirectory, fileName);
                    File.Copy(selectedFile, destPath, true);

                    // Lưu đường dẫn tương đối
                    assessment.InstructionFilePath = Path.Combine("InstructionFiles", fileName);

                    _assessmentRepository.UpdateAsyncNew(assessment.AssessmentId, assessment);

                    MessageBox.Show("Tải file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAssessments();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tải lên file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void DownloadInstructionFile(Assessment assessment)
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assessment.InstructionFilePath);
                string originalExtension = Path.GetExtension(fullPath);
                string fileName = Path.GetFileName(fullPath);

                Microsoft.Win32.SaveFileDialog saveDlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Lưu file hướng dẫn",
                    FileName = fileName,
                    Filter = "All Files (*.*)|*.*"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    string destinationPath = saveDlg.FileName;

                    // ✅ Nếu người dùng không nhập đuôi file, thì tự động thêm lại
                    if (Path.GetExtension(destinationPath) == "")
                    {
                        destinationPath += originalExtension;
                    }

                    File.Copy(fullPath, destinationPath, true);
                    File.SetLastWriteTime(destinationPath, DateTime.Now);

                    MessageBox.Show("Tải xuống thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string keyword = SearchTextBox.Text.Trim().ToLower();
            DateTime? fromDate = FromDatePicker.SelectedDate;
            DateTime? toDate = ToDatePicker.SelectedDate;

            var filtered = _allAssessments.Where(a =>
            {
                bool matchesKeyword =
                    (!string.IsNullOrEmpty(a.AssessmentName) && a.AssessmentName.ToLower().Contains(keyword)) ||
                    (a.Course != null && !string.IsNullOrEmpty(a.Course.CourseName) && a.Course.CourseName.ToLower().Contains(keyword));

                bool matchesDate = true;

                if (fromDate.HasValue)
                    matchesDate &= a.DueDate.Date >= fromDate.Value.Date;

                if (toDate.HasValue)
                    matchesDate &= a.DueDate.Date <= toDate.Value.Date;

                return matchesKeyword && matchesDate;
            }).ToList();

            AssessmentGrid.ItemsSource = filtered;
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
                SearchPlaceholder.Visibility = Visibility.Visible;
        }
        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;

            // Hiện lại placeholder nếu cần
            SearchPlaceholder.Visibility = Visibility.Visible;
            // Gọi lại LoadAssessments để lấy dữ liệu mới
            LoadAssessments();
        }

    }
}
