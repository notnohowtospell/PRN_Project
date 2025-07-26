using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using ProjectPRN.Utils;

namespace ProjectPRN.Admin.CourseManagement
{
    public partial class CourseManagementView : Window, INotifyPropertyChanged
    {
        private readonly PresentationDbContext _context;

        public ObservableCollection<LifeSkillCourse> FilteredCourses { get; set; }

        private LifeSkillCourse _selectedCourse;
        public LifeSkillCourse SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                _selectedCourse = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public CourseManagementView()
        {
            InitializeComponent();
            _context = new PresentationDbContext();

            FilteredCourses = new ObservableCollection<LifeSkillCourse>();

            InitializeData();
        }

        #region Data Loading
        private void InitializeData()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tải dữ liệu...";

                LoadInstructors();
                LoadData();
                UpdateUI();

                txtStatus.Text = "Sẵn sàng";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi tải dữ liệu: {ex.Message}";
                MessageBox.Show($"Không thể tải dữ liệu: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadData()
        {
            try
            {
                var gotCourses = _context.LifeSkillCourses.ToList();
                dgCourses.ItemsSource = gotCourses;

                FilteredCourses.Clear();
                foreach (var course in gotCourses)
                {
                    FilteredCourses.Add(course);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách khóa học: {ex.Message}", ex);
            }
        }

        private void LoadInstructors()
        {
            try
            {
                MessageBox.Show("Start load instructor");

                var instructors = _context.Instructors.ToList();
                MessageBox.Show("Instructor loaded");
                cmbInstructorFilter.Items.Clear();
                cmbInstructorFilter.Items.Add(new ComboBoxItem { Content = "Tất cả giảng viên", Tag = -1 });

                foreach (var instructor in instructors)
                {
                    cmbInstructorFilter.Items.Add(new ComboBoxItem
                    {
                        Content = instructor.InstructorName,
                        Tag = instructor.InstructorId
                    });
                }

                cmbInstructorFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách giảng viên: {ex.Message}", ex);
            }
        }

        private void RefreshData()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang làm mới dữ liệu...";

                LoadData();
                UpdateUI();

                txtStatus.Text = "Đã làm mới dữ liệu thành công";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi làm mới dữ liệu: {ex.Message}";
                MessageBox.Show($"Không thể làm mới dữ liệu: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Event Handlers
        private void BtnAddCourse_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;
            ShowCourseDialog(null);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;

            var button = sender as Button;
            if (button?.Tag is int courseId)
            {
                try
                {
                    var course = _context.LifeSkillCourses.FirstOrDefault(c => c.CourseId == courseId);
                    if (course != null)
                    {
                        ShowCourseDialog(course);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy khóa học.", "Thông báo",
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải thông tin khóa học: {ex.Message}", "Lỗi",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;

            var button = sender as Button;
            if (button?.Tag is int courseId)
            {
                var course = _context.LifeSkillCourses.FirstOrDefault(c => c.CourseId == courseId);
                if (course != null)
                {
                    var result = await ShowConfirmDialog(
                        "Xác nhận xóa",
                        $"Bạn có chắc chắn muốn xóa khóa học '{course.CourseName}'?\nHành động này không thể hoàn tác.");

                    if (result)
                    {
                        DeleteCourse(course);
                    }
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbInstructorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void DgCourses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedCourse = dgCourses.SelectedItem as LifeSkillCourse;
        }
        #endregion

        #region CRUD Operations
        private void CreateCourse(LifeSkillCourse course)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tạo khóa học...";

                _context.LifeSkillCourses.Add(course);
                _context.SaveChanges();

                RefreshData();

                txtStatus.Text = $"Đã tạo khóa học '{course.CourseName}' thành công.";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi tạo khóa học: {ex.Message}";
                MessageBox.Show($"Không thể tạo khóa học: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateCourse(LifeSkillCourse updatedCourse)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang cập nhật khóa học...";

                var existingCourse = _context.LifeSkillCourses.FirstOrDefault(c => c.CourseId == updatedCourse.CourseId);
                if (existingCourse != null)
                {
                    // Update properties
                    existingCourse.CourseName = updatedCourse.CourseName;
                    existingCourse.InstructorId = updatedCourse.InstructorId;
                    existingCourse.StartDate = updatedCourse.StartDate;
                    existingCourse.EndDate = updatedCourse.EndDate;
                    existingCourse.Description = updatedCourse.Description;
                    existingCourse.MaxStudents = updatedCourse.MaxStudents;
                    existingCourse.Price = updatedCourse.Price;
                    existingCourse.Status = updatedCourse.Status;

                    _context.SaveChanges();
                }

                RefreshData();

                txtStatus.Text = $"Đã cập nhật khóa học '{updatedCourse.CourseName}' thành công.";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi cập nhật khóa học: {ex.Message}";
                MessageBox.Show($"Không thể cập nhật khóa học: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void DeleteCourse(LifeSkillCourse course)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang xóa khóa học...";

                var courseName = course?.CourseName ?? "Unknown";

                _context.LifeSkillCourses.Remove(course);
                _context.SaveChanges();

                RefreshData();

                txtStatus.Text = $"Đã xóa khóa học '{courseName}' thành công.";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi xóa khóa học: {ex.Message}";
                MessageBox.Show($"Không thể xóa khóa học: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Filtering and UI Updates
        private void ApplyFilter()
        {
            var filtered = _context.LifeSkillCourses.ToList().AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                var searchTerm = txtSearch.Text.Trim().ToLower();
                filtered = filtered.Where(c =>
                    c.CourseName?.ToLower().Contains(searchTerm) == true ||
                    c.Description?.ToLower().Contains(searchTerm) == true ||
                    c.Instructor?.InstructorName?.ToLower().Contains(searchTerm) == true);
            }

            // Status filter
            if (cmbStatusFilter?.SelectedItem is ComboBoxItem statusItem &&
                statusItem.Content.ToString() != "Tất cả")
            {
                var status = statusItem.Content.ToString();
                filtered = filtered.Where(c => c.Status == status);
            }

            // Instructor filter
            if (cmbInstructorFilter?.SelectedItem is ComboBoxItem instructorItem &&
                instructorItem.Tag is int instructorId)
            {
                if (instructorId != -1)
                {
                    filtered = filtered.Where(c => c.InstructorId == instructorId);
                }
            }

            FilteredCourses.Clear();
            foreach (var course in filtered)
            {
                FilteredCourses.Add(course);
            }

            dgCourses.ItemsSource = FilteredCourses;

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (txtTotalCourses != null)
            {
                txtTotalCourses.Text = FilteredCourses?.Count.ToString() ?? "0";
            }
        }
        #endregion

        #region Dialog Methods
        private async void ShowCourseDialog(LifeSkillCourse courseToEdit)
        {
            try
            {
                var dialog = new CourseEditDialog(courseToEdit, _context.Instructors.ToList());
                var result = await DialogHost.Show(dialog, "RootDialog");

                if (result is LifeSkillCourse course)
                {
                    if (courseToEdit == null)
                    {
                        CreateCourse(course);
                    }
                    else
                    {
                        UpdateCourse(course);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị dialog: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> ShowConfirmDialog(string title, string message)
        {
            try
            {
                var dialog = new ConfirmDialog(title, message);
                var result = await DialogHost.Show(dialog, "RootDialog");
                return result is bool confirm && confirm;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị dialog xác nhận: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        #endregion

        #region Public Methods
        public void Refresh()
        {
            RefreshData();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbInstructorFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0;
            ApplyFilter();
        }

        // Dispose pattern to properly dispose of DbContext
        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}