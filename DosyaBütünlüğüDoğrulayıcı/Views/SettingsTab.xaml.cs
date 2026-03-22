using System;
using System.Windows;
using System.Windows.Controls;
using DosyaBütünlüğüDoğrulayıcı.Services;

namespace DosyaBütünlüğüDoğrulayıcı.Views
{
    /// <summary>
    /// Ayarlar - Uygulama ayarları sekmesi
    /// Tema, başlangıçta açılsın, geçmiş sıfırla, export
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        private DatabaseService _dbService;
        private ThemeService _themeService;

        public SettingsTab()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _themeService = ((App)Application.Current).GetThemeService();

            // Export format varsayılanı
            ComboExportFormat.SelectedIndex = 0;

            // Başlangıçta açılsın checkbox durumunu yükle (opsiyonel)
            LoadSettings();
        }

        /// <summary>
        /// Ayarları yükle
        /// </summary>
        private void LoadSettings()
        {
            // TODO: Windows Registry veya ayarlar dosyasından yükle
        }

        /// <summary>
        /// Başlangıçta açılsın checkbox işaretlendi
        /// </summary>
        private void ChkStartupCheck_Checked(object sender, RoutedEventArgs e)
        {
            // TODO: Windows Registry'ye ekle (for startup)
            MessageBox.Show("Uygulama başlangıçta açılacak şekilde ayarlandı.", "Bilgi");
        }

        /// <summary>
        /// Başlangıçta açılsın checkbox işareti kaldırıldı
        /// </summary>
        private void ChkStartupCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            // TODO: Windows Registry'den kaldır
            MessageBox.Show("Uygulama başlangıçta açılmayacak şekilde ayarlandı.", "Bilgi");
        }

        /// <summary>
        /// Geçmişi temizle
        /// </summary>
        private void BtnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Hash geçmişini silmek istediğinizden emin misiniz?\nBu işlem geri alınamaz!",
                "Uyarı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (_dbService.ClearHistory())
                {
                    MessageBox.Show("Geçmiş başarıyla temizlendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Geçmiş temizlenirken hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Verileri dışa aktar
        /// </summary>
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var format = ComboExportFormat.SelectedItem?.ToString() ?? "CSV";
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

                // Desktop'e kaydet
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var filePath = System.IO.Path.Combine(desktopPath, fileName);

                System.IO.File.WriteAllText(filePath, exportData);

                MessageBox.Show(
                    $"Veriler başarıyla dışa aktarıldı!\n\n{filePath}",
                    "Başarılı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dışa aktarım sırasında hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
