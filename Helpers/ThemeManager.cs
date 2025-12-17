using System;
using System.Windows;

namespace BlackBoxControl.Helpers
{
    /// <summary>
    /// Manages application theme switching
    /// </summary>
    public static class ThemeManager
    {
        private const string ThemeBaseUri = "pack://application:,,,/Themes/";

        public enum Theme
        {
            Green,
            Blue,
            Dark
        }

        /// <summary>
        /// Changes the application theme
        /// </summary>
        /// <param name="theme">The theme to apply</param>
        public static void ChangeTheme(Theme theme)
        {
            try
            {
                string themeName;

                // Use if-else for C# 7.3 compatibility
                if (theme == Theme.Green)
                {
                    themeName = "GreenTheme.xaml";
                }
                else if (theme == Theme.Blue)
                {
                    themeName = "BlueTheme.xaml";
                }
                else if (theme == Theme.Dark)
                {
                    themeName = "DarkTheme.xaml";
                }
                else
                {
                    themeName = "GreenTheme.xaml";
                }

                var app = Application.Current;
                var newTheme = new ResourceDictionary
                {
                    Source = new Uri(ThemeBaseUri + themeName, UriKind.Absolute)
                };

                // Clear existing merged dictionaries and add the new theme
                app.Resources.MergedDictionaries.Clear();
                app.Resources.MergedDictionaries.Add(newTheme);

                // Optionally save the theme preference
                SaveThemePreference(theme);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error changing theme: " + ex.Message,
                    "Theme Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads the saved theme preference on application startup
        /// </summary>
        public static void LoadThemePreference()
        {
            try
            {
                // Try to load from settings
                var savedTheme = Properties.Settings.Default.DefaultTheme;

                if (Enum.TryParse<Theme>(savedTheme, out var theme))
                {
                    ChangeTheme(theme);
                }
            }
            catch
            {
                // If loading fails, use default theme (already loaded in App.xaml)
            }
        }

        /// <summary>
        /// Saves the current theme preference
        /// </summary>
        private static void SaveThemePreference(Theme theme)
        {
            try
            {
                Properties.Settings.Default.DefaultTheme = theme.ToString();
                Properties.Settings.Default.Save();
            }
            catch
            {
                // Silently fail if saving fails
            }
        }

        /// <summary>
        /// Gets a theme resource by key
        /// </summary>
        public static T GetResource<T>(string key)
        {
            if (Application.Current.TryFindResource(key) is T resource)
            {
                return resource;
            }

            throw new Exception($"Resource '{key}' not found in current theme");
        }
    }
}

// Example usage in your MenuViewModel:
/*
using BlackBoxControl.Helpers;

public class MenuViewModel : ViewModelBase
{
    public ICommand BlueThemeCommand { get; }
    public ICommand GreenThemeCommand { get; }
    public ICommand DarkThemeCommand { get; }

    public MenuViewModel()
    {
        BlueThemeCommand = new RelayCommand(() => ThemeManager.ChangeTheme(ThemeManager.Theme.Blue));
        GreenThemeCommand = new RelayCommand(() => ThemeManager.ChangeTheme(ThemeManager.Theme.Green));
        DarkThemeCommand = new RelayCommand(() => ThemeManager.ChangeTheme(ThemeManager.Theme.Dark));
    }
}
*/