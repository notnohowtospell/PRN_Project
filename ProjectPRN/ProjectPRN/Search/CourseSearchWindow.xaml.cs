using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using ProjectPRN.Admin.CourseManagement;
using Repositories.Interfaces;

namespace ProjectPRN.Search
{
    /// <summary>
    /// Interaction logic for CourseSearchWindow.xaml
    /// </summary>
    public partial class CourseSearchWindow : Window, INotifyPropertyChanged
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly ILifeSkillCourseRepository _lifeSkillCourseRepository;

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

        public CourseSearchWindow(ILifeSkillCourseRepository lifeSkillCourseRepository, IInstructorRepository instructorRepository)
        {
            InitializeComponent();
            _lifeSkillCourseRepository = lifeSkillCourseRepository;
            _instructorRepository = instructorRepository;

            FilteredCourses = new ObservableCollection<LifeSkillCourse>();

            _ = InitializeDataAsync();
        }

        #region Data Loading
        private async Task InitializeDataAsync()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tải dữ liệu...";

                await LoadInstructorsAsync();
                await LoadDataAsync();
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

        private async Task LoadDataAsync()
        {
            try
            {
                var gotCourses = await _lifeSkillCourseRepository.GetAllAsync();
                dgCourses.ItemsSource = gotCourses;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải danh sách khóa học: {ex.Message}", ex);
            }
        }

        private async Task LoadInstructorsAsync()
        {
            try
            {
                var instructors = await _instructorRepository.GetAllAsync();

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

        private async Task RefreshDataAsync()
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang làm mới dữ liệu...";

                await LoadDataAsync();
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
        private async void BtnAddCourse_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;
            await ShowCourseDialogAsync(null);
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;

            var button = sender as Button;
            if (button?.Tag is int courseId)
            {
                try
                {
                    var course = await _lifeSkillCourseRepository.GetByIdAsync(courseId);
                    if (course != null)
                    {
                        await ShowCourseDialogAsync(course);
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
                var course = await _lifeSkillCourseRepository.GetByIdAsync(courseId);
                if (course != null)
                {
                    var result = await ShowConfirmDialogAsync(
                        "Xác nhận xóa",
                        $"Bạn có chắc chắn muốn xóa khóa học '{course.CourseName}'?\nHành động này không thể hoàn tác.");

                    if (result)
                    {
                        await DeleteCourseAsync(course);
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
        private async Task CreateCourseAsync(LifeSkillCourse course)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tạo khóa học...";

                await _lifeSkillCourseRepository.AddAsync(course);
                await _lifeSkillCourseRepository.SaveChangesAsync();

                await RefreshDataAsync();

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

        private async Task UpdateCourseAsync(LifeSkillCourse updatedCourse)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang cập nhật khóa học...";

                await _lifeSkillCourseRepository.UpdateAsync(updatedCourse);
                await _lifeSkillCourseRepository.SaveChangesAsync();

                await RefreshDataAsync();

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

        private async Task DeleteCourseAsync(LifeSkillCourse course)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang xóa khóa học...";

                var courseName = course?.CourseName ?? "Unknown";

                await _lifeSkillCourseRepository.DeleteAsync(course.CourseId);
                await _lifeSkillCourseRepository.SaveChangesAsync();

                await RefreshDataAsync();

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
            var filtered = _lifeSkillCourseRepository.GetAll().AsEnumerable();

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
        private async Task ShowCourseDialogAsync(LifeSkillCourse courseToEdit)
        {
            try
            {
                var dialog = new CourseEditDialog(courseToEdit, _instructorRepository.GetAll().ToList());
                var result = await DialogHost.Show(dialog, "RootDialog");

                if (result is LifeSkillCourse course)
                {
                    if (courseToEdit == null)
                    {
                        await CreateCourseAsync(course);
                    }
                    else
                    {
                        await UpdateCourseAsync(course);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị dialog: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> ShowConfirmDialogAsync(string title, string message)
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
        public async Task RefreshAsync()
        {
            await RefreshDataAsync();
        }

        //public async Task LoadCoursesByStatusAsync(string status)
        //{
        //    try
        //    {
        //        IsLoading = true;
        //        txtStatus.Text = $"Đang tải khóa học với trạng thái '{status}'...";

        //        var courses = await _lifeSkillCourseRepository.GetByStatusAsync(status);

        //        ApplyFilter();
        //        UpdateUI();

        //        txtStatus.Text = $"Đã tải {courses.Count()} khóa học với trạng thái '{status}'";
        //    }
        //    catch (Exception ex)
        //    {
        //        txtStatus.Text = $"Lỗi khi tải khóa học theo trạng thái: {ex.Message}";
        //        MessageBox.Show($"Không thể tải khóa học theo trạng thái: {ex.Message}", "Lỗi",
        //                       MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    finally
        //    {
        //        IsLoading = false;
        //    }
        //}

        //public async Task LoadCoursesByInstructorAsync(int instructorId)
        //{
        //    try
        //    {
        //        IsLoading = true;
        //        txtStatus.Text = "Đang tải khóa học theo giảng viên...";

        //        var courses = await _lifeSkillCourseRepository.GetByInstructorAsync(instructorId);

        //        ApplyFilter();
        //        UpdateUI();

        //        var instructor = Instructors.FirstOrDefault(i => i.InstructorId == instructorId);
        //        txtStatus.Text = $"Đã tải {courses.Count()} khóa học của giảng viên '{instructor?.InstructorName}'";
        //    }
        //    catch (Exception ex)
        //    {
        //        txtStatus.Text = $"Lỗi khi tải khóa học theo giảng viên: {ex.Message}";
        //        MessageBox.Show($"Không thể tải khóa học theo giảng viên: {ex.Message}", "Lỗi",
        //                       MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    finally
        //    {
        //        IsLoading = false;
        //    }
        //}
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
            ApplyFilter();
        }
    }
}
