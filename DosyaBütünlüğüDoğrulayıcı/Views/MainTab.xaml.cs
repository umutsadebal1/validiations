using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using DosyaBütünlüğüDoğrulayıcı.Services;
using System.Windows.Input;

namespace DosyaBütünlüğüDoğrulayıcı.Views
{
    /// <summary>
    /// Ana Sayfa - Dosya hash hesaplama sekmesi
    /// </summary>
    public partial class MainTab : Page
    {
        private HashService _hashService;
        private DatabaseService _dbService;
        private string _selectedFilePath;

        public MainTab()
        {
            InitializeComponent();
            _hashService = new HashService();
            _dbService = new DatabaseService();

            // Default algorithm SHA256
            ComboAlgorithm.SelectedIndex = 0;
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

                TxtHashResult.Text = $"{result.Algorithm}:\n{result.FileHash}\n\nDosya: {result.FilePath}\nTarih: {result.CalculatedAt:yyyy-MM-dd HH:mm:ss}";

                // Veritabanına kaydet
                var historyItem = new DosyaBütünlüğüDoğrulayıcı.Models.HashHistory(
                    _selectedFilePath, 
                    result.FileHash, 
                    algorithm);
                
                _dbService.InsertHashHistory(historyItem);

                BtnCopy.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtHashResult.Text = "";
            }
            finally
            {
                BtnCalculate.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Sonucu clipboard'a kopyala
        /// </summary>
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtHashResult.Text))
            {
                Clipboard.SetText(TxtHashResult.Text);
                MessageBox.Show("Hash kopyalandı!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
