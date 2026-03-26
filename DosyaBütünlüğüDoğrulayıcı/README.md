# Dosya Bütünlüğü Doğrulayıcı (VALIDC#) v1.0

## 📋 Proje Açıklaması

WPF tabanlı masaüstü uygulaması olarak geliştirilmiş File Integrity Validator. Dosyaların SHA256, SHA512, MD5 ve SHA1 hash'lerini hesaplayarak dosya bütünlüğünü doğrulamaya yarar.

### Temel Özellikler
- ✅ Hash Hesaplama (SHA256, SHA512, MD5, SHA1)
- ✅ Dosya İzleme (Real-time FileSystemWatcher)
- ✅ Hash Geçmişi (SQLite Database)
- ✅ CSV/JSON Export
- ✅ Koyu/Açık Tema (Dark/Light Mode)
- ✅ Türkçe Arayüz

---

## 🛠️ Teknik Özellikleri

### Teknoloji Stack
- **Framework**: .NET 6.0 (scalable to .NET Framework 4.7.2)
- **UI**: WPF (Windows Presentation Foundation)
- **Database**: SQLite (System.Data.SQLite)
- **Language**: C# 10.0+
- **IDE**: Visual Studio 2022

### Proje Yapısı

```
DosyaBütünlüğüDoğrulayıcı/
├── App.xaml / App.xaml.cs                 # Uygulama başlangıç
├── MainWindow.xaml / MainWindow.xaml.cs   # Ana pencere (Sidebar + TabControl)
├── Models/
│   ├── HashResult.cs                      # Hash sonuç modeli
│   ├── HashHistory.cs                     # Veritabanı model
│   └── FileMonitorItem.cs                 # İzleme list item'ı
├── Services/
│   ├── HashService.cs                     # Hash hesaplama (SHA256, SHA512, MD5, SHA1)
│   ├── DatabaseService.cs                 # SQLite (CRUD, Export)
│   ├── FileMonitorService.cs              # FileSystemWatcher
│   └── ThemeService.cs                    # Tema yönetimi
├── Views/
│   ├── MainTab.xaml / MainTab.xaml.cs     # Ana sayfa (dosya seç, hash hesapla)
│   ├── MonitorTab.xaml / MonitorTab.xaml.cs # İzlet (klasör izleme)
│   └── SettingsTab.xaml / SettingsTab.xaml.cs # Ayarlar (tema, export, geçmiş)
├── Resources/
│   ├── DarkTheme.xaml                     # Koyu tema stilleri
│   ├── LightTheme.xaml                    # Açık tema stilleri
│   ├── app_icon.ico                       # Uygulama ikonu
│   └── app_logo.png                       # Logo
└── DosyaBütünlüğüDoğrulayıcı.csproj       # Proje dosyası
```

---

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
- Windows 10+ veya Windows Server 2016+
- .NET 6.0 Runtime (veya .NET Framework 4.7.2)
- Visual Studio 2022 (geliştirme için)

### Derleme
1. Proje klasörüne girin:
   ```bash
   cd DosyaBütünlüğüDoğrulayıcı
   ```

2. NuGet paketlerini geri yükleyin:
   ```bash
   dotnet restore
   ```

3. Projeyi derleyin:
   ```bash
   dotnet build --configuration Release
   ```

4. Uygulamayı çalıştırın:
   ```bash
   dotnet run
   ```

### Tek Dosya Olarak Yayınlama (xcopy Deploy)
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

Çıktı dosyası: `bin/Release/net6.0-windows/win-x64/publish/DosyaBütünlüğüDoğrulayıcı.exe`

---

## 📖 Kullanıcı Rehberi

### 1️⃣ Ana Sayfa - Hash Hesaplama
1. **Dosya Seç** butonuna tıklayın
2. HashlanmakIstenen dosyayı seçin
3. **Hash Algoritması** dropdown'dan bir algoritma seçin (varsayılan: SHA256)
4. **Hesapla** butonuna tıklayın
5. Hash sonuçları görüntülenecek ve otomatik olarak veritabanına kaydedilecek
6. **Kopyala** ile hash'i clipboard'a kopyalayabilirsiniz

### 2️⃣ İzlet - Dosya İzleme
1. **Klasör Seç** butonuna tıklayarak izlenmek istenen klasörü seçin
2. **İzlemeyi Başlat** butonuna tıklayın
3. Klasördeki dosya değişiklikleri listelenecek:
   - **Yeni**: Yeni dosya oluşturuldu
   - **Değiştirildi**: Dosya değiştirildi
   - **Silindi**: Dosya silindi
4. **İzlemeyi Durdur** ile izlemeyi sonlandırabilirsiniz

### 3️⃣ Ayarlar
- **Görünüm**: Tema değişimleri (MainWindow'den de yapılabilir)
- **Başlangıçta Açılsın**: Windows başlangıcında uygulamayı otomatik aç
- **Hash Geçmişi**: Tüm hash geçmişini sil
- **Dışa Aktar**: Verileri CSV veya JSON olarak Desktop'e kaydet

---

## 💾 Veritabanı Schema

### HashHistory Tablosu
```sql
CREATE TABLE IF NOT EXISTS HashHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FilePath TEXT NOT NULL,
    FileHash TEXT NOT NULL,
    Algorithm TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

**Konum**: `%APPDATA%/DosyaBütünlüğüDoğrulayıcı/App.db`

---

## 🎨 Tema Sistemi

### Koyu Tema (Default)
- Arka Plan: `#1e1e1e`
- Metin: `#ffffff`
- Accent: `#007ACC` (Mavi)

### Açık Tema
- Arka Plan: `#ffffff`
- Metin: `#1e1e1e`
- Accent: `#007ACC` (Mavi)

Tema değişikliği anında uygulanır ve kaydedilir.

---

## 📤 Export Formatları

### CSV Örnek
```
Dosya,Hash,Algoritma,Tarih
"C:\\Documents\\file.pdf","a3d8c5e...","SHA256","2024-03-19 14:30:00"
```

### JSON Örnek
```json
[
  {
    "file": "C:\\Documents\\file.pdf",
    "hash": "a3d8c5e...",
    "algorithm": "SHA256",
    "date": "2024-03-19 14:30:00"
  }
]
```

---

## 🔒 Güvenlik Notları

- Tüm hash işlemleri güvenli System.Security.Cryptography kütüphanesi kullanılarak yapılır
- SHA1 ve MD5 eski algoritmalar olduğundan, yaygın hashler için SHA256/SHA512 önerilir
- Veritabanı local olarak saklanır, hiçbir internet bağlantısı yoktur

---

## 🐛 Bilinen Sorunlar

- Şu an yok

---

## 📝 Lisans

Kendi kullanım için AI yardımı ile yaratılmıştır

---

## 👤 Geliştirici

Muhammed Umut Sadebal

Sürüm: **1.0.2**  
Son Güncelleme: 24.03.2026
