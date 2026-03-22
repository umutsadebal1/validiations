using System;

namespace DosyaBütünlüğüDoğrulayıcı.Models
{
    /// <summary>
    /// Hash geçmişi veritabanı modeli
    /// </summary>
    public class HashHistory
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string FileHash { get; set; }
        public string Algorithm { get; set; }
        public DateTime CreatedAt { get; set; }

        public HashHistory()
        {
            CreatedAt = DateTime.Now;
        }

        public HashHistory(string filePath, string fileHash, string algorithm)
        {
            FilePath = filePath;
            FileHash = fileHash;
            Algorithm = algorithm;
            CreatedAt = DateTime.Now;
        }
    }
}
