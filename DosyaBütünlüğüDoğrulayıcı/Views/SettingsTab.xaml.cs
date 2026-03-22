using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DosyaBütünlüğüDoğrulayıcı.Services;
using Microsoft.Win32;

namespace DosyaBütünlüğüDoğrulayıcı.Views
{
    /// <summary>
    /// Settings tab for app preferences and maintenance actions
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        private const string StartupRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string StartupValueName = "DosyaButunluguDogrulayici";
        private DatabaseService _dbService;
        private ThemeService _themeService;

        public SettingsTab()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _themeService = ((App)Application.Current).GetThemeService();
            _themeService.ThemeChanged += OnThemeChanged;

            // Default export format
            ComboExportFormat.SelectedIndex = 0;

            // Load startup checkbox state from registry.
            LoadSettings();
            UpdateThemeStatus();
        }

        private void OnThemeChanged(bool isDarkTheme)
        {
            Dispatcher.Invoke(UpdateThemeStatus);
        }

        private void UpdateThemeStatus()
        {
            TxtCurrentTheme.Text = _themeService.IsDarkTheme ? "Current Theme: Dark" : "Current Theme: Light";
            BtnSetDarkTheme.IsEnabled = !_themeService.IsDarkTheme;
            BtnSetLightTheme.IsEnabled = _themeService.IsDarkTheme;
        }

        private void BtnSetDarkTheme_Click(object sender, RoutedEventArgs e)
        {
            _themeService.SetTheme(true);
            UpdateThemeStatus();
        }

        private void BtnSetLightTheme_Click(object sender, RoutedEventArgs e)
        {
            _themeService.SetTheme(false);
            UpdateThemeStatus();
        }

        /// <summary>
        /// Load settings from system state
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, false);
                ChkStartupCheck.IsChecked = key?.GetValue(StartupValueName) != null;
            }
            catch
            {
                ChkStartupCheck.IsChecked = false;
            }
        }

        /// <summary>
        /// Enable launch at startup
        /// </summary>
        private void ChkStartupCheck_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true)
                    ?? Registry.CurrentUser.CreateSubKey(StartupRegistryPath, true);

                var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(executablePath))
                {
                    key?.SetValue(StartupValueName, $"\"{executablePath}\"");
                }

                MessageBox.Show("Application will launch at startup.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to enable startup launch: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ChkStartupCheck.IsChecked = false;
            }
        }

        /// <summary>
        /// Disable launch at startup
        /// </summary>
        private void ChkStartupCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true);
                key?.DeleteValue(StartupValueName, false);
                MessageBox.Show("Application will not launch at startup.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to disable startup launch: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ChkStartupCheck.IsChecked = true;
            }
        }

        /// <summary>
        /// Clear saved hash history
        /// </summary>
        private void BtnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete all hash history?\nThis action cannot be undone.",
                "Warning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (_dbService.ClearHistory())
                {
                    MessageBox.Show("History cleared successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("An error occurred while clearing history.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Export history data
        /// </summary>
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var format = (ComboExportFormat.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "CSV";
                string exportData;
                string fileName;

                if (format == "CSV")
                {
                    exportData = _dbService.ExportAsCSV();
                    fileName = $"hash_history_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                }
                else
                {
                    exportData = _dbService.ExportAsJSON();
                    fileName = $"hash_history_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                }

                // Save to Desktop.
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var filePath = System.IO.Path.Combine(desktopPath, fileName);

                System.IO.File.WriteAllText(filePath, exportData);

                MessageBox.Show(
                    $"Data exported successfully.\n\n{filePath}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
