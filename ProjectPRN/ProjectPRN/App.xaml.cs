using System.Windows;
using DataAccessObjects;
using ProjectPRN.Utils;

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
            // Prevent automatic shutdown when windows are closed
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            base.OnStartup(e);
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
                        //instructor.LastLogin = DateTime.Now;
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
