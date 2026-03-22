using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using DosyaBütünlüğüDoğrulayıcı.Models;
using DosyaBütünlüğüDoğrulayıcı.Services;
using System.Windows.Input;

namespace DosyaBütünlüğüDoğrulayıcı.Views
{
    /// <summary>
    /// Main page tab for file and folder hash calculation
    /// </summary>
    public partial class MainTab : Page
    {
        private HashService _hashService;
        private DatabaseService _dbService;
        private string _selectedFilePath;
        private string _selectedFolderPath;
        private List<HashResult> _lastFolderResults = new List<HashResult>();

        public MainTab()
        {
            InitializeComponent();
            _hashService = new HashService();
            _dbService = new DatabaseService();

            // Default algorithm SHA256
            ComboAlgorithm.SelectedIndex = 0;
            ComboAlgorithmFolder.SelectedIndex = 0;
        }

        public void SelectFolderHashTab()
        {
            MainTabControl.SelectedIndex = 1;
        }

        /// <summary>
        /// Open file selection dialog
        /// </summary>
        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                TxtFilePath.Text = _selectedFilePath;
            }
        }

        /// <summary>
        /// Calculate hash for selected file
        /// </summary>
        private async void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                MessageBox.Show("Please select a file!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                BtnCalculate.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
                TxtHashResult.Text = "Calculating...";

                string algorithm = "SHA256";
                if (ComboAlgorithm.SelectedItem is ComboBoxItem selectedItem)
                {
                    algorithm = selectedItem.Content?.ToString() ?? "SHA256";
                }
                var result = await _hashService.CalculateHashAsync(_selectedFilePath, algorithm);

                TxtHashResult.Text = $"{result.Algorithm}:\n{result.FileHash}\n\nFile: {result.FilePath}\nDate: {result.CalculatedAt:yyyy-MM-dd HH:mm:ss}";

                // Save to database
                var historyItem = new DosyaBütünlüğüDoğrulayıcı.Models.HashHistory(
                    _selectedFilePath, 
                    result.FileHash, 
                    algorithm);
                
                _dbService.InsertHashHistory(historyItem);

                BtnCopy.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtHashResult.Text = "";
            }
            finally
            {
                BtnCalculate.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Copy result to clipboard
        /// </summary>
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtHashResult.Text))
            {
                Clipboard.SetText(TxtHashResult.Text);
                MessageBox.Show("Hash copied to clipboard.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Select folder for bulk hashing
        /// </summary>
        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a folder to calculate hashes for all files",
                ShowNewFolderButton = false
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _selectedFolderPath = folderDialog.SelectedPath;
                TxtFolderPath.Text = _selectedFolderPath;
            }
        }

        /// <summary>
        /// Calculate hashes for all files in folder
        /// </summary>
        private async void BtnCalculateFolder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFolderPath))
            {
                MessageBox.Show("Please select a folder!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                BtnCalculateFolder.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
                LstFolderResults.Items.Clear();
                LstFolderResults.Items.Add("Calculating hashes...");

                string algorithm = "SHA256";
                if (ComboAlgorithmFolder.SelectedItem is ComboBoxItem selectedItem)
                {
                    algorithm = selectedItem.Content?.ToString() ?? "SHA256";
                }

                var results = await _hashService.CalculateFolderHashAsync(_selectedFolderPath, algorithm);

                LstFolderResults.Items.Clear();

                if (results.Count == 0)
                {
                    _lastFolderResults.Clear();
                    LstFolderResults.Items.Add("No files found in the selected folder.");
                    BtnExportFolder.IsEnabled = false;
                    return;
                }

                _lastFolderResults = results;

                foreach (var result in results)
                {
                    var shortHashLength = Math.Min(16, result.FileHash?.Length ?? 0);
                    var shortHash = shortHashLength > 0 ? result.FileHash.Substring(0, shortHashLength) : "n/a";
                    var displayText = $"{Path.GetFileName(result.FilePath)} - {shortHash}...";
                    LstFolderResults.Items.Add(displayText);

                    // Save to database
                    var historyItem = new DosyaBütünlüğüDoğrulayıcı.Models.HashHistory(
                        result.FilePath,
                        result.FileHash,
                        algorithm);
                    _dbService.InsertHashHistory(historyItem);
                }

                LstFolderResults.Items.Insert(0, $"✓ Calculated {results.Count} files");
                BtnExportFolder.IsEnabled = true;
                MessageBox.Show($"Successfully calculated hashes for {results.Count} files.", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LstFolderResults.Items.Clear();
            }
            finally
            {
                BtnCalculateFolder.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Export folder hash results
        /// </summary>
        private void BtnExportFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_lastFolderResults == null || _lastFolderResults.Count == 0)
            {
                MessageBox.Show("No folder hash results to export.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Title = "Export Folder Hash Results",
                FileName = $"folder_hash_results_{DateTime.Now:yyyyMMdd_HHmmss}",
                Filter = "CSV Files (*.csv)|*.csv|JSON Files (*.json)|*.json",
                FilterIndex = 1,
                AddExtension = true,
                OverwritePrompt = true
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                var isCsv = saveDialog.FilterIndex == 1 ||
                            saveDialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

                if (isCsv)
                {
                    File.WriteAllText(saveDialog.FileName, BuildFolderResultsCsv(_lastFolderResults), Encoding.UTF8);
                }
                else
                {
                    File.WriteAllText(saveDialog.FileName, BuildFolderResultsJson(_lastFolderResults), Encoding.UTF8);
                }

                MessageBox.Show($"Export completed.\nFile: {saveDialog.FileName}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string BuildFolderResultsCsv(IEnumerable<HashResult> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine("FilePath,FileName,Algorithm,Hash,SizeBytes,CalculatedAt");

            foreach (var item in results)
            {
                sb.AppendLine(
                    $"\"{item.FilePath}\",\"{Path.GetFileName(item.FilePath)}\",\"{item.Algorithm}\",\"{item.FileHash}\",{item.FileSizeBytes},\"{item.CalculatedAt:yyyy-MM-dd HH:mm:ss}\"");
            }

            return sb.ToString();
        }

        private static string BuildFolderResultsJson(IEnumerable<HashResult> results)
        {
            var exportRows = results.Select(item => new
            {
                item.FilePath,
                FileName = Path.GetFileName(item.FilePath),
                item.Algorithm,
                Hash = item.FileHash,
                item.FileSizeBytes,
                CalculatedAt = item.CalculatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return JsonSerializer.Serialize(exportRows, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

