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

namespace ProjectPRN.CertificateManagement
{
    /// <summary>
    /// Interaction logic for StudentCertificateWindow.xaml
    /// </summary>
    public partial class StudentCertificateWindow : Window
    {
        private readonly int _studentId;
        private readonly ICertificateRepository _certificateRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IAssessmentResultRepository _assessmentResultRepository;
        private List<LifeSkillCourse> _courses = new();
        private List<Certificate> _allCertificates = new(); // lưu toàn bộ chứng chỉ để lọc
        public StudentCertificateWindow(int studentId)
        {
            InitializeComponent();
            _studentId = studentId; // Lấy từ login
            _assessmentRepository = new AssessmentRepository(new AssessmentDAO());
            _certificateRepository = new CertificateRepository(new CertificateDAO());
            _assessmentResultRepository = new AssessmentResultRepository(new AssessmentResultDAO());
            Loaded += StudentCertificatePage_Loaded;
        }

        private void StudentCertificatePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCertificatesAsync();
        }
        private async void LoadCertificatesAsync()
        {
            try
            {
                _allCertificates = (await _certificateRepository.GetCertificatesByStudentAsync(_studentId)).ToList();
                CertificateDataGrid.ItemsSource = _allCertificates;

                // Load tất cả khóa học từ chứng chỉ
                var courses = _allCertificates
                    .Where(c => c.Course != null) // đảm bảo không null
                    .Select(c => c.Course)
                    .DistinctBy(c => c.CourseId)
                    .ToList();

                CourseFilterComboBox.ItemsSource = courses;
                CourseFilterComboBox.SelectedIndex = -1; // Không chọn gì ban đầu
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách chứng chỉ: " + ex.Message);
            }
        }

        private void CourseFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CourseFilterComboBox.SelectedItem is LifeSkillCourse selectedCourse)
            {
                CertificateDataGrid.ItemsSource = _allCertificates
                    .Where(c => c.CourseId == selectedCourse.CourseId)
                    .ToList();
            }
            else
            {
                CertificateDataGrid.ItemsSource = _allCertificates;
            }
        }

        private void DownloadCertificateFile(Certificate cert)
        {
            if (cert == null || string.IsNullOrEmpty(cert.FilePath))
            {
                MessageBox.Show("Chứng chỉ không tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                // Đường dẫn gốc file PDF đã lưu (đã là relative path như "certificates/...")
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cert.FilePath);
                string originalExtension = Path.GetExtension(fullPath);
                string fileName = Path.GetFileName(fullPath);

                // Hộp thoại lưu file
                Microsoft.Win32.SaveFileDialog saveDlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Lưu chứng chỉ",
                    FileName = fileName,
                    Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*"
                };

                if (saveDlg.ShowDialog() == true)
                {
                    string destinationPath = saveDlg.FileName;

                    // Nếu người dùng không nhập đuôi file, thêm lại
                    if (Path.GetExtension(destinationPath) == "")
                    {
                        destinationPath += originalExtension;
                    }

                    File.Copy(fullPath, destinationPath, true);
                    File.SetLastWriteTime(destinationPath, DateTime.Now);

                    MessageBox.Show("Tải chứng chỉ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải chứng chỉ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void DownloadCertificate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Certificate cert)
            {
                if (cert != null)
                {
                    DownloadCertificateFile(cert);
                }
                else
                {
                    MessageBox.Show("Chưa có chứng chỉ cho sinh viên này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Hoặc điều hướng về MainWindow nếu cần
        }
    }
}
