using System.Windows;
using System.Windows.Controls;
using DosyaBütünlüğüDoğrulayıcı.Services;

namespace DosyaBütünlüğüDoğrulayıcı.Views
{
    /// <summary>
    /// File monitoring tab
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
        /// Open folder selection dialog
        /// </summary>
        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a folder to monitor",
                ShowNewFolderButton = false
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _selectedFolder = folderDialog.SelectedPath;
                TxtFolderPath.Text = _selectedFolder;
            }
        }

        /// <summary>
        /// Start monitoring
        /// </summary>
        private void BtnStartMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedFolder) && !string.IsNullOrWhiteSpace(TxtFolderPath.Text))
            {
                _selectedFolder = TxtFolderPath.Text.Trim();
            }

            if (string.IsNullOrEmpty(_selectedFolder))
            {
                MessageBox.Show("Please select a folder first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_monitorService.StartMonitoring(_selectedFolder))
            {
                BtnStartMonitor.IsEnabled = false;
                BtnStopMonitor.IsEnabled = true;
                TxtMonitorStatus.Text = "Status: Monitoring started ✓";
                ListViewMonitor.ItemsSource = _monitorService.MonitoredFiles;
                MessageBox.Show($"Monitoring started: {_selectedFolder}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Stop monitoring
        /// </summary>
        private void BtnStopMonitor_Click(object sender, RoutedEventArgs e)
        {
            _monitorService.StopMonitoring();
            BtnStartMonitor.IsEnabled = true;
            BtnStopMonitor.IsEnabled = false;
            TxtMonitorStatus.Text = "Status: Monitoring stopped ⏸";
            MessageBox.Show("Monitoring stopped.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// File changed event callback
        /// </summary>
        private void OnFileChanged(DosyaBütünlüğüDoğrulayıcı.Models.FileMonitorItem item)
        {
            // ListView refreshes automatically through ObservableCollection binding.
        }
    }
}
