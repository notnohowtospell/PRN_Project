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

namespace ProjectPRN.AssessmentManagement
{
    /// <summary>
    /// Interaction logic for AddAssessmentWindow.xaml
    /// </summary>
    public partial class AddAssessmentWindow : Window
    {
        private readonly AssessmentDAO _assessmentDAO;
        private readonly LifeSkillCourseDAO _courseDAO;
        private readonly Assessment _editingAssessment; // null nếu đang thêm
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IAssessmentResultRepository _assessmentResultRepository;
        private string selectedFilePath;
        public AddAssessmentWindow(Assessment? assessment = null)
        {
            InitializeComponent();
            _assessmentDAO = new AssessmentDAO();
            _courseDAO = new LifeSkillCourseDAO(); // nếu bạn chưa có dòng này
            _assessmentRepository = new AssessmentRepository(new AssessmentDAO());
            _assessmentResultRepository = new AssessmentResultRepository(new AssessmentResultDAO());

            _editingAssessment = assessment;

            if (_editingAssessment != null)
            {
                TitleTextBlock.Text = "Sửa bài kiểm tra";
                txtName.Text = _editingAssessment.AssessmentName;
                txtType.Text = _editingAssessment.AssessmentType;
                txtMaxScore.Text = _editingAssessment.MaxScore.ToString();
                dpDueDate.SelectedDate = _editingAssessment.DueDate;
                txtInstructions.Text = _editingAssessment.Instructions;
                // ✅ Thêm dòng sau để hiển thị tên file đã đính kèm
                if (!string.IsNullOrEmpty(_editingAssessment.InstructionFilePath))
                {
                    string fileName = System.IO.Path.GetFileName(_editingAssessment.InstructionFilePath);
                    txtFilePath.Text = fileName; // hoặc lblFileName.Content = fileName;
                    selectedFilePath = _editingAssessment.InstructionFilePath; // giữ lại đường dẫn cũ nếu chưa thay file
                }
            }
            else
            {
                TitleTextBlock.Text = "Thêm bài kiểm tra";
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput(out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtMaxScore.Text, out int maxScore))
            {
                MessageBox.Show("Điểm tối đa phải là số nguyên.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Tên bài kiểm tra không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpDueDate.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn hạn nộp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cbCourse.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn khóa học.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int selectedCourseId = (int)cbCourse.SelectedValue;

            try
            {
                if (_editingAssessment == null)
                {
                    var newAssessment = new Assessment
                    {
                        CourseId = selectedCourseId, // TODO: Truyền vào hoặc chọn khóa học phù hợp
                        AssessmentName = txtName.Text.Trim(),
                        AssessmentType = txtType.Text.Trim(),
                        MaxScore = maxScore,
                        DueDate = dpDueDate.SelectedDate.Value,
                        Instructions = txtInstructions.Text.Trim(),
                        InstructionFilePath = selectedFilePath // ✅ THÊM DÒNG NÀY
                    };

                    await _assessmentDAO.AddAsync(newAssessment);
                    // ✅ Sau khi thêm Assessment thành công → tạo AssessmentResult cho học sinh của khóa học đó
                    var studentDAO = new StudentDAO();
                    var studentsInCourse = await studentDAO.GetStudentsByCourseIdAsync(newAssessment.CourseId);

                    var assessmentResultDAO = new AssessmentResultDAO();
                    foreach (var student in studentsInCourse)
                    {
                        var result = new AssessmentResult
                        {
                            AssessmentId = newAssessment.AssessmentId,
                            StudentId = student.StudentId,
                            Score = null, // chưa có điểm
                            SubmissionFilePath = null,
                            SubmissionDate = null
                        };
                        await assessmentResultDAO.AddAsync(result);
                    }

                    MessageBox.Show("Đã thêm bài kiểm tra và tạo kết quả cho học sinh.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    bool courseChanged = _editingAssessment.CourseId != selectedCourseId;

                    //_editingAssessment.CourseId = selectedCourseId;
                    _editingAssessment.AssessmentName = txtName.Text.Trim();
                    _editingAssessment.AssessmentType = txtType.Text.Trim();
                    _editingAssessment.MaxScore = maxScore;
                    _editingAssessment.DueDate = dpDueDate.SelectedDate.Value;
                    _editingAssessment.Instructions = txtInstructions.Text.Trim();

                    if (courseChanged)
                    {
                        // Cập nhật CourseId mới
                        _editingAssessment.CourseId = selectedCourseId;

                        // XÓA các kết quả cũ
                        var oldResults = await _assessmentResultRepository.GetByAssessmentIdAsync(_editingAssessment.AssessmentId);
                        foreach (var result in oldResults)
                        {
                            await _assessmentResultRepository.DeleteAsync(result.ResultId);
                        }

                        // TẠO lại kết quả cho học sinh của khóa học mới
                        var newStudents = await _assessmentResultRepository.GetStudentsByCourseIdAsync(selectedCourseId);
                        foreach (var student in newStudents)
                        {
                            var newResult = new AssessmentResult
                            {
                                AssessmentId = _editingAssessment.AssessmentId,
                                StudentId = student.StudentId,
                                Score = null,
                                SubmissionDate = null,
                                SubmissionFilePath = null
                            };
                            await _assessmentResultRepository.AddAsync(newResult);
                        }
                    }

                    await _assessmentDAO.UpdateAsync(_editingAssessment);
                    MessageBox.Show("Đã cập nhật bài kiểm tra.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var courses = await _courseDAO.GetAllAsync(); // Dùng async để gọi đúng DAO
            cbCourse.ItemsSource = courses;

            if (_editingAssessment != null)
            {
                cbCourse.SelectedValue = _editingAssessment.CourseId;
            }
        }

        private async void BtnChooseFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn file hướng dẫn",
                Filter = "Tất cả các tệp (*.*)|*.*"
            };

            if (openDlg.ShowDialog() == true)
            {
                try
                {
                    string selectedFile = openDlg.FileName;
                    string extension = System.IO.Path.GetExtension(selectedFile);
                    string safeName = txtName.Text.Trim().Replace(" ", "_"); // optional cleanup
                    string fileName = $"Instruction_Assignment_{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

                    string destDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InstructionFiles");
                    Directory.CreateDirectory(destDirectory);

                    string destPath = System.IO.Path.Combine(destDirectory, fileName);
                    File.Copy(selectedFile, destPath, true);

                    // Cập nhật đường dẫn hiển thị trên giao diện
                    txtFilePath.Text = fileName;

                    // Nếu đang ở chế độ chỉnh sửa, cập nhật luôn DB
                    if (_editingAssessment != null)
                    {
                        _editingAssessment.InstructionFilePath = System.IO.Path.Combine("InstructionFiles", fileName);
                        await _assessmentRepository.UpdateAsyncNew(_editingAssessment.AssessmentId, _editingAssessment);
                        MessageBox.Show("Tải file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Đang thêm mới → lưu lại path để dùng khi nhấn Lưu
                        selectedFilePath = System.IO.Path.Combine("InstructionFiles", fileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tải lên file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool ValidateInput(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorMessage = "Tên bài kiểm tra không được để trống.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtType.Text))
            {
                errorMessage = "Loại bài kiểm tra không được để trống.";
                return false;
            }

            if (!int.TryParse(txtMaxScore.Text, out int maxScore) || maxScore <= 0)
            {
                errorMessage = "Điểm tối đa phải là số nguyên dương.";
                return false;
            }

            if (dpDueDate.SelectedDate == null)
            {
                errorMessage = "Vui lòng chọn hạn nộp.";
                return false;
            }
            // ✅ Kiểm tra ngày không được trước hôm nay
            if (dpDueDate.SelectedDate.Value.Date < DateTime.Today)
            {
                errorMessage = "Hạn nộp không được là ngày trong quá khứ.";
                return false;
            }

            if (cbCourse.SelectedValue == null)
            {
                errorMessage = "Vui lòng chọn khóa học.";
                return false;
            }

            // Validate định dạng file (nếu có file được chọn)
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                //string[] allowedExtensions = { ".pdf", ".doc", ".docx", ".txt" };
                //string extension = System.IO.Path.GetExtension(selectedFilePath).ToLower();
                //if (!allowedExtensions.Contains(extension))
                //{
                //    errorMessage = "Định dạng file không hợp lệ. Chỉ cho phép: PDF, DOC, DOCX, TXT.";
                //    return false;
                //}

                // Kiểm tra kích thước file (giới hạn 5MB)
                string fullFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, selectedFilePath);
                if (File.Exists(fullFilePath))
                {
                    var fileInfo = new FileInfo(fullFilePath);
                    if (fileInfo.Length > 5 * 1024 * 1024)
                    {
                        errorMessage = "File hướng dẫn vượt quá 5MB.";
                        return false;
                    }
                }
            }

            return true;
        }

    }
}
