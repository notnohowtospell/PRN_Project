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
using Microsoft.Win32;
using Path = System.IO.Path;
using System.IO;

namespace ProjectPRN.CourseMaterialManagement
{
    /// <summary>
    /// Interaction logic for CourseMaterialManagementWindow.xaml
    /// </summary>
    public partial class CourseMaterialManagementWindow : Window
    {
        private readonly ICourseMaterialDAO _materialDAO = new CourseMaterialDAO();
        private readonly ILifeSkillCourseDAO _courseDAO = new LifeSkillCourseDAO();
        private readonly bool _isStudent;

        private List<CourseMaterial> _allMaterials = new();
        private List<LifeSkillCourse> _allCourses = new();
        public CourseMaterialManagementWindow(bool isStudent)
        {
            InitializeComponent();
            _isStudent = isStudent;

            ConfigureUIByRole();
            LoadCourses();
            LoadMaterials();
        }
        private void ConfigureUIByRole()
        {
            if (_isStudent)
            {
                AddButton.Visibility = Visibility.Collapsed;
                EditButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }
        private async void LoadCourses()
        {
            _allCourses = (await _courseDAO.GetAllAsync()).ToList();
            CourseFilterComboBox.ItemsSource = _allCourses;
        }

        private async void LoadMaterials()
        {
            _allMaterials = (await _materialDAO.GetAllAsync()).ToList();
            FilterAndDisplay();
        }

        private void FilterAndDisplay()
        {
            var filtered = _allMaterials.AsEnumerable();

            if (CourseFilterComboBox.SelectedValue is int selectedCourseId)
            {
                filtered = filtered.Where(m => m.CourseId == selectedCourseId);
            }

            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string keyword = SearchTextBox.Text.Trim().ToLower();
                filtered = filtered.Where(m => m.Title.ToLower().Contains(keyword));
            }

            CourseMaterialDataGrid.ItemsSource = filtered.ToList();
        }

        private void CourseFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterAndDisplay();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAndDisplay();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new CourseMaterialInputWindow(_allCourses);
            if (window.ShowDialog() == true)
            {
                var newMaterial = window.Material;

                // Không cần upload lại ở đây, vì đã upload trong cửa sổ nhập
                newMaterial.UploadDate = DateTime.Now;
                await _materialDAO.AddAsync(newMaterial);
                LoadMaterials();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (CourseMaterialDataGrid.SelectedItem is CourseMaterial selected)
            {
                var window = new CourseMaterialInputWindow(_allCourses, selected);
                if (window.ShowDialog() == true)
                {
                    var updated = window.Material;
                    await _materialDAO.UpdateAsync(updated);
                    LoadMaterials();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tài liệu để sửa.");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CourseMaterialDataGrid.SelectedItem is CourseMaterial selected)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa tài liệu '{selected.Title}'?", "Xác nhận", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    await _materialDAO.DeleteAsync(selected.MaterialId);
                    LoadMaterials();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tài liệu để xóa.");
            }
        }


        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (CourseMaterialDataGrid.SelectedItem is CourseMaterial selected)
            {
                DownloadFile(selected.FilePath); // bạn đã có logic sẵn
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tài liệu để tải xuống.");
            }
        }

        private void DownloadFile(string relativePath)
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
                if (!File.Exists(fullPath))
                {
                    MessageBox.Show("Tệp không tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string originalExtension = Path.GetExtension(fullPath);
                string fileName = Path.GetFileName(fullPath);

                SaveFileDialog saveDlg = new SaveFileDialog
                {
                    Title = "Lưu tệp tài liệu",
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
                MessageBox.Show("Lỗi khi tải tệp: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
