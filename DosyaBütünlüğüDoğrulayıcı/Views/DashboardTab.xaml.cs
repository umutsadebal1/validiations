using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DosyaBütünlüğüDoğrulayıcı.Services;

namespace DosyaBütünlüğüDoğrulayıcı.Views
{
    public partial class DashboardTab : UserControl
    {
        private DatabaseService _dbService;

        public DashboardTab()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // Load statistics
                var allHashes = _dbService.GetAllHistory();
                StatTotalFiles.Text = allHashes.Count.ToString();
                StatProcessedFiles.Text = allHashes.Count.ToString();
                StatVerified.Text = "0"; // TODO: Track verification status
                StatMismatched.Text = "0"; // TODO: Track mismatches

                // Load recent activities
                if (allHashes.Count > 0)
                {
                    var recentActivities = new ObservableCollection<RecentActivity>();
                    foreach (var hash in allHashes)
                    {
                        recentActivities.Add(new RecentActivity
                        {
                            FileName = System.IO.Path.GetFileName(hash.FilePath),
                            Description = $"{hash.Algorithm} Hash",
                            Timestamp = hash.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }
                    RecentActivityList.ItemsSource = recentActivities;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnQuickHash_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
                mainWindow.MainTabControl.SelectedIndex = 1;
        }

        private void BtnQuickFolder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Klasör hash özelliği yakında eklenecek!", "Bilgi", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnQuickVerify_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
                mainWindow.MainTabControl.SelectedIndex = 2;
        }

        private void BtnQuickSettings_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
                mainWindow.MainTabControl.SelectedIndex = 4;
        }
    }

    public class RecentActivity
    {
        public string FileName { get; set; }
        public string Description { get; set; }
        public string Timestamp { get; set; }
    }
}
