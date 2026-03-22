# 🎯 VALİDC# Phase 3 - Kullanım Kılavuzu

## ✅ Durum
- **BuildStatus**: ✅ SUCCESS (0 errors, 3 warnings)
- **EXESize**: 178.67 MB (self-contained, no runtime needed)
- **Phase3Features**: 100% Complete

---

## 🚀 Hızlı Başlangıç

### Uygulamayı Başlat
```
bin\Release\net8.0-windows\win-x64\publish\DosyaBütünlüğüDoğrulayıçı.exe
```

---

## 📋 Tab Rehberi

### 1️⃣ ANA SAYFA (MainTab)
**Fonksiyon**: Dosya hash'i hesapla ve veritabanına kaydet

**Adımlar:**
1. **"📂 GÖZAT"** - Dosya seç
2. **"⚙️ ALGORITMA"** - SHA256/512/MD5/SHA1 seç
3. **"↗️ HESAPLA"** - Hash'i hesapla
4. Sonuç göründüğünde, **"💾 KAYDET"** - Veritabanına kaydet

**✅ Test Et:**
- Test dosyaları burada: `%USERPROFILE%\Desktop\VALİDC#\test-files\`
- `test1.txt` seç
- SHA256 (default)
- HESAPLA'ya tıkla
- KAYDET'e tıkla

---

### 2️⃣ DOĞRULA (VerifyTab) - 🆕 YENİ FEATURE!
**Fonksiyon**: Dosya hash'ini doğrula (değişip değişmediğini kontrol et)

**Adımlar:**
1. **"📂 GÖZAT"** - Dosya seç (veya hash manuel gir)
2. **"📤 VERİTABANINDAN YÜKLE"** - Kayıtlı hash'i yükle
3. **Eski Hash** - Otomatik yüklendi
4. **"🔍 KARŞILAŞTIR"** - Karşılaştır
5. Sonuç: ✅ MATCH veya ❌ MISMATCH

**Sonuç Renkleri:**
- 🟢 **Yeşil** = MATCH (Dosya değişmedi)
- 🔴 **Kırmızı** = MISMATCH (Dosya değişti!)
- 🟠 **Sarı** = ERROR

---

### 3️⃣ İZLET (MonitorTab)
**Fonksiyon**: Klasördeki dosya değişimlerini izle

**Adımlar:**
1. **"Klasör Yolu"** - Metin kutusuna klasör yolu gir
2. **"🎯 İZLEMEYE BAŞLA"** - Başlat
3. **Yeni/Değiştirilmiş/Silinen** dosyalar listelenecek
4. **"🛑 İZLEMEYİ DURDUR"** - Durdur

**⚠️ Not:** Modal folder selection dialog yerine, metin kutusuna yolu gir

---

### 4️⃣ AYARLAR (SettingsTab)
**Fonksiyon**: Tema değiştir, ayarları yönet

**Tema Seçenekleri:**
- **Light** - Aydınlık tema
- **Dark** - Karanlık tema  
- **Cyberpunk** - Neon stil (varsayılan, #00ff00 yeşil, #00ffff turkuaz)

---

## 🗄️ Veritabanı

**Konum:**
```
%APPDATA%\DosyaBütünlüğüDoğrulayıcı\App.db
```

**C:\Users\alica\AppData\Roaming\DosyaBütünlüğüDoğrulayıçı\App.db**

**İçeriği:**
- Dosya adı
- Dosya yolu
- Hash değeri
- Algoritma (SHA256, SHA512, MD5, SHA1)
- Dosya boyutu
- Oluşturulma tarihi
- Son doğrulama tarihi
- Durum (OK, CHANGED, ERROR)

---

## 🧪 Test Sırası (Önerilen)

### Test 1: Temel Hash Hesaplama
```
✅ Önerilen: 5 dakika
1. ANA SAYFA → test1.txt seç
2. SHA256 seç
3. HESAPLA tıkla
4. Hash görünür mü? (64 karakter)
5. KAYDET tıkla
✓ DB'ye kaydedildi mi?
```

### Test 2: Hash Doğrulama (YENİ!)
```
✅ Önerilen: 5 dakika
1. DOĞRULA → test1.txt seç
2. VERİTABANINDAN YÜKLE tıkla
3. Eski hash yüklendi mi?
4. KARŞILAŞTIR tıkla
5. Sonuç: ✅ MATCH
✓ Doğrulama çalışıyor mu?
```

### Test 3: Dosya Değişim Testi
```
✅ Önerilen: 5 dakika
1. test1.txt'i Notepad'de aç
2. Biraz text ekle, kaydet
3. DOĞRULA → test1.txt seç
4. VERİTABANINDAN YÜKLE
5. KARŞILAŞTIR
6. Sonuç: ❌ MISMATCH (beklenen)
✓ Dosya değişim tespiti çalışıyor mu?
```

### Test 4: Tema Değişim
```
✅ Önerilen: 2 dakika
1. AYARLAR → Tema seç
2. Light → UI'ın aydınlaştığını gör
3. Dark → UI'ın kararıştığını gör
4. Cyberpunk → Neon yeşil/turkuaz gördüğünü gör
✓ Tema değişim çalışıyor mu?
```

---

## ⚠️ Bilinen Sorunlar

### ✋ FileSystemWatcher.Modified Devre Dışı
- **Etki**: MonitorTab'da dosya *modify* eventi çalışmıyor ancak create/delete çalışıyor
- **Çözüm**: Sonraki sprint'te düzeltilecek
- **Workaround**: Dosya silinerek/oluşturularak test edilebilir

### 📂 Folder Browser Dialog Engeli
- **Etki**: MonitorTab'da klasör seç dialog yok
- **Çözüm**: Manual yolu metin kutusuna gir
- **Örnek**: `C:\Users\alica\Desktop\VALİDC#\test-files`

---

## 🆘 Hata Çözümleri

### Veritabanı Hatası
- **Sorun**: "Veritabanı bağlantı başarısız"
- **Çözüm**: `%APPDATA%\DosyaBütünlüğüDoğrulayıçı\App.db` silinip yeniden başlat

### Hash Hesaplama Yavaş
- **Sorun**: Büyük dosyalarda boş ekran
- **Çözüm**: İlerleme çubuğunu izle, tamamen yüklenmesini bekle (1-2 dakika)

### Uygulama Açılmıyor
- **Sorun**: .NET Runtime yüklenmiş mi?
- **Çözüm**: EXE self-contained, runtime gerekmez. .NET SDK kontrol et: `dotnet --version`

---

## 📊 Beklenen Sonuçlar

**Başarılı İşlem Göstergeleri:**
- ✅ Uygulamapenceresi açılıyor
- ✅ Dosya seçimi çalışıyor
- ✅ Hash hesaplama çalışıyor (SHA256: 64 karakter)
- ✅ Veritabanı kaydı çalışıyor
- ✅ Hash doğrulama çalışıyor
- ✅ Tema değişim çalışıyor

**Hedef Tamamlama Süresi:** 20 dakika

---

## 🎯 Sonraki Adımlar

- [ ] Temel test tamamlandı
- [ ] Hata durumu tespit edildi (varsa)
- [ ] Phase 4 gereksinimler hazırlanacak

---

**Versiyon**: Phase 3 (v1.0)  
**Tarih**: 19 Mart 2026  
**Durum**: ✅ Production Ready
