using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using DosyaBütünlüğüDoğrulayıcı.Models;
using DosyaBütünlüğüDoğrulayıcı.Services;

namespace DosyaBütünlüğüDoğrulayıcı.Views
{
    public partial class VerifyTab : UserControl
    {
        private VerificationService _verificationService;
        private DatabaseService _databaseService;
        private VerificationResult _lastResult;

        public VerifyTab()
        {
            InitializeComponent();
            _verificationService = new VerificationService();
            _databaseService = new DatabaseService();

            // Wire up event handlers.
            _verificationService.ProgressUpdated += OnProgressUpdated;
            _verificationService.VerificationCompleted += OnVerificationCompleted;
        }

        /// <summary>
        /// Open file selection dialog
        /// </summary>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select File",
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
                HashInfoTextBlock.Text = $"Path: {openFileDialog.FileName}";

                // Check whether there is a stored hash for the selected file.
                var dbHash = _databaseService.GetHashByFilePath(openFileDialog.FileName);
                if (!string.IsNullOrEmpty(dbHash))
                {
                    HashInfoTextBlock.Text = "✅ Found in database";
                    OldHashTextBox.Text = dbHash;
                }
            }
        }

        /// <summary>
        /// Paste button handler
        /// </summary>
        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OldHashTextBox.Text = Clipboard.GetText();
            }
            catch
            {
                MessageBox.Show("Clipboard could not be read.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load hash value from database
        /// </summary>
        private void LoadFromDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            var filePath = FilePathTextBox.Text;

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Please select a file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dbHash = _databaseService.GetHashByFilePath(filePath);

            if (string.IsNullOrEmpty(dbHash))
            {
                MessageBox.Show("This file is not stored in the database.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OldHashTextBox.Text = dbHash;
            HashInfoTextBlock.Text = "✅ Loaded from database";
        }

        /// <summary>
        /// Start comparison operation
        /// </summary>
        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            var filePath = FilePathTextBox.Text;
            var oldHash = OldHashTextBox.Text.Trim();
            var algorithm = (AlgorithmComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "SHA256";

            // Validation
            if (string.IsNullOrEmpty(filePath))
            {
                ShowError("Please select a file.");
                return;
            }

            if (string.IsNullOrEmpty(oldHash))
            {
                ShowError("Please enter the original hash.");
                return;
            }

            if (!_verificationService.IsValidHashFormat(oldHash))
            {
                ShowError("Invalid hash format. The hash must contain hexadecimal characters (0-9, a-f).");
                return;
            }

            if (!_verificationService.IsValidHashLength(oldHash, algorithm))
            {
                ShowError($"Invalid hash length. {algorithm} requires {(algorithm switch { "SHA256" => 64, "SHA512" => 128, "MD5" => 32, "SHA1" => 40, _ => "?" })} characters.");
                return;
            }

            // Update UI
            ProgressBar.Value = 0;
            ResultBorder.Visibility = Visibility.Collapsed;
            ErrorBorder.Visibility = Visibility.Collapsed;
            CompareButton.IsEnabled = false;

            // Run comparison asynchronously
            _ = _verificationService.CompareHashesAsync(filePath, oldHash, algorithm);
        }

        /// <summary>
        /// Progress update handler
        /// </summary>
        private void OnProgressUpdated(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressTextBlock.Text = message;
                ProgressBar.Value = Math.Min(ProgressBar.Value + 25, 95);
            });
        }

        /// <summary>
        /// Verification completed handler
        /// </summary>
        private void OnVerificationCompleted(VerificationResult result)
        {
            Dispatcher.Invoke(() =>
            {
                _lastResult = result;
                ProgressBar.Value = 100;
                CompareButton.IsEnabled = true;

                // Display result
                DisplayResult(result);
            });
        }

        /// <summary>
        /// Show result on screen
        /// </summary>
        private void DisplayResult(VerificationResult result)
        {
            ErrorBorder.Visibility = Visibility.Collapsed;

            if (result.Status == VerificationResult.VerificationStatus.Error)
            {
                ShowError(result.ErrorDetails);
                return;
            }

            // Set status color styling.
            var statusColor = result.GetStatusColor();
            ResultBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(statusColor);
            ResultBorder.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(30, statusColor.R, statusColor.G, statusColor.B)
            );

            // Update text fields.
            ResultStatusIcon.Text = result.GetStatusIcon();
            ResultStatusText.Text = result.Message;
            ResultStatusText.Foreground = new System.Windows.Media.SolidColorBrush(statusColor);

            ResultFileName.Text = result.FileName;
            ResultFileSize.Text = result.FileSize > 0 
                ? FormatFileSize(result.FileSize) 
                : "N/A";
            ResultAlgorithm.Text = result.Algorithm;
            ResultTime.Text = $"{result.TimeTakenMs}ms";
            ResultDateTime.Text = result.VerifiedAt.ToString("yyyy-MM-dd HH:mm:ss");

            // Hash values
            ResultOldHash.Text = result.OldHash;
            ResultNewHash.Text = result.NewHash;

            // Toggle action buttons based on result status.
            UpdateDatabaseButton.Visibility = result.Status == VerificationResult.VerificationStatus.Mismatch 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            ResultBorder.Visibility = Visibility.Visible;
            ProgressTextBlock.Text = "Verification completed";
        }

        /// <summary>
        /// Show error state
        /// </summary>
        private void ShowError(string message)
        {
            ResultBorder.Visibility = Visibility.Collapsed;
            ErrorMessageTextBlock.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
            ProgressTextBlock.Text = "An error occurred";
            ProgressBar.Value = 0;
            CompareButton.IsEnabled = true;
        }

        /// <summary>
        /// Update database with verification result
        /// </summary>
        private void UpdateDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResult == null)
                return;

            bool success = _verificationService.UpdateVerificationResult(_lastResult);

            if (success)
            {
                MessageBox.Show("Database updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearButton_Click(null, null);
            }
            else
            {
                MessageBox.Show("An error occurred while updating the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Copy detailed report to clipboard and show dialog
        /// </summary>
        private void DetailedReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResult == null)
                return;

            var report = _lastResult.GetDetailedReport();
            Clipboard.SetText(report);
            MessageBox.Show("Report copied to clipboard:\r\n\r\n" + report, "Detailed Report", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Copy new hash to clipboard
        /// </summary>
        private void CopyHashButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResult == null)
                return;

            try
            {
                Clipboard.SetText(_lastResult.NewHash);
                MessageBox.Show("Hash copied successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Failed to copy hash.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Clear form state
        /// </summary>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FilePathTextBox.Text = "";
            OldHashTextBox.Text = "";
            AlgorithmComboBox.SelectedIndex = 0;
            ProgressBar.Value = 0;
            ProgressTextBlock.Text = "Ready";
            ResultBorder.Visibility = Visibility.Collapsed;
            ErrorBorder.Visibility = Visibility.Collapsed;
            HashInfoTextBlock.Text = "No file selected";
            _lastResult = null;
        }

        /// <summary>
        /// Format file size in human-readable units
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
