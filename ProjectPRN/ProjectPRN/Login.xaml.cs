using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataAccessObjects;
using ProjectPRN.Utils;
using System.Security.Cryptography;
using System.Text;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using ProjectPRN.Admin;

namespace ProjectPRN
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private readonly IStudentDAO _studentDAO;
        private readonly IInstructorDAO _instructorDAO;
        public bool LoginSuccessful { get; private set; } = false;
        public string UserType { get; private set; } = "";

        public Login()
        {
            InitializeComponent();
            _studentDAO = new StudentDAO();
            _instructorDAO = new InstructorDAO();

            // Load any saved login preferences
            LoadLoginPreferences();
        }

        private async void LoadLoginPreferences()
        {
            try
            {
                // Load last used email if available
                var lastEmail = await StateManager.GetUserPreferenceAsync<string>("LastLoginEmail");
                if (!string.IsNullOrEmpty(lastEmail))
                {
                    EmailTextBox.Text = lastEmail;
                }

                // Load remember me preference
                var rememberMe = await StateManager.GetUserPreferenceAsync<bool>("RememberMe");
                RememberMeCheckBox.IsChecked = rememberMe;

                // Load saved password if remember me is enabled
                if (rememberMe)
                {
                    var savedPassword = await StateManager.GetUserPreferenceAsync<string>("SavedPassword");
                    if (!string.IsNullOrEmpty(savedPassword))
                    {
                        // Decrypt and set password
                        var decryptedPassword = DecryptPassword(savedPassword);
                        if (!string.IsNullOrEmpty(decryptedPassword))
                        {
                            PasswordBox.Password = decryptedPassword;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load login preferences: {ex.Message}");
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable login button during processing
                LoginButton.IsEnabled = false;
                LoginButton.Content = "SIGNING IN...";

                // Get input values
                string email = EmailTextBox.Text.Trim();
                string password = PasswordBox.Password;
                bool rememberMe = RememberMeCheckBox.IsChecked ?? false;

                // Basic validation
                if (!ValidateLoginInput(email, password))
                {
                    return;
                }

                // Save login preferences
                await SaveLoginPreferences(email, password, rememberMe);

                // Try to authenticate as Student first
                var loginResult = await AuthenticateStudent(email, password, rememberMe);
                if (loginResult)
                {
                    return;
                }

                // If not found in Students, try Instructor table
                loginResult = await AuthenticateInstructor(email, password, rememberMe);
                if (loginResult)
                {
                    return;
                }

                // If no user found or password incorrect
                ShowLoginError("Invalid email or password. Please check your credentials and try again.");

            }
            catch (Exception ex)
            {
                ShowLoginError($"An error occurred during login: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
            }
            finally
            {
                // Re-enable login button
                LoginButton.IsEnabled = true;
                LoginButton.Content = "SIGN IN";
            }
        }

        private bool ValidateLoginInput(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                ShowValidationError("Please enter both email and password.");
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowValidationError("Please enter a valid email address.");
                return false;
            }

            return true;
        }

        private async Task<bool> AuthenticateStudent(string email, string password, bool rememberMe)
        {
            try
            {
                var student = await _studentDAO.GetByEmailAsync(email);
                if (student == null)
                {
                    return false;
                }

                // Verify password
                bool isPasswordValid = PasswordHasher.VerifyPassword(password, student.Password);
                if (!isPasswordValid)
                {
                    return false;
                }

                // Check if student account is active
                if (!(student.Status == "Hoạt động" || student.Status == "Active"))
                {
                    ShowValidationError("Your student account is not active. Please contact support.");
                    return false;
                }

                // Successful student login
                await CompleteStudentLogin(student, rememberMe);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Student authentication error: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> AuthenticateInstructor(string email, string password, bool rememberMe)
        {
            try
            {
                var instructor = await _instructorDAO.GetByEmailAsync(email);
                if (instructor == null)
                {
                    return false;
                }

                // Verify password
                //bool isPasswordValid = PasswordHasher.VerifyPassword(password, instructor.Password);
                //if (!isPasswordValid)
                //{
                //    return false;
                //}

                // Successful instructor login
                await CompleteInstructorLogin(instructor, rememberMe);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Instructor authentication error: {ex.Message}");
                return false;
            }
        }

        private async Task CompleteStudentLogin(BusinessObjects.Models.Student student, bool rememberMe)
        {
            try
            {
                // Update last login time
                student.LastLogin = DateTime.Now;
                await _studentDAO.UpdateAsync(student);

                // Set session as Student
                SessionManager.SetCurrentUser(student);

                // Save session with rememberLogin flag
                await StateManager.SaveUserSessionAsync(rememberLogin: rememberMe);

                // Set success flags
                LoginSuccessful = true;
                UserType = "Student";

                // Show success message
                ShowSuccessMessage($"Welcome back, {student.StudentName}!\nLogged in as Student.");

                // Log successful login
                System.Diagnostics.Debug.WriteLine($"Student login successful: {student.StudentName} ({student.Email})");

                // Navigate to Student Main Window
                var studentMainWindow = new ProjectPRN.Student.StudentMainWindow(student);
                studentMainWindow.Show();

                // Close login window
                this.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to complete student login: {ex.Message}", ex);
            }
        }

        private async Task CompleteInstructorLogin(BusinessObjects.Models.Instructor instructor, bool rememberMe)
        {
            try
            {
                // Update last login time
                //instructor.LastLogin = DateTime.Now;
                await _instructorDAO.UpdateAsync(instructor);

                // Set session as Instructor
                SessionManager.SetCurrentUser(instructor);

                // Save session with rememberLogin flag
                await StateManager.SaveUserSessionAsync(rememberLogin: rememberMe);

                // Set success flags
                LoginSuccessful = true;
                UserType = "Instructor";

                // Show success message
                ShowSuccessMessage($"Welcome back, {instructor.InstructorName}!\nLogged in as Instructor.");

                // Log successful login
                System.Diagnostics.Debug.WriteLine($"Instructor login successful: {instructor.InstructorName} ({instructor.Email})");

                // Navigate to Admin Main Window (Instructor login goes to admin panel)
                var adminMainWindow = new ProjectPRN.Admin.AdminMainWindow();
                adminMainWindow.Show();

                // Close login window
                this.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to complete instructor login: {ex.Message}", ex);
            }
        }

        private async Task SaveLoginPreferences(string email, string password, bool rememberMe)
        {
            try
            {
                await StateManager.SaveUserPreferenceAsync("LastLoginEmail", email);
                await StateManager.SaveUserPreferenceAsync("RememberMe", rememberMe);

                if (rememberMe)
                {
                    // Encrypt and save password
                    var encryptedPassword = EncryptPassword(password);
                    await StateManager.SaveUserPreferenceAsync("SavedPassword", encryptedPassword);
                }
                else
                {
                    // Clear saved password if remember me is not checked
                    await StateManager.SaveUserPreferenceAsync("SavedPassword", "");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save login preferences: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(message, "Validation Error",
                          MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowLoginError(string message)
        {
            MessageBox.Show(message, "Login Failed",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Login Successful",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate to registration form
                Registration registrationWindow = new Registration();
                registrationWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening registration window: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            // For now, show a placeholder message
            MessageBox.Show("Forgot password functionality will be implemented soon.\n\n" +
                          "Please contact administrator for password reset:\n" +
                          "Email: admin@lifeskilllearning.com\n" +
                          "Phone: (555) 123-4567",
                          "Password Recovery",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        // Handle Enter key press for login
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
            
            // Dev tools toggle (Ctrl + Shift + D)
            if (e.Key == Key.D && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                DevToolsPanel.Visibility = DevToolsPanel.Visibility == Visibility.Visible 
                    ? Visibility.Collapsed 
                    : Visibility.Visible;
            }
        }

        // Dev tool: Admin Login Test (no authentication required)
        private void AdminLoginTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Direct navigation to AdminMainWindow without authentication
                var adminMainWindow = new ProjectPRN.Admin.AdminMainWindow();
                adminMainWindow.Show();

                // Close login window
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở trang admin: {ex.Message}", "Lỗi", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add smooth window dragging functionality
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Clear password when window loses focus (security feature)
        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Optionally clear sensitive data when window loses focus
            // PasswordBox.Clear();
        }

        // Focus on email textbox when window loads
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(EmailTextBox.Text))
            {
                EmailTextBox.Focus();
            }
            else
            {
                PasswordBox.Focus();
            }
        }

        // Handle password textbox enter key
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }

        // Auto-fill demo credentials (for development only - remove in production)
        private void DemoCredentials_Click(object sender, RoutedEventArgs e)
        {
            // This is for development/testing only - remove in production
            EmailTextBox.Text = "student@example.com";
            PasswordBox.Password = "password123";
            RememberMeCheckBox.IsChecked = true;
        }

        #region Password Encryption/Decryption
        private string EncryptPassword(string password)
        {
            try
            {
                // Simple encryption using machine key - not the most secure but adequate for this use case
                var data = Encoding.UTF8.GetBytes(password);
                var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to encrypt password: {ex.Message}");
                return "";
            }
        }

        private string DecryptPassword(string encryptedPassword)
        {
            try
            {
                var data = Convert.FromBase64String(encryptedPassword);
                var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to decrypt password: {ex.Message}");
                return "";
            }
        }
        #endregion

        // Handle Remember Me checkbox change
        private async void RememberMeCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = RememberMeCheckBox.IsChecked ?? false;

                if (!isChecked)
                {
                    // Clear saved password when unchecked
                    await StateManager.SaveUserPreferenceAsync("SavedPassword", "");
                    await StateManager.SaveUserPreferenceAsync("RememberMe", false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to handle RememberMe change: {ex.Message}");
            }
        }
    }
}