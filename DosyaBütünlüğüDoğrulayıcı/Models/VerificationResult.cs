using System;
using System.Windows.Media;

namespace DosyaBütünlüğüDoğrulayıcı.Models
{
    /// <summary>
    /// Dosya hash doğrulama sonuçları
    /// MATCH / MISMATCH / ERROR / UNVERIFIED durumları ile
    /// </summary>
    public class VerificationResult
    {
        public enum VerificationStatus
        {
            Match,          // ✅ Hash'ler eşleşti
            Mismatch,       // ❌ Hash'ler farklı (dosya değişim algılandı)
            Error,          // ⚠️ Tekniker hatası oluştu
            Unverified      // ⏸️ Henüz doğrulanmadı
        }

        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string OldHash { get; set; }
        public string NewHash { get; set; }
        public string Algorithm { get; set; }
        public VerificationStatus Status { get; set; }
        public string Message { get; set; }
        public long TimeTakenMs { get; set; }
        public long FileSize { get; set; }
        public bool IsValid { get; set; }
        public DateTime VerifiedAt { get; set; }
        public string ErrorDetails { get; set; }

        /// <summary>
        /// MATCH durumu - Dosya değişmediği zaman
        /// </summary>
        public static VerificationResult CreateMatch(
            string filePath,
            string fileName,
            string hash,
            string algorithm,
            long timeTakenMs,
            long fileSize)
        {
            return new VerificationResult
            {
                FilePath = filePath,
                FileName = fileName,
                OldHash = hash,
                NewHash = hash,
                Algorithm = algorithm,
                Status = VerificationStatus.Match,
                Message = "✅ Hash'ler eşleşti - Dosya DEĞİŞMEDİ",
                TimeTakenMs = timeTakenMs,
                FileSize = fileSize,
                IsValid = true,
                VerifiedAt = DateTime.Now,
                ErrorDetails = ""
            };
        }

        /// <summary>
        /// MISMATCH durumu - Dosya değiştirilmiş
        /// </summary>
        public static VerificationResult CreateMismatch(
            string filePath,
            string fileName,
            string oldHash,
            string newHash,
            string algorithm,
            long timeTakenMs,
            long fileSize)
        {
            return new VerificationResult
            {
                FilePath = filePath,
                FileName = fileName,
                OldHash = oldHash,
                NewHash = newHash,
                Algorithm = algorithm,
                Status = VerificationStatus.Mismatch,
                Message = "❌ Hash'ler farklı - Dosya DEĞİŞTİRİLMİŞ!",
                TimeTakenMs = timeTakenMs,
                FileSize = fileSize,
                IsValid = false,
                VerifiedAt = DateTime.Now,
                ErrorDetails = ""
            };
        }

        /// <summary>
        /// ERROR durumu - Hata oluştu
        /// </summary>
        public static VerificationResult CreateError(
            string filePath,
            string fileName,
            string errorMessage)
        {
            return new VerificationResult
            {
                FilePath = filePath,
                FileName = fileName,
                OldHash = "",
                NewHash = "",
                Algorithm = "",
                Status = VerificationStatus.Error,
                Message = "⚠️ HATA - Doğrulama başarısız",
                TimeTakenMs = 0,
                FileSize = 0,
                IsValid = false,
                VerifiedAt = DateTime.Now,
                ErrorDetails = errorMessage
            };
        }

        /// <summary>
        /// Sonuç rengini WPF için döndür
        /// Match = Yeşil, Mismatch = Kırmızı, Error = Sarı
        /// </summary>
        public Color GetStatusColor()
        {
            return Status switch
            {
                VerificationStatus.Match => Color.FromRgb(0, 255, 0),        // Yeşil
                VerificationStatus.Mismatch => Color.FromRgb(255, 0, 85),    // Kırmızı
                VerificationStatus.Error => Color.FromRgb(255, 170, 0),      // Turuncu/Sarı
                VerificationStatus.Unverified => Color.FromRgb(100, 100, 100), // Gri
                _ => Color.FromRgb(100, 100, 100)
            };
        }

        /// <summary>
        /// Statüs simgesini döndür
        /// </summary>
        public string GetStatusIcon()
        {
            return Status switch
            {
                VerificationStatus.Match => "✅",
                VerificationStatus.Mismatch => "❌",
                VerificationStatus.Error => "⚠️",
                VerificationStatus.Unverified => "⏸️",
                _ => "❓"
            };
        }

        /// <summary>
        /// Detaylı rapor döndür
        /// </summary>
        public string GetDetailedReport()
        {
            string report = $@"
═══════════════════════════════════════════════════════════
HASH DOĞRULAMA RAPORU
═══════════════════════════════════════════════════════════

Dosya Adı:        {FileName}
Dosya Yolu:       {FilePath}
Dosya Boyutu:     {FormatFileSize(FileSize)}
Algoritma:        {Algorithm}
Durum:            {GetStatusIcon()} {Status}

─────────────────────────────────────────────────────────

Eski Hash:   {OldHash}
Yeni Hash:   {NewHash}

─────────────────────────────────────────────────────────

Mesaj:            {Message}
Doğrulama Tarihi:  {VerifiedAt:yyyy-MM-dd HH:mm:ss}
İşlem Süresi:      {TimeTakenMs}ms

{(Status == VerificationStatus.Error ? $"Hata Detayı: {ErrorDetails}" : "")}

═══════════════════════════════════════════════════════════
";
            return report;
        }

        /// <summary>
        /// Dosya boyutunu formatlı şekilde döndür
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
