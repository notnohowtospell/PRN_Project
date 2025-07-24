using BusinessObjects.Models;

namespace ProjectPRN.Utils
{
    public enum UserType
    {
        Student,
        Instructor
    }

    public static class SessionManager
    {
        public static UserType? CurrentUserType { get; private set; }
        public static BusinessObjects.Models.Student? CurrentStudent { get; private set; }
        public static Instructor? CurrentInstructor { get; private set; }
        public static bool IsLoggedIn => CurrentUserType.HasValue;

        public static void SetCurrentUser(BusinessObjects.Models.Student student)
        {
            CurrentUserType = UserType.Student;
            CurrentStudent = student;
            CurrentInstructor = null;
        }

        public static void SetCurrentUser(Instructor instructor)
        {
            CurrentUserType = UserType.Instructor;
            CurrentInstructor = instructor;
            CurrentStudent = null;
        }

        public static async Task ClearSessionAsync()
        {
            CurrentUserType = null;
            CurrentStudent = null;
            CurrentInstructor = null;

            // Also clear saved session file
            await StateManager.ClearUserSessionAsync();
        }

        public static void ClearSession()
        {
            CurrentUserType = null;
            CurrentStudent = null;
            CurrentInstructor = null;
        }

        public static string GetCurrentUserName()
        {
            return CurrentUserType switch
            {
                UserType.Student => CurrentStudent?.StudentName ?? "Unknown",
                UserType.Instructor => CurrentInstructor?.InstructorName ?? "Unknown",
                _ => "Guest"
            };
        }

        public static string GetCurrentUserEmail()
        {
            return CurrentUserType switch
            {
                UserType.Student => CurrentStudent?.Email ?? "",
                UserType.Instructor => CurrentInstructor?.Email ?? "",
                _ => ""
            };
        }


        public static int GetCurrentUserId()
        {
            return CurrentUserType switch
            {
                UserType.Student => CurrentStudent?.StudentId ?? 0,
                UserType.Instructor => CurrentInstructor?.InstructorId ?? 0,
                _ => 0
            };
        }

        public static string GetCurrentUserRole()
        {
            return CurrentUserType switch
            {
                UserType.Student => "Student",
                UserType.Instructor => "Instructor",
                _ => "Guest"
            };
        }

        public static string GetSessionInfo()
        {
            if (!IsLoggedIn)
                return "No user logged in";

            return $"User: {GetCurrentUserName()}\n" +
                   $"Email: {GetCurrentUserEmail()}\n" +
                   $"Role: {GetCurrentUserRole()}\n" +
                   $"ID: {GetCurrentUserId()}";
        }

        public static bool IsStudent => CurrentUserType == UserType.Student;
        public static bool IsInstructor => CurrentUserType == UserType.Instructor;

        /// <summary>
        /// Logout current user and optionally clear saved session
        /// </summary>
        /// <param name="clearSavedSession">Whether to clear the saved "Remember Me" session</param>
        public static async Task LogoutAsync(bool clearSavedSession = true)
        {
            if (clearSavedSession)
            {
                await ClearSessionAsync();
            }
            else
            {
                ClearSession();
            }
        }
    }
}