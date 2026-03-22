using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DosyaBütünlüğüDoğrulayıcı.Services;

namespace DosyaBütünlüğüDoğrulayıcı
{
    /// <summary>
    /// MainWindow - main application window
    /// Handles sidebar navigation and TabControl management
    /// </summary>
    public partial class MainWindow : Window
    {
        private ThemeService _themeService;
        private Views.DashboardTab _dashboardTabContent;
        private Frame _mainTabFrame;
        private Views.VerifyTab _verifyTabContent;
        private Views.MonitorTab _monitorTabContent;
        private Views.SettingsTab _settingsTabContent;

        public MainWindow()
        {
            InitializeComponent();
            _themeService = ((App)Application.Current).GetThemeService();
            _themeService.ThemeChanged += OnThemeChanged;
            UpdateThemeToggleLabel();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                EnsureMainTabLoaded();
                MainTabControl.SelectedIndex = 1;
                UpdateButtonStates(1);
            }, DispatcherPriority.ApplicationIdle);
        }

        private void EnsureMainTabLoaded()
        {
            if (_mainTabFrame != null)
            {
                return;
            }

            _mainTabFrame = new Frame
            {
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden
            };
            _mainTabFrame.Navigate(new Views.MainTab());

            MainTabHost.Children.Clear();
            MainTabHost.Children.Add(_mainTabFrame);
        }

        private void EnsureDashboardLoaded()
        {
            if (_dashboardTabContent != null)
            {
                return;
            }

            _dashboardTabContent = new Views.DashboardTab();
            DashboardHost.Children.Clear();
            DashboardHost.Children.Add(_dashboardTabContent);
        }

        private void EnsureVerifyLoaded()
        {
            if (_verifyTabContent != null)
            {
                return;
            }

            _verifyTabContent = new Views.VerifyTab();
            VerifyTabHost.Children.Clear();
            VerifyTabHost.Children.Add(_verifyTabContent);
        }

        private void EnsureMonitorLoaded()
        {
            if (_monitorTabContent != null)
            {
                return;
            }

            _monitorTabContent = new Views.MonitorTab();
            MonitorTabHost.Children.Clear();
            MonitorTabHost.Children.Add(_monitorTabContent);
        }

        private void EnsureSettingsLoaded()
        {
            if (_settingsTabContent != null)
            {
                return;
            }

            _settingsTabContent = new Views.SettingsTab();
            SettingsTabHost.Children.Clear();
            SettingsTabHost.Children.Add(_settingsTabContent);
        }

        public void OpenFolderHashTab()
        {
            EnsureMainTabLoaded();
            MainTabControl.SelectedIndex = 1;
            UpdateButtonStates(1);

            if (_mainTabFrame?.Content is Views.MainTab loadedPage)
            {
                loadedPage.SelectFolderHashTab();
            }
        }

        private void OnThemeChanged(bool isDarkTheme)
        {
            UpdateThemeToggleLabel();
        }

        private void UpdateThemeToggleLabel()
        {
            BtnThemeToggle.Content = _themeService.IsDarkTheme ? "🌙 Dark Theme" : "☀️ Light Theme";
        }

        /// <summary>
        /// Navigate to Dashboard Tab
        /// </summary>
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            EnsureDashboardLoaded();
            MainTabControl.SelectedIndex = 0;
            UpdateButtonStates(0);
            _dashboardTabContent?.RefreshDashboard();
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender is not TabControl tabControl || !ReferenceEquals(tabControl, MainTabControl))
            {
                return;
            }

            switch (MainTabControl.SelectedIndex)
            {
                case 0:
                    Dispatcher.BeginInvoke(() =>
                    {
                        EnsureDashboardLoaded();
                        if (_dashboardTabContent != null && _dashboardTabContent.IsLoaded)
                        {
                            _dashboardTabContent.RefreshDashboard();
                        }
                    }, DispatcherPriority.Background);
                    break;
                case 1:
                    EnsureMainTabLoaded();
                    break;
                case 2:
                    EnsureVerifyLoaded();
                    break;
                case 3:
                    EnsureMonitorLoaded();
                    break;
                case 4:
                    EnsureSettingsLoaded();
                    break;
            }
        }

        /// <summary>
        /// Navigate to Main Page Tab
        /// </summary>
        private void BtnMainTab_Click(object sender, RoutedEventArgs e)
        {
            EnsureMainTabLoaded();
            MainTabControl.SelectedIndex = 1;
            UpdateButtonStates(1);
        }

        /// <summary>
        /// Navigate to Verify Tab
        /// </summary>
        private void BtnVerifyTab_Click(object sender, RoutedEventArgs e)
        {
            EnsureVerifyLoaded();
            MainTabControl.SelectedIndex = 2;
            UpdateButtonStates(2);
        }

        /// <summary>
        /// Navigate to Monitor Tab
        /// </summary>
        private void BtnMonitorTab_Click(object sender, RoutedEventArgs e)
        {
            EnsureMonitorLoaded();
            MainTabControl.SelectedIndex = 3;
            UpdateButtonStates(3);
        }

        /// <summary>
        /// Navigate to Settings Tab
        /// </summary>
        private void BtnSettingsTab_Click(object sender, RoutedEventArgs e)
        {
            EnsureSettingsLoaded();
            MainTabControl.SelectedIndex = 4;
            UpdateButtonStates(4);
        }

        /// <summary>
        /// Toggle theme between dark and light
        /// </summary>
        private void BtnThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            _themeService.ToggleTheme();
            UpdateThemeToggleLabel();
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
                buttons[i].SetResourceReference(Button.ForegroundProperty, "SidebarButtonForegroundBrush");

                if (i == selectedIndex)
                {
                    buttons[i].SetResourceReference(Button.BackgroundProperty, "AccentBrush");
                }
                else
                {
                    buttons[i].SetResourceReference(Button.BackgroundProperty, "SidebarButtonBackgroundBrush");
                }
            }
        }
    }
}
