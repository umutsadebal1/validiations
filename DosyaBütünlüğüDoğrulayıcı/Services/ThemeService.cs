using System;
using System.Windows;
using System.IO;

namespace DosyaBütünlüğüDoğrulayıcı.Services
{
    /// <summary>
    /// Tema yönetimi servisi (Koyu/Açık tema geçişi)
    /// Ayarlar LocalAppData'ya kaydedilir
    /// </summary>
    public class ThemeService
    {
        private const string SettingsFileName = "ThemeSettings.txt";
        private readonly string _settingsPath;
        private bool _isDarkTheme = true;

        public event Action<bool> ThemeChanged;

        public ThemeService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsDir = Path.Combine(appDataPath, "DosyaBütünlüğüDoğrulayıcı");

            if (!Directory.Exists(settingsDir))
                Directory.CreateDirectory(settingsDir);

            _settingsPath = Path.Combine(settingsDir, SettingsFileName);

            LoadThemeSettings();
        }

        /// <summary>
        /// Kaydedilmiş tema ayarlarını yükle
        /// </summary>
        private void LoadThemeSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var content = File.ReadAllText(_settingsPath);
                    _isDarkTheme = bool.Parse(content);
                }
                else
                {
                    _isDarkTheme = true; // Varsayılan: Koyu tema
                    SaveThemeSettings();
                }
            }
            catch
            {
                _isDarkTheme = true;
            }
        }

        /// <summary>
        /// Tema ayarlarını kaydet
        /// </summary>
        private void SaveThemeSettings()
        {
            try
            {
                File.WriteAllText(_settingsPath, _isDarkTheme.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tema ayarları kaydedilemedi: {ex.Message}");
            }
        }

        /// <summary>
        /// Temayı değiştir
        /// </summary>
        public void ToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            ApplyTheme();
            SaveThemeSettings();
            ThemeChanged?.Invoke(_isDarkTheme);
        }

        /// <summary>
        /// Temayı uygula
        /// </summary>
        public void ApplyTheme()
        {
            try
            {
                var themeUri = _isDarkTheme
                    ? new Uri("Resources/DarkTheme.xaml", UriKind.Relative)
                    : new Uri("Resources/LightTheme.xaml", UriKind.Relative);

                // Mevcut tema resource'unu kaldır
                Application.Current.Resources.MergedDictionaries.Clear();

                // Yeni temayı ekle
                var dictionary = new ResourceDictionary { Source = themeUri };
                Application.Current.Resources.MergedDictionaries.Add(dictionary);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tema uygulanırken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Koyu tema aktif mi
        /// </summary>
        public bool IsDarkTheme => _isDarkTheme;

        /// <summary>
        /// Temayı başlangıçta uygula
        /// </summary>
        public void InitializeTheme()
        {
            ApplyTheme();
        }
    }
}
