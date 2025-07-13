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
using System.Text.RegularExpressions;
using BusinessObjects.Models;
using DataAccessObjects;
using ProjectPRN.Utils;

namespace ProjectPRN
{
    /// <summary>
    /// Interaction logic for Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        private readonly IStudentDAO _studentDAO;
        private readonly IInstructorDAO _instructorDAO;

        public Registration()
        {
            InitializeComponent();
            _studentDAO = new StudentDAO();
            _instructorDAO = new InstructorDAO();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable register button during processing
                RegisterButton.IsEnabled = false;
                RegisterButton.Content = "CREATING ACCOUNT...";

                // Get input values
                string fullName = FullNameTextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();
                string phone = PhoneTextBox.Text.Trim();
                string password = PasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;
                bool termsAccepted = TermsCheckBox.IsChecked ?? false;

                // Validation
                if (!ValidateInput(fullName, email, phone, password, confirmPassword, termsAccepted))
                {
                    return;
                }

                // Check if email already exists in both tables
                var existingStudent = await _studentDAO.GetByEmailAsync(email);
                var existingInstructor = await _instructorDAO.GetByEmailAsync(email);
                
                if (existingStudent != null || existingInstructor != null)
                {
                    MessageBox.Show("An account with this email address already exists. Please use a different email or try logging in.", 
                                  "Email Already Exists", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    return;
                }

                // Hash the password
                string hashedPassword = PasswordHasher.HashPassword(password);

                // Create account based on user type (default to Student for now)
                await CreateStudentAccount(fullName, email, phone, hashedPassword);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during registration: {ex.Message}", 
                              "Registration Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable register button
                RegisterButton.IsEnabled = true;
                RegisterButton.Content = "CREATE ACCOUNT";
            }
        }

        private async Task CreateStudentAccount(string fullName, string email, string phone, string hashedPassword)
        {
            // Generate student code
            string studentCode = GenerateStudentCode();

            // Create new student object
            var newStudent = new Student
            {
                StudentCode = studentCode,
                StudentName = fullName,
                Email = email,
                PhoneNumber = phone,
                Password = hashedPassword,
                Status = "Active", // Set default status
                DateOfBirth = null, // Can be set later in profile
                AvatarPath = null, // Can be set later in profile
                LastLogin = null // Will be set on first login
            };

            // Save to database
            await _studentDAO.AddAsync(newStudent);

            // Show success message
            MessageBox.Show($"Student account created successfully for {fullName}!\nStudent Code: {studentCode}\nWelcome to LifeSkill Learning Platform.", 
                          "Registration Successful", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);

            // Navigate to login
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }

        private async Task CreateInstructorAccount(string fullName, string email, string phone, string hashedPassword)
        {
            // Create new instructor object
            var newInstructor = new Instructor
            {
                InstructorName = fullName,
                Email = email,
                PhoneNumber = phone,
                Password = hashedPassword,
                Experience = 0, // Default experience, can be updated later
                LastLogin = null // Will be set on first login
            };

            // Save to database
            await _instructorDAO.AddAsync(newInstructor);

            // Show success message
            MessageBox.Show($"Instructor account created successfully for {fullName}!\nWelcome to LifeSkill Learning Platform.", 
                          "Registration Successful", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);

            // Navigate to login
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }

        private string GenerateStudentCode()
        {
            // Generate a unique student code
            // Format: STU + current year + random 4 digits
            var random = new Random();
            var year = DateTime.Now.Year.ToString();
            var randomNumber = random.Next(1000, 9999).ToString();
            return $"STU{year}{randomNumber}";
        }

        private bool ValidateInput(string fullName, string email, string phone, string password, string confirmPassword, bool termsAccepted)
        {
            // Check if all fields are filled
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || 
                string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(password) || 
                string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate full name (at least 2 words)
            if (fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                MessageBox.Show("Please enter your full name (first and last name).", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate email format
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate phone number (basic validation)
            if (!IsValidPhone(phone))
            {
                MessageBox.Show("Please enter a valid phone number (10-11 digits).", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate password strength
            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Enhanced password validation
            if (!IsStrongPassword(password))
            {
                MessageBox.Show("Password must contain at least one uppercase letter, one lowercase letter, and one number.", 
                              "Password Too Weak", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return false;
            }

            // Check password confirmation
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check terms acceptance
            if (!termsAccepted)
            {
                MessageBox.Show("Please accept the Terms and Conditions to continue.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Remove all non-digit characters
            string digits = Regex.Replace(phone, @"\D", "");
            
            // Check if it's 10 or 11 digits (common phone number lengths)
            return digits.Length >= 10 && digits.Length <= 11;
        }

        private bool IsStrongPassword(string password)
        {
            // Check for at least one uppercase, one lowercase, and one digit
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            
            return hasUpper && hasLower && hasDigit;
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to login
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }

        // Handle Enter key press for registration
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RegisterButton_Click(sender, e);
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