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

            // Event handler'ı bağla
            _verificationService.ProgressUpdated += OnProgressUpdated;
            _verificationService.VerificationCompleted += OnVerificationCompleted;
        }

        /// <summary>
        /// Dosya seçme iletişim kutusu
        /// </summary>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Dosya Seçin",
                Filter = "Tüm Dosyalar (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
                HashInfoTextBlock.Text = $"Yol: {openFileDialog.FileName}";

                // Dosya için veritabanında hash olup olmadığını kontrol et
                var dbHash = _databaseService.GetHashByFilePath(openFileDialog.FileName);
                if (!string.IsNullOrEmpty(dbHash))
                {
                    HashInfoTextBlock.Text = $"✅ Veritabanında bulundu";
                    OldHashTextBox.Text = dbHash;
                }
            }
        }

        /// <summary>
        /// Yapıştır düğmesi
        /// </summary>
        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OldHashTextBox.Text = Clipboard.GetText();
            }
            catch
            {
                MessageBox.Show("Pano okunamadı", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Veritabanından yükle
        /// </summary>
        private void LoadFromDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            var filePath = FilePathTextBox.Text;

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Lütfen önce dosya seçin", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dbHash = _databaseService.GetHashByFilePath(filePath);

            if (string.IsNullOrEmpty(dbHash))
            {
                MessageBox.Show("Bu dosya veritabanında kaydedilmiş değil", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OldHashTextBox.Text = dbHash;
            HashInfoTextBlock.Text = "✅ Veritabanından yüklendi";
        }

        /// <summary>
        /// Karşılaştırma işlemini başlat
        /// </summary>
        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            var filePath = FilePathTextBox.Text;
            var oldHash = OldHashTextBox.Text.Trim();
            var algorithm = (AlgorithmComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "SHA256";

            // Validasyon
            if (string.IsNullOrEmpty(filePath))
            {
                ShowError("Lütfen dosya seçin");
                return;
            }

            if (string.IsNullOrEmpty(oldHash))
            {
                ShowError("Lütfen eski hash'i girin");
                return;
            }

            if (!_verificationService.IsValidHashFormat(oldHash))
            {
                ShowError("Geçersiz hash formatı. Hash'in hex karakter (0-9, a-f) içermesi gerekir");
                return;
            }

            if (!_verificationService.IsValidHashLength(oldHash, algorithm))
            {
                ShowError($"Geçersiz hash uzunluğu. {algorithm} için {(algorithm switch { "SHA256" => 64, "SHA512" => 128, "MD5" => 32, "SHA1" => 40, _ => "?" })} karakter olmalı");
                return;
            }

            // UI güncelle
            ProgressBar.Value = 0;
            ResultBorder.Visibility = Visibility.Collapsed;
            ErrorBorder.Visibility = Visibility.Collapsed;
            CompareButton.IsEnabled = false;

            // Karşılaştırmayı async yap
            _ = _verificationService.CompareHashesAsync(filePath, oldHash, algorithm);
        }

        /// <summary>
        /// İlerleme güncellemesi
        /// </summary>
        private void OnProgressUpdated(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressTextBlock.Text = message;
                ProgressBar.Value = Math.Min(ProgressBar.Value + 25, 95); // Plus animasyon
            });
        }

        /// <summary>
        /// Doğrulama tamamlandı
        /// </summary>
        private void OnVerificationCompleted(VerificationResult result)
        {
            Dispatcher.Invoke(() =>
            {
                _lastResult = result;
                ProgressBar.Value = 100;
                CompareButton.IsEnabled = true;

                // Sonuç göster
                DisplayResult(result);
            });
        }

        /// <summary>
        /// Sonucu ekranda göster
        /// </summary>
        private void DisplayResult(VerificationResult result)
        {
            ErrorBorder.Visibility = Visibility.Collapsed;

            if (result.Status == VerificationResult.VerificationStatus.Error)
            {
                ShowError(result.ErrorDetails);
                return;
            }

            // Renk ayarla
            var statusColor = result.GetStatusColor();
            ResultBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(statusColor);
            ResultBorder.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(30, statusColor.R, statusColor.G, statusColor.B)
            );

            // Metin güncelle
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

            // Hash'ler
            ResultOldHash.Text = result.OldHash;
            ResultNewHash.Text = result.NewHash;

            // Düğmeleri göster/gizle
            UpdateDatabaseButton.Visibility = result.Status == VerificationResult.VerificationStatus.Mismatch 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            ResultBorder.Visibility = Visibility.Visible;
            ProgressTextBlock.Text = "Doğrulama tamamlandı";
        }

        /// <summary>
        /// Hata göster
        /// </summary>
        private void ShowError(string message)
        {
            ResultBorder.Visibility = Visibility.Collapsed;
            ErrorMessageTextBlock.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
            ProgressTextBlock.Text = "Hata oluştu";
            ProgressBar.Value = 0;
            CompareButton.IsEnabled = true;
        }

        /// <summary>
        /// Veritabanını güncelle
        /// </summary>
        private void UpdateDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResult == null)
                return;

            bool success = _verificationService.UpdateVerificationResult(_lastResult);

            if (success)
            {
                MessageBox.Show("Veritabanı başarıyla güncellendi", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearButton_Click(null, null);
            }
            else
            {
                MessageBox.Show("Veritabanı güncellenirken hata oluştu", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Detaylı rapor göster
        /// </summary>
        private void DetailedReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResult == null)
                return;

            var report = _lastResult.GetDetailedReport();
            Clipboard.SetText(report);
            MessageBox.Show("Rapor panoya kopyalandı:\r\n\r\n" + report, "Detaylı Rapor", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Hash'i kopyala
        /// </summary>
        private void CopyHashButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResult == null)
                return;

            try
            {
                Clipboard.SetText(_lastResult.NewHash);
                MessageBox.Show("Hash başarıyla kopyalandı", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Hash kopyalanamadı", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Formu temizle
        /// </summary>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FilePathTextBox.Text = "";
            OldHashTextBox.Text = "";
            AlgorithmComboBox.SelectedIndex = 0;
            ProgressBar.Value = 0;
            ProgressTextBlock.Text = "Hazır";
            ResultBorder.Visibility = Visibility.Collapsed;
            ErrorBorder.Visibility = Visibility.Collapsed;
            HashInfoTextBlock.Text = "Dosya seçilmedi";
            _lastResult = null;
        }

        /// <summary>
        /// Dosya boyutunu formatlı göster
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
