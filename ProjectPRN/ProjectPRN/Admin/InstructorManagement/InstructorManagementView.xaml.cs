using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using ProjectPRN.Admin.CourseManagement;
using ProjectPRN.Utils;

namespace ProjectPRN.Admin.InstructorManagement
{
    public partial class InstructorManagementView : Window, INotifyPropertyChanged
    {
        private readonly ApplicationDbContext _context;
        public ObservableCollection<InstructorViewModel> FilteredInstructors { get; set; }

        private InstructorViewModel _selectedInstructor;
        public InstructorViewModel SelectedInstructor
        {
            get => _selectedInstructor;
            set
            {
                _selectedInstructor = value;
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

        public InstructorManagementView()
        {
            InitializeComponent();
            DataContext = this;
            _context = new ApplicationDbContext();

            FilteredInstructors = new ObservableCollection<InstructorViewModel>();

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

        private void LoadInstructors()
        {
            var instructors = _context.Instructors
                .Include(i => i.LifeSkillCourses)
                .ToList();

            FilteredInstructors.Clear();

            foreach (var instructor in instructors)
            {
                FilteredInstructors.Add(new InstructorViewModel
                {
                    InstructorId = instructor.InstructorId,
                    InstructorName = instructor.InstructorName,
                    Email = instructor.Email,
                    PhoneNumber = instructor.PhoneNumber,
                    Experience = instructor.Experience,
                    //LastLogin = instructor.LastLogin,
                    CourseCount = instructor.LifeSkillCourses?.Count ?? 0,
                    //Password = instructor.Password
                });
            }

            dgInstructors.ItemsSource = FilteredInstructors;
        }
        #endregion

        #region Event Handlers
        private async void BtnAddInstructor_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;
            await ShowInstructorDialogAsync(null);
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;

            var button = sender as Button;
            if (button?.Tag is int instructorId)
            {
                var instructor = FilteredInstructors.FirstOrDefault(i => i.InstructorId == instructorId);
                if (instructor != null)
                {
                    await ShowInstructorDialogAsync(instructor);
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoading) return;

            var button = sender as Button;
            if (button?.Tag is int instructorId)
            {
                var instructor = FilteredInstructors.FirstOrDefault(i => i.InstructorId == instructorId);
                if (instructor != null)
                {
                    var result = await ShowConfirmDialogAsync(
                        "Xác nhận xóa",
                        $"Bạn có chắc chắn muốn xóa giảng viên '{instructor.InstructorName}'?\n\n" +
                        $"Giảng viên này đang có {instructor.CourseCount} khóa học.\n" +
                        "Hành động này không thể hoàn tác.");

                    if (result)
                    {
                        await DeleteInstructorAsync(instructor);
                    }
                }
            }
        }

        private async void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int instructorId)
            {
                var instructor = FilteredInstructors.FirstOrDefault(i => i.InstructorId == instructorId);
                if (instructor != null)
                {
                    await ShowInstructorDetailsDialogAsync(instructor);
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbExperienceFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbExperienceFilter.SelectedIndex = 0;
            ApplyFilter();
        }

        private void DgInstructors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedInstructor = dgInstructors.SelectedItem as InstructorViewModel;
        }
        #endregion

        #region CRUD Operations
        private async Task CreateInstructorAsync(InstructorViewModel instructorViewModel)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang tạo giảng viên...";

                var instructor = new Instructor
                {
                    InstructorName = instructorViewModel.InstructorName,
                    Email = instructorViewModel.Email,
                    PhoneNumber = instructorViewModel.PhoneNumber,
                    Experience = instructorViewModel.Experience,
                    //Password = instructorViewModel.Password ?? "DefaultPassword123!",
                    //LastLogin = null
                };

                await _context.Instructors.AddAsync(instructor);
                await _context.SaveChangesAsync();

                LoadInstructors();
                UpdateUI();

                txtStatus.Text = $"Đã tạo giảng viên '{instructor.InstructorName}' thành công.";
                MessageBox.Show("Tạo giảng viên thành công!", "Thành công",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi tạo giảng viên: {ex.Message}";
                MessageBox.Show($"Không thể tạo giảng viên: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateInstructorAsync(InstructorViewModel instructorViewModel)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang cập nhật giảng viên...";

                var instructor = await _context.Instructors.FindAsync(instructorViewModel.InstructorId);
                if (instructor != null)
                {
                    instructor.InstructorName = instructorViewModel.InstructorName;
                    instructor.Email = instructorViewModel.Email;
                    instructor.PhoneNumber = instructorViewModel.PhoneNumber;
                    instructor.Experience = instructorViewModel.Experience;

                    // Only update password if it's provided
                    if (!string.IsNullOrWhiteSpace(instructorViewModel.Password))
                    {
                        //instructor.Password = PasswordHasher.HashPassword(instructorViewModel.Password);
                    }

                    await _context.SaveChangesAsync();

                    LoadInstructors();
                    UpdateUI();

                    txtStatus.Text = $"Đã cập nhật giảng viên '{instructor.InstructorName}' thành công.";
                    MessageBox.Show("Cập nhật giảng viên thành công!", "Thành công",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi cập nhật giảng viên: {ex.Message}";
                MessageBox.Show($"Không thể cập nhật giảng viên: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteInstructorAsync(InstructorViewModel instructorViewModel)
        {
            try
            {
                IsLoading = true;
                txtStatus.Text = "Đang xóa giảng viên...";

                var instructor = await _context.Instructors
                    .Include(i => i.LifeSkillCourses)
                    .FirstOrDefaultAsync(i => i.InstructorId == instructorViewModel.InstructorId);

                if (instructor != null)
                {
                    // Check if instructor has active courses
                    if (instructor.LifeSkillCourses.Any(c => c.Status == "Mở đăng ký"))
                    {
                        MessageBox.Show("Không thể xóa giảng viên đang có khóa học đang mở đăng ký.\nVui lòng đóng tất cả khóa học trước khi xóa.",
                                       "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _context.Instructors.Remove(instructor);
                    await _context.SaveChangesAsync();

                    LoadInstructors();
                    UpdateUI();

                    txtStatus.Text = $"Đã xóa giảng viên '{instructor.InstructorName}' thành công.";
                    MessageBox.Show("Xóa giảng viên thành công!", "Thành công",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Lỗi khi xóa giảng viên: {ex.Message}";
                MessageBox.Show($"Không thể xóa giảng viên: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Filtering
        private void ApplyFilter()
        {
            var query = _context.Instructors.Include(i => i.LifeSkillCourses).AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                var searchTerm = txtSearch.Text.Trim().ToLower();
                query = query.Where(i =>
                    i.InstructorName.ToLower().Contains(searchTerm) ||
                    i.Email.ToLower().Contains(searchTerm) ||
                    i.PhoneNumber.ToLower().Contains(searchTerm));
            }

            // Experience filter
            if (cmbExperienceFilter?.SelectedItem is ComboBoxItem experienceItem &&
                experienceItem.Content.ToString() != "Tất cả")
            {
                var experience = experienceItem.Content.ToString();
                query = experience switch
                {
                    "Dưới 2 năm" => query.Where(i => i.Experience < 2),
                    "2-5 năm" => query.Where(i => i.Experience >= 2 && i.Experience <= 5),
                    "5-10 năm" => query.Where(i => i.Experience > 5 && i.Experience <= 10),
                    "Trên 10 năm" => query.Where(i => i.Experience > 10),
                    _ => query
                };
            }

            var filteredInstructors = query.ToList();
            RefreshFilteredInstructors(filteredInstructors);
        }

        private void RefreshFilteredInstructors(IEnumerable<Instructor> instructors)
        {
            FilteredInstructors.Clear();

            foreach (var instructor in instructors)
            {
                FilteredInstructors.Add(new InstructorViewModel
                {
                    InstructorId = instructor.InstructorId,
                    InstructorName = instructor.InstructorName,
                    Email = instructor.Email,
                    PhoneNumber = instructor.PhoneNumber,
                    Experience = instructor.Experience,
                    //LastLogin = instructor.LastLogin,
                    CourseCount = instructor.LifeSkillCourses?.Count ?? 0,
                    //Password = instructor.Password
                });
            }

            UpdateUI();
        }
        #endregion

        #region Dialog Methods
        private async Task ShowInstructorDialogAsync(InstructorViewModel instructorToEdit)
        {
            try
            {
                var dialog = new InstructorEditDialog(instructorToEdit);
                var result = await DialogHost.Show(dialog, "RootDialog");

                if (result is InstructorViewModel instructor)
                {
                    if (instructorToEdit == null)
                    {
                        await CreateInstructorAsync(instructor);
                    }
                    else
                    {
                        await UpdateInstructorAsync(instructor);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị dialog: {ex.Message}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ShowInstructorDetailsDialogAsync(InstructorViewModel instructor)
        {
            try
            {
                var dialog = new InstructorDetailsDialog(instructor);
                await DialogHost.Show(dialog, "RootDialog");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị chi tiết giảng viên: {ex.Message}", "Lỗi",
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

        #region Helper Methods
        private void UpdateUI()
        {
            if (txtTotalInstructors != null)
            {
                txtTotalInstructors.Text = FilteredInstructors?.Count.ToString() ?? "0";
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    // View Model
    public class InstructorViewModel
    {
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int Experience { get; set; }
        public DateTime? LastLogin { get; set; }
        public int CourseCount { get; set; }
        public string? Password { get; set; }
    }
}