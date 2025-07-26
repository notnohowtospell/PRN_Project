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
using Microsoft.Win32;
using Path = System.IO.Path;

namespace ProjectPRN.CourseMaterialManagement
{
    /// <summary>
    /// Interaction logic for CourseMaterialInputWindow.xaml
    /// </summary>
    public partial class CourseMaterialInputWindow : Window
    {
        public CourseMaterial Material { get; private set; } = new();
        private readonly string _uploadFolder = "UploadMaterials";

        public CourseMaterialInputWindow(List<LifeSkillCourse> courses, CourseMaterial? material = null)
        {
            InitializeComponent();
            CourseComboBox.ItemsSource = courses;

            if (material != null)
            {
                Material = new CourseMaterial
                {
                    MaterialId = material.MaterialId,
                    CourseId = material.CourseId,
                    Title = material.Title,
                    FilePath = material.FilePath,
                    UploadDate = material.UploadDate
                };

                CourseComboBox.SelectedValue = Material.CourseId;
                TitleTextBox.Text = Material.Title;
                FilePathTextBox.Text = Material.FilePath;
            }
        }

        private void ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Chọn file tài liệu";
            dialog.Filter = "All Files (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                // Lấy thông tin file gốc
                string selectedFile = dialog.FileName;
                string extension = Path.GetExtension(selectedFile);

                // Lấy thông tin từ form
                if (CourseComboBox.SelectedItem is LifeSkillCourse selectedCourse && !string.IsNullOrWhiteSpace(TitleTextBox.Text))
                {
                    string fileName = $"Material_{selectedCourse.CourseName}_{TitleTextBox.Text}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                    string uploadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _uploadFolder);
                    Directory.CreateDirectory(uploadFolder); // Tạo thư mục nếu chưa có

                    string destPath = Path.Combine(uploadFolder, fileName);
                    File.Copy(selectedFile, destPath, true);

                    // Lưu đường dẫn tương đối
                    string relativePath = Path.Combine(_uploadFolder, fileName);
                    FilePathTextBox.Text = relativePath;
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn khóa học và nhập tiêu đề trước khi chọn file.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (CourseComboBox.SelectedValue == null ||
                string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                string.IsNullOrWhiteSpace(FilePathTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            Material.CourseId = (int)CourseComboBox.SelectedValue;
            Material.Title = TitleTextBox.Text.Trim();
            Material.FilePath = FilePathTextBox.Text.Trim();
            Material.UploadDate = DateTime.Now; // ✅ Cập nhật ngày upload mới nhất

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
