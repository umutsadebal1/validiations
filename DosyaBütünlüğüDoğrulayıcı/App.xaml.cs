using System.Windows;
using System.Windows.Threading;
using DosyaBütünlüğüDoğrulayıcı.Services;

namespace DosyaBütünlüğüDoğrulayıcı
{
    /// <summary>
    /// Application class entry point for the WPF app
    /// </summary>
    public partial class App : Application
    {
        private ThemeService _themeService;

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);

            // Initialize the theme service and apply the saved theme.
            _themeService = new ThemeService();
            _themeService.InitializeTheme();

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            mainWindow.WindowState = WindowState.Normal;
            mainWindow.ShowInTaskbar = true;
            mainWindow.ShowActivated = true;
            mainWindow.Show();
            mainWindow.Activate();

            Dispatcher.BeginInvoke(() =>
            {
                if (MainWindow == null)
                {
                    return;
                }

                MainWindow.WindowState = WindowState.Normal;
                MainWindow.Topmost = true;
                MainWindow.Topmost = false;
                MainWindow.Activate();
                MainWindow.Focus();
            }, DispatcherPriority.ApplicationIdle);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Beklenmeyen hata: {e.Exception.Message}", "Uygulama Hatasi", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            Current.Shutdown();
        }

        public ThemeService GetThemeService()
        {
            return _themeService ?? new ThemeService();
        }
    }
}
