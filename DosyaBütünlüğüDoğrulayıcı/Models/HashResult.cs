using System;

namespace DosyaBütünlüğüDoğrulayıcı.Models
{
    /// <summary>
    /// Hash hesaplama sonuçlarını temsil eden model
    /// </summary>
    public class HashResult
    {
        public string FilePath { get; set; }
        public string FileHash { get; set; }
        public string Algorithm { get; set; }
        public DateTime CalculatedAt { get; set; }
        public long FileSizeBytes { get; set; }

        public HashResult()
        {
            CalculatedAt = DateTime.Now;
        }

        public HashResult(string filePath, string fileHash, string algorithm)
        {
            FilePath = filePath;
            FileHash = fileHash;
            Algorithm = algorithm;
            CalculatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Algorithm}: {FileHash}";
        }
    }
}
