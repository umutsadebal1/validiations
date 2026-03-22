using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DosyaBütünlüğüDoğrulayıcı.Models;

namespace DosyaBütünlüğüDoğrulayıcı.Services
{
    /// <summary>
    /// File hash calculation service
    /// Supported Algorithms: SHA256, SHA512, MD5, SHA1
    /// </summary>
    public class HashService
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DosyaBütünlüğüDoğrulayıcı",
            "Logs"
        );

        public HashService()
        {
            // Ensure log directory exists
            if (!Directory.Exists(LogPath))
                Directory.CreateDirectory(LogPath);
        }

        /// <summary>
        /// Calculate file hash (asynchronous)
        /// </summary>
        public async Task<HashResult> CalculateHashAsync(string filePath, string algorithm = "SHA256")
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException($"File not found: {filePath}");

                    var fileInfo = new FileInfo(filePath);
                    string hash = ComputeHash(filePath, algorithm);

                    var result = new HashResult
                    {
                        FilePath = filePath,
                        FileHash = hash,
                        Algorithm = algorithm,
                        CalculatedAt = DateTime.Now,
                        FileSizeBytes = fileInfo.Length
                    };

                    LogOperation($"File hash calculated: {Path.GetFileName(filePath)} ({algorithm})");
                    return result;
                }
                catch (Exception ex)
                {
                    LogError($"Error calculating hash for {filePath}: {ex.Message}");
                    throw new Exception($"Error calculating hash: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Calculate hashes for all files in a folder
        /// </summary>
        public async Task<List<HashResult>> CalculateFolderHashAsync(string folderPath, string algorithm = "SHA256")
        {
            return await Task.Run(() =>
            {
                var results = new List<HashResult>();

                try
                {
                    if (!Directory.Exists(folderPath))
                        throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

                    var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                    LogOperation($"Starting folder hash calculation: {folderPath} ({files.Length} files, {algorithm})");

                    foreach (var file in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            string hash = ComputeHash(file, algorithm);

                            results.Add(new HashResult
                            {
                                FilePath = file,
                                FileHash = hash,
                                Algorithm = algorithm,
                                CalculatedAt = DateTime.Now,
                                FileSizeBytes = fileInfo.Length
                            });
                        }
                        catch (Exception ex)
                        {
                            LogError($"Skipped file {file}: {ex.Message}");
                        }
                    }

                    LogOperation($"Folder hash calculation completed: {results.Count} files processed");
                    return results;
                }
                catch (Exception ex)
                {
                    LogError($"Error calculating folder hash for {folderPath}: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Compute hash value (synchronous helper)
        /// </summary>
        private string ComputeHash(string filePath, string algorithm)
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes;

                switch (algorithm.ToUpper())
                {
                    case "SHA256":
                        using (var sha256 = SHA256.Create())
                            hashBytes = sha256.ComputeHash(stream);
                        break;

                    case "SHA512":
                        using (var sha512 = SHA512.Create())
                            hashBytes = sha512.ComputeHash(stream);
                        break;

                    case "MD5":
                        using (var md5 = MD5.Create())
                            hashBytes = md5.ComputeHash(stream);
                        break;

                    case "SHA1":
                        using (var sha1 = SHA1.Create())
                            hashBytes = sha1.ComputeHash(stream);
                        break;

                    default:
                        throw new NotSupportedException($"Unsupported algorithm: {algorithm}");
                }

                return BytesToHex(hashBytes);
            }
        }

        /// <summary>
        /// Convert byte array to hexadecimal string
        /// </summary>
        private string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Get supported algorithms
        /// </summary>
        public static string[] GetSupportedAlgorithms()
        {
            return new[] { "SHA256", "SHA512", "MD5", "SHA1" };
        }

        /// <summary>
        /// Log operation to file
        /// </summary>
        private void LogOperation(string message)
        {
            try
            {
                string logFile = Path.Combine(LogPath, $"hash_log_{DateTime.Now:yyyy-MM-dd}.txt");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch { /* Log file write errors are silently ignored */ }
        }

        /// <summary>
        /// Log error to file
        /// </summary>
        private void LogError(string message)
        {
            try
            {
                string logFile = Path.Combine(LogPath, $"hash_error_{DateTime.Now:yyyy-MM-dd}.txt");
                string logEntry = $"[ERROR] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch { /* Log file write errors are silently ignored */ }
        }
    }
}
