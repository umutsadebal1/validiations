using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DosyaBütünlüğüDoğrulayıçı.Models;

namespace DosyaBütünlüğüDoğrulayıçı.Services
{
    /// <summary>
    /// Hash doğrulama servisi
    /// Önceki hash ile yeni hash'i karşılaştırır
    /// MATCH/MISMATCH/ERROR sonuçları verir
    /// </summary>
    public class VerificationService
    {
        private readonly HashService _hashService;
        private readonly DatabaseService _dbService;

        public event Action<VerificationResult> VerificationCompleted;
        public event Action<string> ProgressUpdated;

        public VerificationService()
        {
            _hashService = new HashService();
            _dbService = new DatabaseService();
        }

        /// <summary>
        /// İki hash'i karşılaştır ve sonuç döndür
        /// </summary>
        public VerificationResult CompareHashes(string filePath, string oldHash, string algorithm)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return VerificationResult.CreateError(filePath, Path.GetFileName(filePath), 
                        "Dosya bulunamadı");
                }

                // Timing
                var stopwatch = Stopwatch.StartNew();

                // Yeni hash hesapla
                ProgressUpdated?.Invoke("Hash hesaplanıyor...");
                
                var newHashResult = _hashService.CalculateHashAsync(filePath, algorithm).Result;
                
                stopwatch.Stop();

                // Karşılaştır
                var fileName = Path.GetFileName(filePath);
                var fileInfo = new FileInfo(filePath);

                if (oldHash.Equals(newHashResult.FileHash, StringComparison.OrdinalIgnoreCase))
                {
                    ProgressUpdated?.Invoke("Doğrulama tamamlandı: MATCH ✅");
                    
                    var result = VerificationResult.CreateMatch(
                        filePath, 
                        fileName, 
                        newHashResult.FileHash,
                        algorithm, 
                        stopwatch.ElapsedMilliseconds,
                        fileInfo.Length
                    );

                    VerificationCompleted?.Invoke(result);
                    return result;
                }
                else
                {
                    ProgressUpdated?.Invoke("Doğrulama tamamlandı: MISMATCH ❌");
                    
                    var result = VerificationResult.CreateMismatch(
                        filePath,
                        fileName,
                        oldHash,
                        newHashResult.FileHash,
                        algorithm,
                        stopwatch.ElapsedMilliseconds,
                        fileInfo.Length
                    );

                    VerificationCompleted?.Invoke(result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                ProgressUpdated?.Invoke("Doğrulama failed: Hata");
                
                var result = VerificationResult.CreateError(
                    filePath, 
                    Path.GetFileName(filePath), 
                    ex.Message
                );

                VerificationCompleted?.Invoke(result);
                return result;
            }
        }

        /// <summary>
        /// Async olarak iki hash'i karşılaştır
        /// </summary>
        public async Task<VerificationResult> CompareHashesAsync(string filePath, string oldHash, string algorithm)
        {
            return await Task.Run(() => CompareHashes(filePath, oldHash, algorithm));
        }

        /// <summary>
        /// Veritabanından dosya hash'ini getir ve doğrula
        /// </summary>
        public VerificationResult VerifyAgainstDatabase(string filePath, string algorithm)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return VerificationResult.CreateError(filePath, Path.GetFileName(filePath),
                        "Dosya bulunamadı");
                }

                // Veritabanından hash al
                var dbHash = _dbService.GetHashByFilePath(filePath);

                if (string.IsNullOrEmpty(dbHash))
                {
                    return VerificationResult.CreateError(filePath, Path.GetFileName(filePath),
                        "Bu dosya veritabanında kayıtlı değil");
                }

                // Karşılaştır
                return CompareHashes(filePath, dbHash, algorithm);
            }
            catch (Exception ex)
            {
                return VerificationResult.CreateError(filePath, Path.GetFileName(filePath),
                    $"Veritabanı hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Async versiyonu
        /// </summary>
        public async Task<VerificationResult> VerifyAgainstDatabaseAsync(string filePath, string algorithm)
        {
            return await Task.Run(() => VerifyAgainstDatabase(filePath, algorithm));
        }

        /// <summary>
        /// Veritabanını doğrulama sonucuna göre güncelle
        /// </summary>
        public bool UpdateVerificationResult(VerificationResult result)
        {
            try
            {
                if (result.Status == VerificationResult.VerificationStatus.Mismatch)
                {
                    // Dosya değiştirildi - veritabanı güncelle
                    return _dbService.UpdateHashRecord(
                        result.FilePath,
                        result.NewHash,
                        "CHANGED"
                    );
                }
                else if (result.Status == VerificationResult.VerificationStatus.Match)
                {
                    // Dosya değişmedi - sadece LastVerified güncelle
                    return _dbService.UpdateLastVerified(result.FilePath);
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Verification update failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Batch doğrulama - Veritabanındaki tüm hashes doğrula
        /// </summary>
        public async Task<int> VerifyAllInDatabaseAsync()
        {
            try
            {
                ProgressUpdated?.Invoke("Toplu doğrulama başladı...");

                var allHashes = _dbService.GetAllHistory();
                int verifiedCount = 0;

                foreach (var hash in allHashes)
                {
                    try
                    {
                        if (!File.Exists(hash.FilePath))
                        {
                            ProgressUpdated?.Invoke($"Atlanıyor: {hash.FileName} (dosya bulunamadı)");
                            continue;
                        }

                        var result = await CompareHashesAsync(
                            hash.FilePath,
                            hash.FileHash,
                            hash.Algorithm
                        );

                        UpdateVerificationResult(result);
                        verifiedCount++;

                        ProgressUpdated?.Invoke($"Doğrulandı: {hash.FileName} ({result.Status})");
                    }
                    catch (Exception ex)
                    {
                        ProgressUpdated?.Invoke($"Hata: {hash.FileName} - {ex.Message}");
                    }
                }

                ProgressUpdated?.Invoke($"Toplu doğrulama tamamlandı: {verifiedCount}/{allHashes.Count}");
                return verifiedCount;
            }
            catch (Exception ex)
            {
                ProgressUpdated?.Invoke($"Toplu doğrulama hatası: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Dosya statüsünü kontrol et (OK/CHANGED/ERROR)
        /// </summary>
        public string GetFileStatus(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return "ERROR";

                var dbHash = _dbService.GetHashByFilePath(filePath);
                if (string.IsNullOrEmpty(dbHash))
                    return "UNVERIFIED";

                // TODO: Veritabanında kaydedilen algoritmasını getir
                var algorithm = "SHA256"; // Default

                var fileHashResult = _hashService.CalculateHashAsync(filePath, algorithm).Result;

                if (fileHashResult.FileHash.Equals(dbHash, StringComparison.OrdinalIgnoreCase))
                    return "OK";
                else
                    return "CHANGED";
            }
            catch
            {
                return "ERROR";
            }
        }

        /// <summary>
        /// Hash algoritmasını doğrula
        /// </summary>
        public bool IsValidAlgorithm(string algorithm)
        {
            var validAlgos = new[] { "SHA256", "SHA512", "MD5", "SHA1" };
            return System.Array.Exists(validAlgos, element => element == algorithm.ToUpper());
        }

        /// <summary>
        /// Hash formatını doğrula (hex string)
        /// </summary>
        public bool IsValidHashFormat(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return false;

            // Hex string kontrolü
            return System.Text.RegularExpressions.Regex.IsMatch(
                hash.ToLower(),
                @"^[a-f0-9]+$"
            );
        }

        /// <summary>
        /// Hash uzunluğunu doğrula (algoritma göre)
        /// </summary>
        public bool IsValidHashLength(string hash, string algorithm)
        {
            int expectedLength = algorithm.ToUpper() switch
            {
                "SHA256" => 64,
                "SHA512" => 128,
                "MD5" => 32,
                "SHA1" => 40,
                _ => 0
            };

            return hash?.Length == expectedLength;
        }
    }
}
