using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DosyaBütünlüğüDoğrulayıcı.Models;
using DosyaBütünlüğüDoğrulayıcı.Services;

namespace DosyaBütünlüğüDoğrulayıcı.Views
{
    public partial class DashboardTab : UserControl
    {
        private DatabaseService _dbService;
        private HashService _hashService;

        public DashboardTab()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _hashService = new HashService();
            LoadDashboardData();
        }

        public void RefreshDashboard()
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // Load statistics
                var allHashes = _dbService.GetAllHistory();
                var latestPerFile = allHashes
                    .GroupBy(item => item.FilePath, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.OrderByDescending(item => item.CreatedAt).First())
                    .ToList();

                StatTotalFiles.Text = latestPerFile.Count.ToString();
                StatProcessedFiles.Text = allHashes.Count.ToString();
                StatVerified.Text = "...";
                StatMismatched.Text = "...";

                // Load recent activities
                RecentActivityList.ItemsSource = null;
                if (allHashes.Count > 0)
                {
                    var recentActivities = new ObservableCollection<RecentActivity>();
                    foreach (var hash in allHashes.Take(50))
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

                // Compute expensive mismatch stats in background so startup UI does not freeze.
                _ = LoadVerificationStatsAsync(latestPerFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadVerificationStatsAsync(List<HashHistory> latestPerFile)
        {
            try
            {
                int verifiedCount = 0;
                int mismatchCount = 0;

                foreach (var item in latestPerFile)
                {
                    if (!File.Exists(item.FilePath))
                    {
                        continue;
                    }

                    verifiedCount++;

                    var currentHash = await _hashService.CalculateHashAsync(item.FilePath, item.Algorithm);
                    if (!string.Equals(currentHash.FileHash, item.FileHash, StringComparison.OrdinalIgnoreCase))
                    {
                        mismatchCount++;
                    }
                }

                await Dispatcher.InvokeAsync(() =>
                {
                    StatVerified.Text = verifiedCount.ToString();
                    StatMismatched.Text = mismatchCount.ToString();
                });
            }
            catch
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    StatVerified.Text = "0";
                    StatMismatched.Text = "0";
                });
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
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.OpenFolderHashTab();
            }
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
