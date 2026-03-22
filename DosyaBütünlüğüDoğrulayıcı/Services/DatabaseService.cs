using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using DosyaBütünlüğüDoğrulayıcı.Models;

namespace DosyaBütünlüğüDoğrulayıcı.Services
{
    /// <summary>
    /// SQLite veritabanı servisi
    /// Dosya: App.db (AppData klasöründe)
    /// </summary>
    public class DatabaseService
    {
        private readonly string _connectionString;
        private const string DbFileName = "App.db";

        public DatabaseService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrWhiteSpace(appDataPath))
            {
                appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }

            if (string.IsNullOrWhiteSpace(appDataPath))
            {
                appDataPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            var dbPath = Path.Combine(appDataPath, "DosyaBütünlüğüDoğrulayıcı");

            if (!Directory.Exists(dbPath))
                Directory.CreateDirectory(dbPath);

            var dbFile = Path.Combine(dbPath, DbFileName);
            _connectionString = $"Data Source={dbFile};Version=3;";

            InitializeDatabase();
        }

        /// <summary>
        /// Veritabanını ve tabloları başlat
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS HashHistory (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            FilePath TEXT NOT NULL,
                            FileHash TEXT NOT NULL,
                            Algorithm TEXT NOT NULL,
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                        );";

                    using (var command = new SQLiteCommand(createTableSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Veritabanı başlatılamadı: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Hash geçmişine yeni kayıt ekle
        /// </summary>
        public bool InsertHashHistory(HashHistory history)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO HashHistory (FilePath, FileHash, Algorithm, CreatedAt) VALUES (@path, @hash, @algo, @date)";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@path", history.FilePath);
                        command.Parameters.AddWithValue("@hash", history.FileHash);
                        command.Parameters.AddWithValue("@algo", history.Algorithm);
                        command.Parameters.AddWithValue("@date", history.CreatedAt);

                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veri ekleme hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tüm hash geçmişini getir
        /// </summary>
        public List<HashHistory> GetAllHistory()
        {
            var history = new List<HashHistory>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string sql = "SELECT Id, FilePath, FileHash, Algorithm, CreatedAt FROM HashHistory ORDER BY CreatedAt DESC";

                    using (var command = new SQLiteCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            history.Add(new HashHistory
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FilePath = reader["FilePath"].ToString(),
                                FileHash = reader["FileHash"].ToString(),
                                Algorithm = reader["Algorithm"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veri okuma hatası: {ex.Message}");
            }

            return history;
        }

        /// <summary>
        /// Geçmişi temizle
        /// </summary>
        public bool ClearHistory()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string sql = "DELETE FROM HashHistory";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geçmiş silme hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// CSV olarak dışa aktar
        /// </summary>
        public string ExportAsCSV()
        {
            var history = GetAllHistory();
            var csv = new StringBuilder();
            csv.AppendLine("Dosya,Hash,Algoritma,Tarih");

            foreach (var item in history)
            {
                csv.AppendLine($"\"{item.FilePath}\",\"{item.FileHash}\",\"{item.Algorithm}\",\"{item.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
            }

            return csv.ToString();
        }

        /// <summary>
        /// JSON olarak dışa aktar
        /// </summary>
        public string ExportAsJSON()
        {
            var history = GetAllHistory();
            var jsonData = history.Select(h => new
            {
                file = h.FilePath,
                hash = h.FileHash,
                algorithm = h.Algorithm,
                date = h.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            return JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Dosya yoluna göre hash'i getir (Doğrulama için)
        /// </summary>
        public string GetHashByFilePath(string filePath)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string sql = "SELECT FileHash FROM HashHistory WHERE FilePath = @path ORDER BY CreatedAt DESC LIMIT 1";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@path", filePath);
                        var result = command.ExecuteScalar();

                        return result?.ToString() ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hash getme hatası: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Hash kaydını güncelle (doğrulama sonrası)
        /// </summary>
        public bool UpdateHashRecord(string filePath, string newHash, string status = "OK")
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string sql = @"
                        UPDATE HashHistory 
                        SET FileHash = @hash, CreatedAt = @date 
                        WHERE FilePath = @path
                    ";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@path", filePath);
                        command.Parameters.AddWithValue("@hash", newHash);
                        command.Parameters.AddWithValue("@date", DateTime.Now);

                        int rowsUpdated = command.ExecuteNonQuery();
                        return rowsUpdated > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hash güncelleme hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Son doğrulama tarihini güncelle
        /// </summary>
        public bool UpdateLastVerified(string filePath)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string updateSql = @"
                        UPDATE HashHistory 
                        SET CreatedAt = @date
                        WHERE FilePath = @path
                    ";

                    using (var command = new SQLiteCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@path", filePath);
                        command.Parameters.AddWithValue("@date", DateTime.Now);
                        
                        int rowsUpdated = command.ExecuteNonQuery();
                        return rowsUpdated > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Doğrulama tarihi güncelleme hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Hash'e göre kayıt ara
        /// </summary>
        public List<HashHistory> SearchByHash(string hash)
        {
            var results = new List<HashHistory>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string sql = "SELECT Id, FilePath, FileHash, Algorithm, CreatedAt FROM HashHistory WHERE FileHash LIKE @hash";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@hash", $"%{hash}%");

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new HashHistory
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    FilePath = reader["FilePath"].ToString(),
                                    FileHash = reader["FileHash"].ToString(),
                                    Algorithm = reader["Algorithm"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hash arama hatası: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Algoritma'ya göre kayıtları filtrele
        /// </summary>
        public List<HashHistory> FilterByAlgorithm(string algorithm)
        {
            var results = new List<HashHistory>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string sql = "SELECT Id, FilePath, FileHash, Algorithm, CreatedAt FROM HashHistory WHERE Algorithm = @algo ORDER BY CreatedAt DESC";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@algo", algorithm);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new HashHistory
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    FilePath = reader["FilePath"].ToString(),
                                    FileHash = reader["FileHash"].ToString(),
                                    Algorithm = reader["Algorithm"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Filtre hatası: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Tarih aralığına göre kayıtları getir
        /// </summary>
        public List<HashHistory> GetHistoryByDateRange(DateTime startDate, DateTime endDate)
        {
            var results = new List<HashHistory>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string sql = @"
                        SELECT Id, FilePath, FileHash, Algorithm, CreatedAt 
                        FROM HashHistory 
                        WHERE CreatedAt BETWEEN @start AND @end 
                        ORDER BY CreatedAt DESC
                    ";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@start", startDate);
                        command.Parameters.AddWithValue("@end", endDate);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new HashHistory
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    FilePath = reader["FilePath"].ToString(),
                                    FileHash = reader["FileHash"].ToString(),
                                    Algorithm = reader["Algorithm"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tarih aralığı hatası: {ex.Message}");
            }

            return results;
        }
    }
}
