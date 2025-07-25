using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BusinessObjects.Models;
using DataAccessObjects;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using Repositories;
using Repositories.Interfaces;
using Path = System.IO.Path;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font;
using TextAlignment = iText.Layout.Properties.TextAlignment;

namespace ProjectPRN.CertificateManagement
{
    /// <summary>
    /// Interaction logic for CertificateManagementWindow.xaml
    /// </summary>
    public partial class CertificateManagementWindow : Window
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IAssessmentResultRepository _assessmentResultRepository;
        private List<LifeSkillCourse> _courses = new();
        public CertificateManagementWindow()
        {
            InitializeComponent();
            _assessmentRepository = new AssessmentRepository(new AssessmentDAO());
            _certificateRepository = new CertificateRepository(new CertificateDAO());
            _assessmentResultRepository = new AssessmentResultRepository(new AssessmentResultDAO());
            LoadPassedStudentsAsync();
        }
        private async void LoadPassedStudentsAsync()
        {
            var results = await _assessmentResultRepository.GetPassedResultsAsync();
            StudentDataGrid.ItemsSource = results;

            // Lấy danh sách khóa học duy nhất từ AssessmentResult
            _courses = results
                .Select(r => r.Assessment.Course)
                .GroupBy(c => c.CourseId)
                .Select(g => g.First())
                .ToList();

            CourseComboBox.ItemsSource = _courses;
        }
        private async void CourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CourseComboBox.SelectedItem is LifeSkillCourse selectedCourse)
            {
                var results = await _assessmentResultRepository.GetPassedResultsAsync();
                var filtered = results.Where(r => r.Assessment.CourseId == selectedCourse.CourseId).ToList();
                StudentDataGrid.ItemsSource = filtered;
            }
        }


        private async void GenerateCertificate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AssessmentResult result)
            {
                var studentId = result.StudentId;
                var courseId = result.Assessment.CourseId;

                //Kiểm tra đã cấp chứng chỉ chưa
               var existed = await _certificateRepository.GetCertificateByStudentAndCourseAsync(studentId, courseId);
                if (existed != null)
                {
                    MessageBox.Show("Sinh viên này đã được cấp chứng chỉ cho khóa học này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Lấy student và course
                var student = result.Student;
                var course = result.Assessment.Course;

                // Tạo chứng chỉ mới
                var certificate = new Certificate
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    IssueDate = DateTime.Now,
                    CertificateCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),

                };
                // Gọi hàm tạo PDF và nhận đường dẫn
                GenerateCertificatePdf(certificate, student, course);
                await _certificateRepository.CreateAsync(certificate);
                MessageBox.Show("Đã cấp chứng chỉ thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        //private void CertificateButton_Loaded(object sender, RoutedEventArgs e)
        //{
        //}
        public static void GenerateCertificatePdf(Certificate cert, BusinessObjects.Models.Student student, LifeSkillCourse course)
        {
            // Tạo thư mục lưu file nếu chưa có
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, $"{student.StudentId}_{course.CourseId}.pdf");

            // Tạo writer với WriterProperties (không cần BouncyCastle)
            var writerProperties = new WriterProperties();
            using (var writer = new PdfWriter(filePath, writerProperties))
            using (var pdf = new PdfDocument(writer))
            {
                var doc = new Document(pdf);

                // Load font Unicode (Arial) để hỗ trợ tiếng Việt
                var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                PdfFont font = PdfFontFactory.CreateFont(
                                                fontPath,                     // string path
                                                PdfEncodings.IDENTITY_H,      // encoding
                                                PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED // hoặc true nếu đúng version
                                            );
                doc.SetFont(font);

                // Bắt đầu tạo nội dung chứng chỉ
                doc.Add(new Paragraph("CHỨNG CHỈ HOÀN THÀNH")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(24)
                    .SetBold());

                doc.Add(new Paragraph("Chứng nhận rằng")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16));

                doc.Add(new Paragraph(student.StudentName)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetBold());

                doc.Add(new Paragraph("đã hoàn thành khóa học kỹ năng:")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16));

                doc.Add(new Paragraph(course.CourseName)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(18)
                    .SetBold());

                doc.Add(new Paragraph($"Ngày cấp: {cert.IssueDate:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(20));

                doc.Add(new Paragraph($"Mã chứng chỉ: {cert.CertificateCode}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
            }

            // Cập nhật đường dẫn lưu file
            cert.FilePath = $"Certificates/{student.StudentId}_{course.CourseId}.pdf";
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
            if (sender is Button button && button.Tag is AssessmentResult result)
            {
                var cert = await _certificateRepository.GetCertificateByStudentAndCourseAsync(
                    result.StudentId, result.Assessment.Course.CourseId);

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
