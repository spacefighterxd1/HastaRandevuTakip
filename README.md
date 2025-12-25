# Hasta Randevu Takip Sistemi

ASP.NET Core MVC ile geliştirilmiş hasta ve randevu yönetim sistemi.

## 📋 Proje Hakkında

Bu proje, sağlık kuruluşlarında hasta bilgileri ve randevu yönetimi için geliştirilmiş modern bir web uygulamasıdır. Tüm CRUD (Create, Read, Update, Delete) işlemleri Entity Framework Core ile gerçekleştirilmektedir.

## 🚀 Özellikler

- ✅ Hasta kayıt yönetimi (CRUD işlemleri)
- ✅ Randevu oluşturma ve güncelleme
- ✅ Arama ve filtreleme
- ✅ Sıralama (sorting)
- ✅ Responsive tasarım (mobil uyumlu)
- ✅ Form validasyonları
- ✅ Güvenli veri yönetimi
- ✅ İstatistikler (Ana sayfa)

## 🛠️ Kullanılan Teknolojiler

- **Backend:** ASP.NET Core MVC 8.0
- **Veritabanı:** SQL Server / Entity Framework Core
- **Frontend:** HTML5, CSS3, Bootstrap 5, JavaScript
- **ORM:** Entity Framework Core 8.0

## 📦 Kurulum

### Gereksinimler

- .NET 8.0 SDK
- SQL Server (LocalDB veya SQL Server Express)
- Visual Studio 2022 veya Visual Studio Code

### Adımlar

1. Projeyi klonlayın:
```bash
git clone [repository-url]
cd HastaRandevuTakip
```

2. NuGet paketlerini yükleyin:
```bash
dotnet restore
```

3. Veritabanı bağlantı string'ini `appsettings.json` dosyasında düzenleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HastaRandevuTakipDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

4. Veritabanını oluşturun:
```bash
dotnet ef database update
```

Veya uygulamayı çalıştırdığınızda otomatik olarak oluşturulacaktır (Development modunda).

5. Uygulamayı çalıştırın:
```bash
dotnet run
```

6. Tarayıcıda açın: `https://localhost:5001` veya `http://localhost:5000`

## 📁 Proje Yapısı

```
HastaRandevuTakip/
│
├── Controllers/
│   ├── HomeController.cs          # Ana sayfa ve istatistikler
│   ├── HastaController.cs        # Hasta CRUD işlemleri
│   └── RandevuController.cs      # Randevu CRUD işlemleri
│
├── Models/
│   ├── Hasta.cs                  # Hasta entity modeli
│   ├── Randevu.cs                # Randevu entity modeli
│   └── ApplicationDbContext.cs   # DbContext
│
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml          # Ana sayfa
│   │   └── About.cshtml          # Hakkında sayfası
│   ├── Hasta/
│   │   ├── Index.cshtml          # Hasta listesi
│   │   ├── Create.cshtml         # Yeni hasta ekleme
│   │   ├── Edit.cshtml           # Hasta düzenleme
│   │   ├── Details.cshtml        # Hasta detayları
│   │   └── Delete.cshtml         # Hasta silme
│   ├── Randevu/
│   │   ├── Index.cshtml          # Randevu listesi
│   │   ├── Create.cshtml         # Yeni randevu oluşturma
│   │   ├── Edit.cshtml           # Randevu düzenleme
│   │   ├── Details.cshtml        # Randevu detayları
│   │   └── Delete.cshtml         # Randevu silme
│   └── Shared/
│       └── _Layout.cshtml        # Ana layout
│
├── wwwroot/
│   ├── css/
│   │   └── site.css             # Özel CSS stilleri
│   └── js/
│       └── site.js               # JavaScript fonksiyonları
│
├── Program.cs                    # Uygulama başlangıç noktası
├── appsettings.json              # Uygulama ayarları
└── HastaRandevuTakip.csproj      # Proje dosyası
```

## 🗄️ Veritabanı Şeması

### Hasta Tablosu
- `Id` (int, PK)
- `Ad` (nvarchar(100))
- `Soyad` (nvarchar(100))
- `TCKimlikNo` (nvarchar(11))
- `Telefon` (nvarchar(20))
- `Email` (nvarchar(200), nullable)
- `DogumTarihi` (datetime, nullable)
- `Adres` (nvarchar(500), nullable)
- `CreatedDate` (datetime)

### Randevu Tablosu
- `Id` (int, PK)
- `HastaId` (int, FK → Hasta)
- `RandevuTarihi` (datetime)
- `DoktorAdi` (nvarchar(200), nullable)
- `Poliklinik` (nvarchar(200), nullable)
- `Notlar` (nvarchar(1000), nullable)
- `Durum` (int) - Enum: Bekliyor, Onaylandı, İptal Edildi, Tamamlandı
- `CreatedDate` (datetime)

## 🌐 Deployment

### Render.com (Önerilen)

1. GitHub repository'nize projeyi push edin
2. Render.com'da yeni bir Web Service oluşturun
3. Repository'yi bağlayın
4. Build Command: `dotnet publish -c Release -o ./publish`
5. Start Command: `dotnet HastaRandevuTakip.dll`
6. Environment Variables:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__DefaultConnection=[Production SQL Server Connection String]`
7. PostgreSQL veya SQL Server database ekleyin ve connection string'i ayarlayın

### Railway

1. Railway hesabı oluşturun
2. New Project → Deploy from GitHub
3. Repository'yi seçin
4. PostgreSQL database ekleyin
5. Environment Variables ayarlayın
6. Deploy butonuna tıklayın

### Azure

1. Azure Portal'da App Service oluşturun
2. Deployment Center'dan GitHub'ı bağlayın
3. SQL Database oluşturun
4. Connection String'i App Settings'e ekleyin
5. Deploy edin

## 📝 Notlar

- Development modunda veritabanı otomatik oluşturulur
- Production'da migration kullanın: `dotnet ef database update`
- Seed data (örnek veriler) ApplicationDbContext'te tanımlıdır

## 👨‍💻 Geliştirici

Bu proje ASP.NET Core MVC öğrenme amaçlı geliştirilmiştir.

## 📄 Lisans

Bu proje eğitim amaçlıdır.


