using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DosyaBütünlüğüDoğrulayıcı.Services;

namespace DosyaBütünlüğüDoğrulayıcı
{
    /// <summary>
    /// MainWindow - Uygulamanın ana penceresi
    /// Sidebar navigation ve TabControl yönetimi
    /// </summary>
    public partial class MainWindow : Window
    {
        private ThemeService _themeService;

        public MainWindow()
        {
            InitializeComponent();
            _themeService = ((App)Application.Current).GetThemeService();
        }

        /// <summary>
        /// Navigate to Dashboard Tab
        /// </summary>
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 0;
            UpdateButtonStates(0);
        }

        /// <summary>
        /// Navigate to Main Page Tab
        /// </summary>
        private void BtnMainTab_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            UpdateButtonStates(1);
        }

        /// <summary>
        /// Navigate to Verify Tab
        /// </summary>
        private void BtnVerifyTab_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
            UpdateButtonStates(2);
        }

        /// <summary>
        /// Navigate to Monitor Tab
        /// </summary>
        private void BtnMonitorTab_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
            UpdateButtonStates(3);
        }

        /// <summary>
        /// Navigate to Settings Tab
        /// </summary>
        private void BtnSettingsTab_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 4;
            UpdateButtonStates(4);
        }

        /// <summary>
        /// Toggle theme between dark and light
        /// </summary>
        private void BtnThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            _themeService.ToggleTheme();
            BtnThemeToggle.Content = _themeService.IsDarkTheme ? "🌙 Toggle Theme" : "☀️ Toggle Theme";
        }

        /// <summary>
        /// Exit application with confirmation
        /// </summary>
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", 
                "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Update sidebar button active/inactive states with proper resource binding
        /// </summary>
        private void UpdateButtonStates(int selectedIndex)
        {
            var buttons = new[] { BtnDashboard, BtnMainTab, BtnVerifyTab, BtnMonitorTab, BtnSettingsTab };
            
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == selectedIndex)
                {
                    buttons[i].SetResourceReference(Button.BackgroundProperty, "AccentBrush");
                }
                else
                {
                    buttons[i].SetResourceReference(Button.BackgroundProperty, "SurfaceBrush");
                }
            }
        }
    }
}
