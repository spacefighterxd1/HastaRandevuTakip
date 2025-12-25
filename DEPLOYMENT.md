# Deployment Rehberi

Bu doküman, Hasta Randevu Takip Sistemi'ni canlıya almak için gerekli adımları içermektedir.

## 🌐 Render.com ile Deployment (Önerilen)

### Adım 1: GitHub Repository Hazırlığı

1. Projeyi GitHub'a push edin:
```bash
git init
git add .
git commit -m "Initial commit"
git remote add origin [your-github-repo-url]
git push -u origin main
```

### Adım 2: Render.com'da Web Service Oluşturma

1. [Render.com](https://render.com) hesabı oluşturun
2. Dashboard'dan "New +" → "Web Service" seçin
3. GitHub repository'nizi bağlayın
4. Ayarları yapın:
   - **Name:** hasta-randevu-takip
   - **Environment:** .NET
   - **Build Command:** `dotnet publish -c Release -o ./publish`
   - **Start Command:** `dotnet HastaRandevuTakip.dll`
   - **Instance Type:** Free (veya daha yüksek)

### Adım 3: PostgreSQL Database Oluşturma

1. Render Dashboard'da "New +" → "PostgreSQL" seçin
2. Database ayarları:
   - **Name:** hasta-randevu-db
   - **Database:** HastaRandevuTakipDB
   - **User:** hasta_user
   - **Plan:** Free

### Adım 4: Environment Variables Ayarlama

Web Service'in Environment Variables bölümüne ekleyin:

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=[PostgreSQL Connection String]
```

PostgreSQL Connection String formatı:
```
Host=[host];Port=[port];Database=[database];Username=[user];Password=[password];SSL Mode=Require;
```

### Adım 5: Database Migration

Render.com'da Shell açın ve migration çalıştırın:

```bash
dotnet ef database update
```

Veya `Program.cs`'de `EnsureCreated()` kullanıyorsanız, ilk çalıştırmada otomatik oluşturulur.

### Adım 6: Deploy

1. "Manual Deploy" → "Deploy latest commit" tıklayın
2. Build ve deploy işlemi tamamlanana kadar bekleyin
3. Canlı URL'iniz hazır!

---

## 🚂 Railway ile Deployment

### Adım 1: Railway Hesabı

1. [Railway.app](https://railway.app) hesabı oluşturun
2. GitHub ile giriş yapın

### Adım 2: Yeni Proje

1. "New Project" → "Deploy from GitHub repo"
2. Repository'nizi seçin
3. Railway otomatik olarak .NET projesini algılar

### Adım 3: PostgreSQL Database

1. "New" → "Database" → "Add PostgreSQL"
2. Database otomatik oluşturulur

### Adım 4: Environment Variables

Settings → Variables bölümüne ekleyin:

```
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=[Railway otomatik sağlar]
```

### Adım 5: Connection String Düzenleme

`Program.cs`'de connection string'i Railway'in sağladığı `DATABASE_URL`'den alacak şekilde düzenleyin:

```csharp
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
```

---

## ☁️ Azure ile Deployment

### Adım 1: Azure App Service Oluşturma

1. Azure Portal → "Create a resource" → "Web App"
2. Ayarlar:
   - **Name:** hasta-randevu-takip
   - **Runtime stack:** .NET 8
   - **Operating System:** Linux (veya Windows)
   - **Region:** Seçiniz

### Adım 2: SQL Database Oluşturma

1. Azure Portal → "Create a resource" → "SQL Database"
2. Ayarları yapın ve App Service ile bağlayın

### Adım 3: Deployment

1. App Service → "Deployment Center"
2. "GitHub" seçin ve repository'yi bağlayın
3. Otomatik deployment aktif olur

### Adım 4: Connection String

1. App Service → "Configuration" → "Application settings"
2. Connection string ekleyin:
   - **Name:** DefaultConnection
   - **Value:** [SQL Database connection string]
   - **Type:** SQLAzure

---

## 🐳 Docker ile Deployment

### Docker Build

```bash
docker build -t hasta-randevu-takip .
```

### Docker Run

```bash
docker run -d -p 8080:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="[connection-string]" \
  hasta-randevu-takip
```

---

## ✅ Deployment Sonrası Kontroller

1. ✅ Ana sayfa açılıyor mu?
2. ✅ Hasta listesi görüntüleniyor mu?
3. ✅ Yeni hasta eklenebiliyor mu?
4. ✅ Randevu oluşturulabiliyor mu?
5. ✅ Veritabanı bağlantısı çalışıyor mu?

---

## 🔧 Sorun Giderme

### Database Connection Hatası

- Connection string'i kontrol edin
- Firewall ayarlarını kontrol edin
- SSL Mode ayarlarını kontrol edin (PostgreSQL için)

### Build Hatası

- .NET 8.0 SDK yüklü mü kontrol edin
- NuGet paketleri restore edildi mi kontrol edin

### Runtime Hatası

- Logları kontrol edin (Render/Railway/Azure logs)
- Environment variables doğru mu kontrol edin

---

## 📝 Notlar

- Production'da `EnsureCreated()` yerine migration kullanın
- Connection string'leri environment variables'da saklayın
- HTTPS kullanın
- Regular backup alın


