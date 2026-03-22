using System;

namespace DosyaBütünlüğüDoğrulayıcı.Models
{
    /// <summary>
    /// Dosya izleme ListView öğesi modeli
    /// </summary>
    public class FileMonitorItem
    {
        public string FileName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } // Modified, New, Deleted
        public string FullPath { get; set; }

        public FileMonitorItem(string fileName, string fullPath, string status)
        {
            FileName = fileName;
            FullPath = fullPath;
            Status = status;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{FileName} - {Status} ({Timestamp:HH:mm:ss})";
        }
    }
}
