using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DosyaBütünlüğüDoğrulayıcı.Models;

namespace DosyaBütünlüğüDoğrulayıcı.Services
{
    /// <summary>
    /// Dosya hash hesaplama servisi
    /// Desteklenen Algoritmalar: SHA256, SHA512, MD5, SHA1
    /// </summary>
    public class HashService
    {
        /// <summary>
        /// Dosya hash'ini hesapla (asynchronous)
        /// </summary>
        public async Task<HashResult> CalculateHashAsync(string filePath, string algorithm = "SHA256")
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException($"Dosya bulunamadı: {filePath}");

                    var fileInfo = new FileInfo(filePath);
                    string hash = ComputeHash(filePath, algorithm);

                    return new HashResult
                    {
                        FilePath = filePath,
                        FileHash = hash,
                        Algorithm = algorithm,
                        CalculatedAt = DateTime.Now,
                        FileSizeBytes = fileInfo.Length
                    };
                }
                catch (Exception ex)
                {
                    throw new Exception($"Hash hesaplanırken hata: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Hash değerini hesapla (synchronous helper)
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
                        throw new NotSupportedException($"Desteklenmeyen algoritma: {algorithm}");
                }

                return BytesToHex(hashBytes);
            }
        }

        /// <summary>
        /// Byte dizisini hexadecimal string'e dönüştür
        /// </summary>
        private string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Desteklenen algoritmaları döndür
        /// </summary>
        public static string[] GetSupportedAlgorithms()
        {
            return new[] { "SHA256", "SHA512", "MD5", "SHA1" };
        }
    }
}
