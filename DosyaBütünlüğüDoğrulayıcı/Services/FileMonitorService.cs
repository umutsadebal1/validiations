using System;
using System.IO;
using System.Collections.ObjectModel;
using DosyaBütünlüğüDoğrulayıcı.Models;

namespace DosyaBütünlüğüDoğrulayıcı.Services
{
    /// <summary>
    /// Dosya izleme servisi (FileSystemWatcher)
    /// Gerçek-zamanlı dosya değişikliklerini izler
    /// </summary>
    public class FileMonitorService
    {
        private FileSystemWatcher _watcher;
        public event Action<FileMonitorItem> FileChanged;
        public ObservableCollection<FileMonitorItem> MonitoredFiles { get; private set; }

        public FileMonitorService()
        {
            MonitoredFiles = new ObservableCollection<FileMonitorItem>();
        }

        /// <summary>
        /// Belirtilen klasörü izlemeye başla
        /// </summary>
        public bool StartMonitoring(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    throw new DirectoryNotFoundException($"Klasör bulunamadı: {folderPath}");

                if (_watcher != null)
                    StopMonitoring();

                _watcher = new FileSystemWatcher(folderPath)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                    IncludeSubdirectories = true
                };

                _watcher.Created += OnFileCreated;
                // _watcher.Modified += OnFileModified; // TODO: Fix Modified event
                _watcher.Deleted += OnFileDeleted;
                _watcher.Error += OnError;

                _watcher.EnableRaisingEvents = true;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"İzleme başlatılamadı: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// İzlemeyi durdur
        /// </summary>
        public void StopMonitoring()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }

            MonitoredFiles.Clear();
        }

        /// <summary>
        /// Yeni dosya oluşturuldu event
        /// </summary>
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            var item = new FileMonitorItem(Path.GetFileName(e.FullPath), e.FullPath, "Yeni");
            FileChanged?.Invoke(item);
            MonitoredFiles.Add(item);
        }

        /// <summary>
        /// Dosya değiştirildi event
        /// </summary>
        private void OnFileModified(object sender, FileSystemEventArgs e)
        {
            var item = new FileMonitorItem(Path.GetFileName(e.FullPath), e.FullPath, "Değiştirildi");
            FileChanged?.Invoke(item);
            MonitoredFiles.Add(item);
        }

        /// <summary>
        /// Dosya silindi event
        /// </summary>
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            var item = new FileMonitorItem(Path.GetFileName(e.FullPath), e.FullPath, "Silindi");
            FileChanged?.Invoke(item);
            MonitoredFiles.Add(item);
        }

        /// <summary>
        /// Hata event
        /// </summary>
        private void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"İzleme hatası: {e.GetException()?.Message}");
        }

        /// <summary>
        /// İzleme aktif mi kontrol et
        /// </summary>
        public bool IsMonitoring => _watcher?.EnableRaisingEvents ?? false;
    }
}
