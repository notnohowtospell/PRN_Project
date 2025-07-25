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
using Repositories.Interfaces;
using Repositories;
using Path = System.IO.Path;

namespace ProjectPRN.AssessmentManagement
{
    /// <summary>
    /// Interaction logic for StudentAssessmentWindow.xaml
    /// </summary>
    public partial class StudentAssessmentWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IAssessmentResultRepository _assessmentResultRepository;
        private readonly ILifeSkillCourseRepository _lifeSkillCourseRepository;
        private readonly int _studentId;

        public StudentAssessmentWindow(int studentId)
        {
            InitializeComponent();
            _studentId = studentId;
            _assessmentRepository = new AssessmentRepository(new AssessmentDAO());
            _assessmentResultRepository = new AssessmentResultRepository(new AssessmentResultDAO());
            _lifeSkillCourseRepository = new LifeSkillCourseRepository(new LifeSkillCourseDAO());
            LoadAssessments();
            LoadCourse();
        }

        private async void LoadAssessments()
        {
            try
            {
                // Lấy toàn bộ bài kiểm tra (có bao gồm khóa học)
                var allResults = await _assessmentResultRepository.GetAllAsync();
                // Lọc theo studentId
                var studentResults = allResults
                    .Where(r => r.StudentId == _studentId)
                    .ToList();
                // Hiển thị lên DataGrid
                StudentAssessmentGrid.ItemsSource = studentResults;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu bài kiểm tra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadCourse()
        {
            var courses = await _lifeSkillCourseRepository.GetAllAsync();
            CourseFilterComboBox.ItemsSource = courses;
            CourseFilterComboBox.DisplayMemberPath = "CourseName"; // Hiển thị tên
            CourseFilterComboBox.SelectedValuePath = "CourseId";   // Lấy ID khi chọn
        }

        private void SubmitAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Assessment assessment)
            {
                var result = assessment.AssessmentResults.FirstOrDefault();
                if (result != null)
                {
                    HandleSubmit(result);
                }
                else
                {
                    MessageBox.Show("Chưa có bài nộp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void SubmitButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Assessment assessment)
            {
                var result = assessment.AssessmentResults.FirstOrDefault();

                if (result != null)
                {
                    if (assessment.DueDate < DateTime.Now)
                    {
                        btn.Content = "Hết hạn";
                        btn.IsEnabled = false;
                    }
                    else if (!string.IsNullOrEmpty(result.SubmissionFilePath))
                    {
                        btn.Content = "Sửa bài";
                    }
                    else
                    {
                        btn.Content = "Nộp bài";
                    }
                }
            }
        }

        private void DownloadInstructionFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Assessment assessment)
            {
                HandleDownload(assessment);
            }
        }

        private async void HandleSubmit(AssessmentResult assessmentResult)
        {
            // Kiểm tra hạn nộp
            if (assessmentResult.Assessment != null && assessmentResult.Assessment.DueDate < DateTime.Now)
            {
                MessageBox.Show("Đã quá hạn nộp bài, bạn không thể nộp hoặc sửa bài nữa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
                    string fileName = $"Submit_Assignment{assessmentResult.AssessmentId}_StudentId{assessmentResult.StudentId}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

                    string destDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SubmitFiles");
                    Directory.CreateDirectory(destDirectory);

                    string destPath = Path.Combine(destDirectory, fileName);
                    File.Copy(selectedFile, destPath, true);

                    // Lưu đường dẫn tương đối
                    assessmentResult.SubmissionFilePath = Path.Combine("SubmitFiles", fileName);
                    assessmentResult.SubmissionDate = DateTime.Now;

                    await _assessmentResultRepository.UpdateAsyncResultNew(assessmentResult.ResultId, assessmentResult);

                    MessageBox.Show("Tải file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAssessments();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tải lên file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void HandleDownload(Assessment assessment)
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

        private void SubmissionDateTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock tb && tb.Tag is Assessment assessment)
            {
                var result = assessment.AssessmentResults?.FirstOrDefault();
                if (result?.SubmissionDate != null)
                {
                    tb.Text = $"Nộp: {result.SubmissionDate:dd/MM/yyyy HH:mm}";
                }
                else
                {
                    tb.Text = ""; // hoặc tb.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Đóng cửa sổ hiện tại
            this.Close();
        }
        private async void CourseFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CourseFilterComboBox.SelectedItem is LifeSkillCourse selectedCourse)
            {
                var allResults = await _assessmentResultRepository.GetAllAsync();

                var filtered = allResults
                    .Where(r => r.Assessment != null && r.Assessment.Course != null
                                && r.Assessment.Course.CourseId == selectedCourse.CourseId)
                    .ToList();

                StudentAssessmentGrid.ItemsSource = filtered;
            }
        }


    }
}
