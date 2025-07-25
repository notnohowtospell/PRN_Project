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
using Path = System.IO.Path;
using System.IO;

namespace ProjectPRN.AssessmentManagement
{
    /// <summary>
    /// Interaction logic for StudentSubmissionManagementWindow.xaml
    /// </summary>
    public partial class StudentSubmissionManagementWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IAssessmentResultRepository _assessmentResultRepository;
        private readonly ILifeSkillCourseRepository _lifeSkillCourseRepository;
        private List<AssessmentResult> _allAssessmentResults = new List<AssessmentResult>();
        public StudentSubmissionManagementWindow()
        {
            InitializeComponent();
            _assessmentRepository = new AssessmentRepository(new AssessmentDAO());
            _assessmentResultRepository = new AssessmentResultRepository(new AssessmentResultDAO());
            _lifeSkillCourseRepository = new LifeSkillCourseRepository(new LifeSkillCourseDAO());
            LoadData();

        }

        private async void LoadData()
        {
            try
            {
                _allAssessmentResults = (await _assessmentResultRepository.GetAllAsync()).ToList();
                SubmissionDataGrid.ItemsSource = _allAssessmentResults;

                var courses = await _lifeSkillCourseRepository.GetAllAsync();
                CourseFilterComboBox.ItemsSource = courses;
                CourseFilterComboBox.DisplayMemberPath = "CourseName";
                CourseFilterComboBox.SelectedValuePath = "CourseId";

                var assessments = await _assessmentRepository.GetAllAsync();
                AssignmentFilterComboBox.ItemsSource = assessments;
                AssignmentFilterComboBox.DisplayMemberPath = "AssessmentName";
                AssignmentFilterComboBox.SelectedValuePath = "AssessmentId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CourseFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CourseFilterComboBox.SelectedValue is int selectedCourseId)
            {
                var filtered = _allAssessmentResults
                    .Where(r => r.Assessment.CourseId == selectedCourseId)
                    .ToList();
                SubmissionDataGrid.ItemsSource = filtered;
            }
            else
            {
                SubmissionDataGrid.ItemsSource = _allAssessmentResults;
            }
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            var assessmentResult = (sender as Button)?.DataContext as AssessmentResult;
            if (assessmentResult == null || string.IsNullOrEmpty(assessmentResult.SubmissionFilePath))
            {
                MessageBox.Show("Không có file nộp để tải xuống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assessmentResult.SubmissionFilePath);
                string originalExtension = Path.GetExtension(fullPath);
                string fileName = Path.GetFileName(fullPath);

                Microsoft.Win32.SaveFileDialog saveDlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Lưu file nộp",
                    FileName = fileName,
                    Filter = "All Files (*.*)|*.*"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    string destinationPath = saveDlg.FileName;

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
        private async void EditScore_Click(object sender, RoutedEventArgs e)
        {
            var result = (sender as Button)?.DataContext as AssessmentResult;
            if (result == null)
            {
                MessageBox.Show("Không tìm thấy dữ liệu bài nộp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Nhập điểm cho {result.Student.StudentName} (tối đa {result.Assessment.MaxScore}):",
                "Chấm điểm",
                result.Score?.ToString() ?? "0"
            );

            if (decimal.TryParse(input, out decimal score))
            {
                if (score < 0 || score > result.Assessment.MaxScore)
                {
                    MessageBox.Show($"Điểm phải nằm trong khoảng 0 đến {result.Assessment.MaxScore}.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    result.Score = score;
                    await _assessmentResultRepository.UpdateAsync(result);
                    MessageBox.Show("Cập nhật điểm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi cập nhật điểm: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Điểm không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void FilterChanged(object sender, EventArgs e)
        {
            var filtered = _allAssessmentResults.AsQueryable();

            if (CourseFilterComboBox.SelectedValue is int selectedCourseId)
            {
                filtered = filtered.Where(x => x.Assessment.CourseId == selectedCourseId);
            }

            if (AssignmentFilterComboBox.SelectedValue is int selectedAssessmentId)
            {
                filtered = filtered.Where(x => x.AssessmentId == selectedAssessmentId);
            }

            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                filtered = filtered.Where(x => x.Student.StudentName.ToLower().Contains(SearchTextBox.Text.ToLower()));
            }

            SubmissionDataGrid.ItemsSource = filtered.ToList();
        }
        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            CourseFilterComboBox.SelectedIndex = -1;
            AssignmentFilterComboBox.SelectedIndex = -1;
            SearchTextBox.Text = string.Empty;
            SubmissionDataGrid.ItemsSource = _allAssessmentResults;
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Đóng cửa sổ hiện tại
            this.Close();
        }
    }
}
