using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BusinessObjects.Models;
using MaterialDesignThemes.Wpf;

namespace ProjectPRN.Admin.CourseManagement
{
    public partial class CourseEditDialog : UserControl
    {
        private readonly LifeSkillCourse _originalCourse;
        private readonly List<Instructor> _instructors;
        private readonly List<string> _validationErrors;


        public CourseEditDialog(LifeSkillCourse courseToEdit, List<Instructor> instructors)
        {
            InitializeComponent();

            _originalCourse = courseToEdit;
            _instructors = instructors;
            _validationErrors = new List<string>();

            LoadInstructors();
            LoadCourseData();
        }

        #region Initialization
        private void LoadInstructors()
        {
            cmbInstructor.ItemsSource = _instructors;
            if (_instructors.Any())
            {
                cmbInstructor.SelectedIndex = 0;
            }
        }

        private void LoadCourseData()
        {
            if (_originalCourse != null)
            {
                // Edit mode
                txtDialogTitle.Text = "Chỉnh sửa khóa học";

                txtCourseName.Text = _originalCourse.CourseName;
                cmbInstructor.SelectedValue = _originalCourse.InstructorId;
                dpStartDate.SelectedDate = _originalCourse.StartDate;
                dpEndDate.SelectedDate = _originalCourse.EndDate;
                txtDescription.Text = _originalCourse.Description;
                txtMaxStudents.Text = _originalCourse.MaxStudents?.ToString();
                txtPrice.Text = _originalCourse.Price?.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));

                // Set status
                var statusItems = cmbStatus.Items.Cast<ComboBoxItem>().ToList();
                var statusItem = statusItems.FirstOrDefault(item =>
                    item.Content.ToString().Equals(_originalCourse.Status, StringComparison.Ordinal));

                if (statusItem != null)
                {
                    cmbStatus.SelectedItem = statusItem;
                }

                btnSave.Content = "CẬP NHẬT";
            }
            else
            {
                // Add mode
                txtDialogTitle.Text = "Thêm khóa học mới";
                cmbStatus.SelectedIndex = 0; // Default to "Đang mở"
                dpStartDate.SelectedDate = DateTime.Now.AddDays(7);
                dpEndDate.SelectedDate = DateTime.Now.AddDays(37);
                btnSave.Content = "THÊM";
            }
        }
        #endregion

        #region Validation
        private void ValidateForm(object sender = null, EventArgs e = null)
        {
            _validationErrors.Clear();

            // Course name validation
            if (string.IsNullOrWhiteSpace(txtCourseName.Text))
            {
                _validationErrors.Add("• Tên khóa học không được để trống");
            }
            else if (txtCourseName.Text.Length < 3)
            {
                _validationErrors.Add("• Tên khóa học phải có ít nhất 3 ký tự");
            }

            // Instructor validation
            if (cmbInstructor.SelectedValue == null)
            {
                _validationErrors.Add("• Vui lòng chọn giảng viên");
            }

            // Date validation
            if (!dpStartDate.SelectedDate.HasValue)
            {
                _validationErrors.Add("• Vui lòng chọn ngày bắt đầu");
            }

            if (!dpEndDate.SelectedDate.HasValue)
            {
                _validationErrors.Add("• Vui lòng chọn ngày kết thúc");
            }

            if (dpStartDate.SelectedDate.HasValue && dpEndDate.SelectedDate.HasValue)
            {
                if (dpEndDate.SelectedDate <= dpStartDate.SelectedDate)
                {
                    _validationErrors.Add("• Ngày kết thúc phải sau ngày bắt đầu");
                }

                if (dpStartDate.SelectedDate < DateTime.Now.Date && _originalCourse == null)
                {
                    _validationErrors.Add("• Ngày bắt đầu không thể là ngày trong quá khứ");
                }
            }

            // Status validation
            if (cmbStatus.SelectedItem == null)
            {
                _validationErrors.Add("• Vui lòng chọn trạng thái");
            }

            // Price validation
            if (!string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                if (!decimal.TryParse(txtPrice.Text.Replace(",", "").Replace(".", ""), out decimal price))
                {
                    _validationErrors.Add("• Học phí phải là số hợp lệ");
                }
                else if (price < 0)
                {
                    _validationErrors.Add("• Học phí không thể âm");
                }
            }

            // Max students validation
            if (!string.IsNullOrWhiteSpace(txtMaxStudents.Text))
            {
                if (!int.TryParse(txtMaxStudents.Text, out int maxStudents))
                {
                    _validationErrors.Add("• Số lượng tối đa học viên phải là số nguyên");
                }
                else if (maxStudents <= 0)
                {
                    _validationErrors.Add("• Số lượng tối đa học viên phải lớn hơn 0");
                }
                else if (maxStudents > 100)
                {
                    _validationErrors.Add("• Số lượng tối đa học viên không được vượt quá 100");
                }
            }

            UpdateValidationUI();
        }

        private void UpdateValidationUI()
        {
            if (_validationErrors.Any())
            {
                borderValidation.Visibility = Visibility.Visible;
                icValidationErrors.ItemsSource = _validationErrors;
                btnSave.IsEnabled = false;
            }
            else
            {
                borderValidation.Visibility = Visibility.Collapsed;
                btnSave.IsEnabled = true;
            }
        }
        #endregion

        #region Input Validation
        private void NumberValidationTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers and one decimal point
            var regex = new Regex(@"^[0-9,\.]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void IntegerValidationTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only integers
            var regex = new Regex(@"^[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        #endregion

        #region Event Handlers
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!btnSave.IsEnabled) return;

            try
            {
                var course = CreateCourseFromForm();

                // Close dialog with the course object as result
                DialogHost.CloseDialogCommand.Execute(course, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu khóa học: {ex.Message}",
                               "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, this);
        }
        #endregion

        #region Helper Methods
        private LifeSkillCourse CreateCourseFromForm()
        {
            var course = new LifeSkillCourse();

            // Copy ID if editing existing course
            if (_originalCourse != null)
            {
                course.CourseId = _originalCourse.CourseId;
            }

            course.CourseName = txtCourseName.Text.Trim();
            course.InstructorId = (int)cmbInstructor.SelectedValue;
            course.StartDate = dpStartDate.SelectedDate;
            course.EndDate = dpEndDate.SelectedDate;
            course.Description = txtDescription.Text.Trim();
            course.Status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString();

            // Parse price
            if (!string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                var priceText = txtPrice.Text.Replace(",", "").Replace(".", "");
                if (decimal.TryParse(priceText, out decimal price))
                {
                    course.Price = price;
                }
            }

            // Parse max students
            if (!string.IsNullOrWhiteSpace(txtMaxStudents.Text))
            {
                if (int.TryParse(txtMaxStudents.Text, out int maxStudents))
                {
                    course.MaxStudents = maxStudents;
                }
            }

            return course;
        }
        #endregion
    }
}