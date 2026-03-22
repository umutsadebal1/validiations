using System;

namespace DosyaBütünlüğüDoğrulayıcı.Models
{
    /// <summary>
    /// Hash doğrulama sonucu modeli
    /// MATCH, MISMATCH, ERROR durumları
    /// </summary>
    public class VerificationResult
    {
        public enum VerificationStatus
        {
            Match,          // Hashler eşleşti ✅
            Mismatch,       // Hashler uyuşmadı ❌
            Error,          // Hata oluştu ⚠️
            Unverified      // Henüz doğrulanmadı
        }

        public int FileHashLogId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Algorithm { get; set; }

        public string OldHash { get; set; }
        public string NewHash { get; set; }

        public VerificationStatus Status { get; set; }
        public string Message { get; set; }
        public DateTime VerifiedAt { get; set; }
        public long TimeTakenMs { get; set; } // Hesaplama süresi (ms)

        public long FileSize { get; set; }
        public bool IsValid { get; set; }
        public string ErrorDetails { get; set; }

        public VerificationResult()
        {
            VerifiedAt = DateTime.Now;
            Status = VerificationStatus.Unverified;
            IsValid = false;
        }

        /// <summary>
        /// MATCH sonucu oluştur
        /// </summary>
        public static VerificationResult CreateMatch(string filePath, string fileName, string hash, string algorithm, long timeTaken, long fileSize)
        {
            return new VerificationResult
            {
                FilePath = filePath,
                FileName = fileName,
                OldHash = hash,
                NewHash = hash,
                Algorithm = algorithm,
                Status = VerificationStatus.Match,
                Message = "✅ MATCH - Dosya değişmedi",
                VerifiedAt = DateTime.Now,
                TimeTakenMs = timeTaken,
                FileSize = fileSize,
                IsValid = true
            };
        }

        /// <summary>
        /// MISMATCH sonucu oluştur
        /// </summary>
        public static VerificationResult CreateMismatch(string filePath, string fileName, string oldHash, string newHash, string algorithm, long timeTaken, long fileSize)
        {
            return new VerificationResult
            {
                FilePath = filePath,
                FileName = fileName,
                OldHash = oldHash,
                NewHash = newHash,
                Algorithm = algorithm,
                Status = VerificationStatus.Mismatch,
                Message = "❌ MISMATCH - Dosya değiştirildi!",
                VerifiedAt = DateTime.Now,
                TimeTakenMs = timeTaken,
                FileSize = fileSize,
                IsValid = false
            };
        }

        /// <summary>
        /// ERROR sonucu oluştur
        /// </summary>
        public static VerificationResult CreateError(string filePath, string fileName, string errorMsg)
        {
            return new VerificationResult
            {
                FilePath = filePath,
                FileName = fileName,
                Status = VerificationStatus.Error,
                Message = "⚠️ HATA - Doğrulama başarısız",
                VerifiedAt = DateTime.Now,
                IsValid = false,
                ErrorDetails = errorMsg
            };
        }

        /// <summary>
        /// Doğrulama sonucunun rengini döndür (WPF binding için)
        /// </summary>
        public System.Windows.Media.Color GetStatusColor()
        {
            return Status switch
            {
                VerificationStatus.Match => System.Windows.Media.Color.FromRgb(0, 255, 0),      // 🟢 Yeşil
                VerificationStatus.Mismatch => System.Windows.Media.Color.FromRgb(255, 0, 85),  // 🔴 Kırmızı
                VerificationStatus.Error => System.Windows.Media.Color.FromRgb(255, 170, 0),    // 🟡 Sarı
                _ => System.Windows.Media.Color.FromRgb(128, 128, 128)                           // ⚪ Gri
            };
        }

        /// <summary>
        /// Detaylı rapor oluştur
        /// </summary>
        public string GetDetailedReport()
        {
            var report = $@"
╔════════════════════════════════════════════════════════╗
║               HASH DOĞRULAMA RAPORU                   ║
╚════════════════════════════════════════════════════════╝

📄 DOSYA BİLGİSİ:
   Adı: {FileName}
   Yolu: {FilePath}
   Boyut: {FormatFileSize(FileSize)}
   İşlem Süresi: {TimeTakenMs} ms

🔐 HASH BİLGİSİ:
   Algoritma: {Algorithm}

📊 DOĞRULAMA SONUCU:
   Durum: {Status}
   Mesaj: {Message}
   Tarih: {VerifiedAt:yyyy-MM-dd HH:mm:ss}

🔗 HASH KARŞILAŞTIRMASI:
   Eski Hash: {OldHash}
   Yeni Hash: {NewHash}
";

            if (Status == VerificationStatus.Error && !string.IsNullOrEmpty(ErrorDetails))
            {
                report += $@"
⚠️ HATA DETAYLARI:
   {ErrorDetails}
";
            }

            report += "\n╚════════════════════════════════════════════════════════╝";

            return report;
        }

        private string FormatFileSize(long bytes)
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
