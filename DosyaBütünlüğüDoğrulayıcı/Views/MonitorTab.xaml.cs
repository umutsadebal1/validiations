using System.Windows;
using System.Windows.Controls;
using DosyaBütünlüğüDoğrulayıcı.Services;

namespace DosyaBütünlüğüDoğrulayıcı.Views
{
    /// <summary>
    /// İzlet - Dosya izleme sekmesi
    /// </summary>
    public partial class MonitorTab : UserControl
    {
        private FileMonitorService _monitorService;
        private string _selectedFolder;

        public MonitorTab()
        {
            InitializeComponent();
            _monitorService = new FileMonitorService();
            _monitorService.FileChanged += OnFileChanged;
        }

        /// <summary>
        /// Klasör seç dialog'u aç
        /// </summary>
        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            // Display a message asking the user to enter the folder path manually
            MessageBox.Show("Lütfen aşağıdaki metin kutusuna izlemek istediğiniz klasörün yolunu girin.", "Klasör Seç", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// İzlemeyi başlat
        /// </summary>
        private void BtnStartMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFolder))
            {
                MessageBox.Show("Lütfen bir klasör seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_monitorService.StartMonitoring(_selectedFolder))
            {
                BtnStartMonitor.IsEnabled = false;
                BtnStopMonitor.IsEnabled = true;
                TxtMonitorStatus.Text = "Durum: İzleme başladı ✓";
                ListViewMonitor.ItemsSource = _monitorService.MonitoredFiles;
                MessageBox.Show($"İzleme başladı: {_selectedFolder}", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// İzlemeyi durdur
        /// </summary>
        private void BtnStopMonitor_Click(object sender, RoutedEventArgs e)
        {
            _monitorService.StopMonitoring();
            BtnStartMonitor.IsEnabled = true;
            BtnStopMonitor.IsEnabled = false;
            TxtMonitorStatus.Text = "Durum: İzleme durmuş ⏸";
            MessageBox.Show("İzleme durduruldu.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Dosya değişikliği event
        /// </summary>
        private void OnFileChanged(DosyaBütünlüğüDoğrulayıcı.Models.FileMonitorItem item)
        {
            // ListView otomatik olarak ObservableCollection'ı izler
        }
    }
}
