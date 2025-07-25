using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectPRN.Utils
{
    public static class ThemeManager
    {
        public static void ChangeTheme(string theme)
        {
            var app = Application.Current;
            // Remove previous theme dictionaries (assuming only one at a time)
            for (int i = app.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                var dict = app.Resources.MergedDictionaries[i];
                if (dict.Source != null && (dict.Source.OriginalString.Contains("LightTheme.xaml") || dict.Source.OriginalString.Contains("DarkTheme.xaml")))
                {
                    app.Resources.MergedDictionaries.RemoveAt(i);
                }
            }

            // Add the chosen theme
            var themeDict = new ResourceDictionary();
            switch (theme)
            {
                case "Dark":
                    themeDict.Source = new Uri("Theme/DarkTheme.xaml", UriKind.Relative);
                    break;
                default:
                    themeDict.Source = new Uri("Theme/LightTheme.xaml", UriKind.Relative);
                    break;
            }
            app.Resources.MergedDictionaries.Add(themeDict);
            // Save user preference
            _ = StateManager.SaveUserPreferenceAsync("AppTheme", theme);
        }

        public static async Task LoadThemeAsync()
        {
            var theme = await StateManager.GetUserPreferenceAsync<string>("AppTheme", "Light");
            ChangeTheme(theme ?? "Light");
        }
    }
}
