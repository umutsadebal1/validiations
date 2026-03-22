using System;

namespace DosyaBütünlüğüDoğrulayıcı.Models
{
    /// <summary>
    /// Detaylı dosya hash bilgisi
    /// Dosya doğrulama ve takip için kullanılır
    /// </summary>
    public class FileHashModel
    {
        /// <summary>
        /// Veritabanında unique ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Dosya adı
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Tam dosya yolu
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Hash değeri (Hex string)
        /// </summary>
        public string FileHash { get; set; }

        /// <summary>
        /// Kullanılan algoritma (SHA256, SHA512, MD5, SHA1)
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// Dosya boyutu (byte cinsinden)
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Hash'in oluşturulduğu tarih
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Son doğrulama tarihi
        /// </summary>
        public DateTime LastVerified { get; set; }

        /// <summary>
        /// Dosya durum (OK, CHANGED, ERROR, UNVERIFIED)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Kayıt versiyonu (tracking için)
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Dosyanın doğru olup olmadığı
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Hash'i oluşturan kullanıcı
        /// </summary>
        public string CreatedBy { get; set; }

        public FileHashModel()
        {
            Created = DateTime.Now;
            LastVerified = DateTime.Now;
            Status = "UNVERIFIED";
            Version = 1;
            IsValid = false;
            Algorithm = "SHA256";
            CreatedBy = Environment.UserName;
        }

        /// <summary>
        /// Kısa öz döndür
        /// </summary>
        public string GetSummary()
        {
            return $"{FileName} - {Algorithm} - {Status}";
        }

        /// <summary>
        /// Dosya boyutunu formatlı döndür (B, KB, MB, GB)
        /// </summary>
        public string FormatFileSize()
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = FileSizeBytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Hash'in ilk 16 karakterini döndür (display için)
        /// </summary>
        public string GetHashPreview()
        {
            if (string.IsNullOrEmpty(FileHash))
                return "N/A";
            
            return FileHash.Length > 16 ? FileHash.Substring(0, 16) + "..." : FileHash;
        }

        /// <summary>
        /// Durum açıklaması döndür
        /// </summary>
        public string GetStatusDescription()
        {
            return Status switch
            {
                "OK" => "✅ Dosya değişim yok",
                "CHANGED" => "❌ Dosya değiştirildi",
                "ERROR" => "⚠️ Hata oluştu",
                "UNVERIFIED" => "⏸️ Henüz doğrulanmadı",
                _ => "❓ Bilinmeyen durum"
            };
        }

        /// <summary>
        /// Güncelleme sonrası örneğini kopyala
        /// </summary>
        public FileHashModel Clone()
        {
            return new FileHashModel
            {
                Id = this.Id,
                FileName = this.FileName,
                FilePath = this.FilePath,
                FileHash = this.FileHash,
                Algorithm = this.Algorithm,
                FileSizeBytes = this.FileSizeBytes,
                Created = this.Created,
                LastVerified = this.LastVerified,
                Status = this.Status,
                Version = this.Version,
                IsValid = this.IsValid,
                CreatedBy = this.CreatedBy
            };
        }
    }
}
