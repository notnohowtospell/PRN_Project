using System.Text.Json;
using System.IO;
using System.Windows;

namespace ProjectPRN.Utils
{
    public class ApplicationState
    {
        public WindowState WindowState { get; set; }
        public double WindowLeft { get; set; }
        public double WindowTop { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public string? LastSelectedTab { get; set; }
        public Dictionary<string, object> UserPreferences { get; set; } = new();
        public DateTime LastSaved { get; set; }
    }

    public class UserSession
    {
        public int? UserId { get; set; }
        public string? UserType { get; set; } // "Student" or "Instructor"
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public DateTime LoginTime { get; set; }
        public bool RememberLogin { get; set; }
        public Dictionary<string, object> SessionData { get; set; } = new();
    }

    public static class StateManager
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "LifeSkillLearning");
        
        private static readonly string StateFilePath = Path.Combine(AppDataFolder, "app_state.json");
        private static readonly string SessionFilePath = Path.Combine(AppDataFolder, "user_session.json");

        static StateManager()
        {
            // Ensure app data folder exists
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
        }

        #region Application State Management

        public static async Task SaveApplicationStateAsync(Window mainWindow, string? selectedTab = null)
        {
            try
            {
                var state = new ApplicationState
                {
                    WindowState = mainWindow.WindowState,
                    WindowLeft = mainWindow.Left,
                    WindowTop = mainWindow.Top,
                    WindowWidth = mainWindow.Width,
                    WindowHeight = mainWindow.Height,
                    LastSelectedTab = selectedTab,
                    LastSaved = DateTime.Now
                };

                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(StateFilePath, json);
            }
            catch (Exception ex)
            {
                // Log error but don't show to user as it's not critical
                System.Diagnostics.Debug.WriteLine($"Failed to save application state: {ex.Message}");
            }
        }

        public static async Task<ApplicationState?> LoadApplicationStateAsync()
        {
            try
            {
                if (!File.Exists(StateFilePath))
                    return null;

                var json = await File.ReadAllTextAsync(StateFilePath);
                return JsonSerializer.Deserialize<ApplicationState>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load application state: {ex.Message}");
                return null;
            }
        }

        public static void RestoreWindowState(Window window, ApplicationState state)
        {
            try
            {
                // Validate the saved position is still valid (in case screen resolution changed)
                if (state.WindowLeft >= 0 && state.WindowLeft < SystemParameters.VirtualScreenWidth &&
                    state.WindowTop >= 0 && state.WindowTop < SystemParameters.VirtualScreenHeight)
                {
                    window.Left = state.WindowLeft;
                    window.Top = state.WindowTop;
                }

                if (state.WindowWidth > 0 && state.WindowHeight > 0)
                {
                    window.Width = state.WindowWidth;
                    window.Height = state.WindowHeight;
                }

                window.WindowState = state.WindowState;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to restore window state: {ex.Message}");
            }
        }

        public static async Task SaveUserPreferenceAsync(string key, object value)
        {
            try
            {
                var state = await LoadApplicationStateAsync() ?? new ApplicationState();
                state.UserPreferences[key] = value;
                state.LastSaved = DateTime.Now;

                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(StateFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save user preference: {ex.Message}");
            }
        }

        public static async Task<T?> GetUserPreferenceAsync<T>(string key, T? defaultValue = default)
        {
            try
            {
                var state = await LoadApplicationStateAsync();
                if (state?.UserPreferences?.ContainsKey(key) == true)
                {
                    var jsonElement = (JsonElement)state.UserPreferences[key];
                    return jsonElement.Deserialize<T>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get user preference: {ex.Message}");
            }
            
            return defaultValue;
        }

        #endregion

        #region Session Management

        public static async Task SaveUserSessionAsync(bool rememberLogin = false)
        {
            try
            {
                if (!SessionManager.IsLoggedIn)
                    return;

                var session = new UserSession
                {
                    UserId = SessionManager.GetCurrentUserId(),
                    UserType = SessionManager.GetCurrentUserRole(),
                    UserName = SessionManager.GetCurrentUserName(),
                    Email = SessionManager.GetCurrentUserEmail(),
                    LoginTime = DateTime.Now,
                    RememberLogin = rememberLogin
                };

                var json = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(SessionFilePath, json);

                System.Diagnostics.Debug.WriteLine($"Session saved to: {SessionFilePath}");
                System.Diagnostics.Debug.WriteLine($"Remember login: {rememberLogin}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save user session: {ex.Message}");
            }
        }

        public static async Task<UserSession?> LoadUserSessionAsync()
        {
            try
            {
                if (!File.Exists(SessionFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Session file not found: {SessionFilePath}");
                    return null;
                }

                var json = await File.ReadAllTextAsync(SessionFilePath);
                var session = JsonSerializer.Deserialize<UserSession>(json);

                if (session != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Session loaded: {session.UserName}, RememberLogin: {session.RememberLogin}");

                    // If RememberLogin is true, check if session is still valid (not older than 30 days)
                    if (session.RememberLogin && DateTime.Now.Subtract(session.LoginTime).TotalDays <= 30)
                    {
                        return session;
                    }
                    // If RememberLogin is false, still return session but it won't be used for auto-login
                    else if (!session.RememberLogin)
                    {
                        return session;
                    }
                }

                // Clear invalid session
                await ClearUserSessionAsync();
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load user session: {ex.Message}");
                return null;
            }
        }

        public static async Task ClearUserSessionAsync()
        {
            try
            {
                if (File.Exists(SessionFilePath))
                {
                    File.Delete(SessionFilePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear user session: {ex.Message}");
            }
        }

        public static async Task SaveSessionDataAsync(string key, object value)
        {
            try
            {
                var session = await LoadUserSessionAsync() ?? new UserSession();
                session.SessionData[key] = value;

                var json = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(SessionFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save session data: {ex.Message}");
            }
        }

        public static async Task<T?> GetSessionDataAsync<T>(string key, T? defaultValue = default)
        {
            try
            {
                var session = await LoadUserSessionAsync();
                if (session?.SessionData?.ContainsKey(key) == true)
                {
                    var jsonElement = (JsonElement)session.SessionData[key];
                    return jsonElement.Deserialize<T>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get session data: {ex.Message}");
            }

            return defaultValue;
        }

        #endregion
    }
}