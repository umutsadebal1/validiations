using System.Windows;
using DosyaBütünlüğüDoğrulayıcı.Services;

namespace DosyaBütünlüğüDoğrulayıcı
{
    /// <summary>
    /// Application sınıfı - WPF uygulaması eyer noktası
    /// </summary>
    public partial class App : Application
    {
        private ThemeService _themeService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Tema servisini başlat ve temayı uygula
            _themeService = new ThemeService();
            _themeService.InitializeTheme();
        }

        public ThemeService GetThemeService()
        {
            return _themeService ?? new ThemeService();
        }
    }
}
