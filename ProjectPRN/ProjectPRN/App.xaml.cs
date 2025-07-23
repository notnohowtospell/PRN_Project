using System.Windows;
using BusinessObjects.Models;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectPRN.Admin.CourseManagement;
using ProjectPRN.Student.Courses;
using ProjectPRN.Utils;
using Repositories;
using Repositories.Interfaces;

namespace ProjectPRN
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            var connectionString = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build()
            .GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Transient);

            // Register DAOs
            services.AddScoped<ILifeSkillCourseDAO, LifeSkillCourseDAO>();
            services.AddScoped<IInstructorDAO, InstructorDAO>();
            services.AddScoped<IStudentDAO, StudentDAO>();
            services.AddScoped<IEnrollmentDAO, EnrollmentDAO>();
            services.AddScoped<IPaymentDAO, PaymentDAO>();

            // Register Repositories
            services.AddScoped<ILifeSkillCourseRepository, LifeSkillCourseRepository>();
            services.AddScoped<IInstructorRepository, InstructorRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            services.AddTransient<CourseManagementView>();
            services.AddTransient<StudentCourseView>();


            ServiceProvider = services.BuildServiceProvider();

            base.OnStartup(e);

            // Try to restore user session
            var sessionRestored = await TryRestoreUserSession();

            if (!sessionRestored)
            {
                // No valid session found, show login window
                var loginWindow = new Login();
                loginWindow.Show();
            }
            //var mainWindow = ServiceProvider.GetRequiredService<Registration>();
            var mainWindow = new Registration();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // Save application state when exiting
            if (MainWindow != null)
            {
                await StateManager.SaveApplicationStateAsync(MainWindow);
            }

            base.OnExit(e);
        }

        private async Task<bool> TryRestoreUserSession()
        {
            try
            {
                var savedSession = await StateManager.LoadUserSessionAsync();
                if (savedSession != null && savedSession.RememberLogin)
                {
                    System.Diagnostics.Debug.WriteLine($"Found saved session for: {savedSession.UserName}");

                    // Validate session with database and restore SessionManager
                    var sessionValid = await ValidateAndRestoreSession(savedSession);

                    if (sessionValid)
                    {
                        // Session is valid, open main window directly
                        var mainWindow = new MainWindow();
                        mainWindow.Show();

                        // Show welcome back message
                        MessageBox.Show($"Welcome back, {savedSession.UserName}!", "Auto Login",
                                      MessageBoxButton.OK, MessageBoxImage.Information);

                        return true;
                    }
                    else
                    {
                        // Session invalid, clear it
                        await StateManager.ClearUserSessionAsync();
                        System.Diagnostics.Debug.WriteLine("Saved session is invalid, cleared.");
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to restore session: {ex.Message}");

                // Clear potentially corrupted session
                await StateManager.ClearUserSessionAsync();
                return false;
            }
        }

        private async Task<bool> ValidateAndRestoreSession(UserSession savedSession)
        {
            try
            {
                // Validate session data
                if (savedSession.UserId == null || string.IsNullOrEmpty(savedSession.UserType) ||
                    string.IsNullOrEmpty(savedSession.Email))
                {
                    return false;
                }

                // Check if user still exists in database and restore SessionManager
                if (savedSession.UserType == "Student")
                {
                    var studentDAO = new StudentDAO();
                    var student = await studentDAO.GetByEmailAsync(savedSession.Email);

                    if (student != null && student.StudentId == savedSession.UserId && student.Status == "Active")
                    {
                        // Restore SessionManager state
                        SessionManager.SetCurrentUser(student);

                        // Update last login
                        student.LastLogin = DateTime.Now;
                        await studentDAO.UpdateAsync(student);

                        System.Diagnostics.Debug.WriteLine($"Student session restored: {student.StudentName}");
                        return true;
                    }
                }
                else if (savedSession.UserType == "Instructor")
                {
                    var instructorDAO = new InstructorDAO();
                    var instructor = await instructorDAO.GetByEmailAsync(savedSession.Email);

                    if (instructor != null && instructor.InstructorId == savedSession.UserId)
                    {
                        // Restore SessionManager state
                        SessionManager.SetCurrentUser(instructor);

                        // Update last login
                        instructor.LastLogin = DateTime.Now;
                        await instructorDAO.UpdateAsync(instructor);

                        System.Diagnostics.Debug.WriteLine($"Instructor session restored: {instructor.InstructorName}");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Session validation failed: {ex.Message}");
                return false;
            }
        }
    }
}
