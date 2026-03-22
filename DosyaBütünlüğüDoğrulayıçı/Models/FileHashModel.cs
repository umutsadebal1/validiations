using System;

namespace DosyaBütünlüğüDoğrulayıcı.Models
{
    /// <summary>
    /// Dosya hash bilgisi - Detaylı model
    /// Hash atama ve doğrulama için
    /// </summary>
    public class FileHashModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileHash { get; set; }
        public string Algorithm { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastVerified { get; set; }
        public string Status { get; set; } // OK, CHANGED, ERROR, UNVERIFIED
        public int Version { get; set; }
        public bool IsValid { get; set; }

        public FileHashModel()
        {
            Created = DateTime.Now;
            Status = "UNVERIFIED";
            Version = 1;
            IsValid = true;
        }

        public FileHashModel(string fileName, string filePath, string fileHash, string algorithm, long fileSizeBytes)
        {
            FileName = fileName;
            FilePath = filePath;
            FileHash = fileHash;
            Algorithm = algorithm;
            FileSizeBytes = fileSizeBytes;
            Created = DateTime.Now;
            Status = "OK";
            Version = 1;
            IsValid = true;
        }

        public override string ToString()
        {
            return $"{FileName} ({Algorithm}): {FileHash}";
        }

        /// <summary>
        /// Dosya bilgisini özet olarak döndür
        /// </summary>
        public string GetSummary()
        {
            return $@"
Dosya: {FileName}
Yol: {FilePath}
Boyut: {FormatFileSize(FileSizeBytes)}
Algoritma: {Algorithm}
Durum: {Status}
Hash: {FileHash}
Oluşturuldu: {Created:yyyy-MM-dd HH:mm:ss}
Doğrulandı: {(LastVerified.HasValue ? LastVerified.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Henüz doğrulanmadı")}";
        }

        /// <summary>
        /// Dosya boyutunu okunaklı formata dönüştür
        /// </summary>
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
