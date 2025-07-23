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
using DataAccessObjects;
using ProjectPRN.Utils;
using BusinessObjects.Models;

namespace ProjectPRN
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private readonly IStudentDAO _studentDAO;
        private readonly IInstructorDAO _instructorDAO;

        public Login()
        {
            InitializeComponent();
            _studentDAO = new StudentDAO();
            _instructorDAO = new InstructorDAO();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please enter both email and password.", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Try to authenticate as Student first
                var student = await _studentDAO.GetByEmailAsync(email);
                if (student != null)
                {
                    // Verify password for student
                    bool isPasswordValid = PasswordHasher.VerifyPassword(password, student.Password);

                    if (isPasswordValid)
                    {
                        // Check if student account is active
                        if (!(student.Status == "Hoạt động" || student.Status == "Active"))
                        {
                            MessageBox.Show("Your student account is not active. Please contact support.", "Account Inactive",
                                          MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Update last login time
                        student.LastLogin = DateTime.Now;
                        await _studentDAO.UpdateAsync(student);

                        // Set session as Student
                        SessionManager.SetCurrentUser(student);

                        // Save session if remember me is checked
                        if (rememberMe)
                        {
                            await StateManager.SaveUserSessionAsync(rememberLogin: true);
                        }

                        // Show success message
                        MessageBox.Show($"Welcome back, {student.StudentName}!\nLogged in as Student.", "Login Successful",
                                      MessageBoxButton.OK, MessageBoxImage.Information);

                        // Open main window
                        OpenMainWindow();
                        return;
                    }
                }

                // If not found in Students or password incorrect, try Instructor table
                var instructor = await _instructorDAO.GetByEmailAsync(email);
                if (instructor != null)
                {
                    // Verify password for instructor
                    bool isPasswordValid = PasswordHasher.VerifyPassword(password, instructor.Password);

                    if (isPasswordValid)
                    {
                        // Update last login time
                        instructor.LastLogin = DateTime.Now;
                        await _instructorDAO.UpdateAsync(instructor);

                        // Set session as Instructor
                        SessionManager.SetCurrentUser(instructor);

                        // Save session if remember me is checked
                        if (rememberMe)
                        {
                            await StateManager.SaveUserSessionAsync(rememberLogin: true);
                        }

                        // Show success message
                        MessageBox.Show($"Welcome back, {instructor.InstructorName}!\nLogged in as Instructor.", "Login Successful",
                                      MessageBoxButton.OK, MessageBoxImage.Information);

                        // Open main window
                        OpenMainWindow();
                        return;
                    }
                }

                // If no user found or password incorrect
                MessageBox.Show("Invalid email or password. Please check your credentials and try again.", "Login Failed",
                              MessageBoxButton.OK, MessageBoxImage.Error);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during login: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable login button
                LoginButton.IsEnabled = true;
                LoginButton.Content = "SIGN IN";
            }
        }

        private void OpenMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Forgot password functionality will be implemented soon.", "Coming Soon",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to registration form
            Registration registrationWindow = new Registration();
            registrationWindow.Show();
            this.Close();
        }

        // Handle Enter key press for login
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
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
    }
}
