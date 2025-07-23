using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace ProjectPRN.Admin.InstructorManagement
{
    public partial class InstructorEditDialog : UserControl
    {
        private readonly InstructorViewModel _originalInstructor;
        private readonly ObservableCollection<string> _validationErrors;


        public InstructorEditDialog(InstructorViewModel instructorToEdit)
        {
            InitializeComponent();

            _originalInstructor = instructorToEdit;
            _validationErrors = new ObservableCollection<string>();

            LoadInstructorData();
        }

        #region Initialization
        private void LoadInstructorData()
        {
            if (_originalInstructor != null)
            {
                // Edit mode
                txtDialogTitle.Text = "Chỉnh sửa giảng viên";
                txtPasswordNote.Visibility = Visibility.Visible;

                txtInstructorName.Text = _originalInstructor.InstructorName;
                txtEmail.Text = _originalInstructor.Email;
                txtPhoneNumber.Text = _originalInstructor.PhoneNumber;
                txtExperience.Text = _originalInstructor.Experience.ToString();

                btnSave.Content = "CẬP NHẬT";
                ValidateForm();
            }
            else
            {
                // Add mode
                txtDialogTitle.Text = "Thêm giảng viên mới";
                txtPasswordNote.Visibility = Visibility.Collapsed;
                btnSave.Content = "THÊM";
            }
        }
        #endregion

        #region Validation
        private void ValidateForm(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void ValidateForm(object sender, RoutedEventArgs e)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            if (_validationErrors == null) return; // Prevent null reference during initialization

            _validationErrors.Clear();

            // Name validation
            if (string.IsNullOrWhiteSpace(txtInstructorName?.Text))
            {
                _validationErrors.Add("• Họ và tên không được để trống");
            }
            else if (txtInstructorName.Text.Length < 2)
            {
                _validationErrors.Add("• Họ và tên phải có ít nhất 2 ký tự");
            }

            // Email validation
            if (string.IsNullOrWhiteSpace(txtEmail?.Text))
            {
                _validationErrors.Add("• Email không được để trống");
            }
            else if (!IsValidEmail(txtEmail.Text))
            {
                _validationErrors.Add("• Email không hợp lệ");
            }

            // Phone validation
            if (string.IsNullOrWhiteSpace(txtPhoneNumber?.Text))
            {
                _validationErrors.Add("• Số điện thoại không được để trống");
            }
            else if (!IsValidPhoneNumber(txtPhoneNumber.Text))
            {
                _validationErrors.Add("• Số điện thoại không hợp lệ (10-11 số)");
            }

            // Experience validation
            if (string.IsNullOrWhiteSpace(txtExperience?.Text))
            {
                _validationErrors.Add("• Số năm kinh nghiệm không được để trống");
            }
            else if (!int.TryParse(txtExperience.Text, out int experience))
            {
                _validationErrors.Add("• Số năm kinh nghiệm phải là số nguyên");
            }
            else if (experience < 0)
            {
                _validationErrors.Add("• Số năm kinh nghiệm không thể âm");
            }
            else if (experience > 50)
            {
                _validationErrors.Add("• Số năm kinh nghiệm không được vượt quá 50");
            }

            // Password validation
            if (_originalInstructor == null)
            {
                // For new instructors - password is required
                if (string.IsNullOrWhiteSpace(txtPassword?.Password))
                {
                    _validationErrors.Add("• Mật khẩu không được để trống khi tạo giảng viên mới");
                }
                else if (!IsStrongPassword(txtPassword.Password))
                {
                    _validationErrors.Add("• Mật khẩu phải có ít nhất 6 ký tự, ít nhất 1 ký tự in hoa, 1 ký tự in thường, và 1 chữ số.");
                }
            }
            else
            {
                // For existing instructors - password is optional, but if provided must be strong
                if (!string.IsNullOrWhiteSpace(txtPassword?.Password) && !IsStrongPassword(txtPassword.Password))
                {
                    _validationErrors.Add("• Mật khẩu phải có ít nhất 6 ký tự, ít nhất 1 ký tự in hoa, 1 ký tự in thường, và 1 chữ số.");
                }
            }

            UpdateValidationUI();
        }

        private bool IsStrongPassword(string password)
        {
            // Check for at least one uppercase, one lowercase, and one digit
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            return hasUpper && hasLower && hasDigit;
        }

        private void UpdateValidationUI()
        {
            if (icValidationErrors.ItemsSource == null)
            {
                icValidationErrors.ItemsSource = _validationErrors;
            }

            if (_validationErrors.Any())
            {
                borderValidation.Visibility = Visibility.Visible;
                btnSave.IsEnabled = false;
            }
            else
            {
                borderValidation.Visibility = Visibility.Collapsed;
                btnSave.IsEnabled = true;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phone)
        {
            var phoneRegex = new Regex(@"^[0-9]{10,11}$");
            return phoneRegex.IsMatch(phone);
        }
        #endregion

        #region Input Validation
        private void PhoneValidationTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers
            var regex = new Regex(@"^[0-9]*$");
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
                var instructor = CreateInstructorFromForm();

                // Close dialog with the instructor object as result
                DialogHost.CloseDialogCommand.Execute(instructor, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu giảng viên: {ex.Message}",
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
        private InstructorViewModel CreateInstructorFromForm()
        {
            var instructor = new InstructorViewModel();

            // Copy ID if editing existing instructor
            if (_originalInstructor != null)
            {
                instructor.InstructorId = _originalInstructor.InstructorId;
            }

            instructor.InstructorName = txtInstructorName.Text.Trim();
            instructor.Email = txtEmail.Text.Trim();
            instructor.PhoneNumber = txtPhoneNumber.Text.Trim();
            instructor.Experience = int.Parse(txtExperience.Text);

            // Password handling
            if (!string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                instructor.Password = txtPassword.Password;
            }
            else if (_originalInstructor == null)
            {
                instructor.Password = "DefaultPassword123!"; // Default for new instructors
            }

            return instructor;
        }
        #endregion
    }
}