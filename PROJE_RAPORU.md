# Hasta Randevu Takip Sistemi - Proje Raporu

## 1. Projenin Amacı

Bu proje, sağlık kuruluşlarında hasta bilgileri ve randevu yönetimi için geliştirilmiş modern bir web uygulamasıdır. Sistem, hastaların kayıtlarını tutmak, randevu oluşturmak, güncellemek ve takip etmek için kullanılmaktadır.

### Temel Amaçlar:
- Hasta bilgilerinin güvenli bir şekilde saklanması
- Randevu oluşturma ve yönetimi
- Kullanıcı dostu arayüz
- Responsive tasarım ile mobil uyumluluk
- CRUD işlemlerinin eksiksiz yapılması

---

## 2. Kullanılan Teknolojiler

### Backend:
- **ASP.NET Core MVC 8.0**: Web framework
- **Entity Framework Core 8.0**: ORM (Object-Relational Mapping)
- **SQL Server**: Veritabanı (LocalDB veya SQL Server Express)

### Frontend:
- **HTML5**: Yapısal markup
- **CSS3**: Stil ve tasarım
- **Bootstrap 5.3.2**: Responsive CSS framework
- **JavaScript**: İstemci tarafı işlemler
- **jQuery 3.7.1**: DOM manipülasyonu
- **jQuery Validation**: Form validasyonu

### Development Tools:
- **Visual Studio 2022** veya **Visual Studio Code**
- **.NET 8.0 SDK**
- **Git**: Versiyon kontrolü

---

## 3. Veritabanı Şeması

### ER Diyagramı

```
┌─────────────┐         ┌──────────────┐
│   Hasta     │         │   Randevu    │
├─────────────┤         ├──────────────┤
│ Id (PK)     │◄────────│ HastaId (FK) │
│ Ad          │   1     │ Id (PK)      │
│ Soyad       │    │    │ RandevuTarihi│
│ TCKimlikNo  │    │    │ DoktorAdi    │
│ Telefon     │    │    │ Poliklinik   │
│ Email       │    │    │ Notlar       │
│ DogumTarihi │    │    │ Durum        │
│ Adres       │    │    │ CreatedDate  │
│ CreatedDate │    N    └──────────────┘
└─────────────┘
```

### Tablo Yapıları

#### Hasta Tablosu
| Alan | Tür | Açıklama | Zorunlu |
|------|-----|----------|---------|
| Id | int | Primary Key, Identity | Evet |
| Ad | nvarchar(100) | Hasta adı | Evet |
| Soyad | nvarchar(100) | Hasta soyadı | Evet |
| TCKimlikNo | nvarchar(11) | TC Kimlik No (11 haneli) | Evet |
| Telefon | nvarchar(20) | Telefon numarası | Evet |
| Email | nvarchar(200) | E-posta adresi | Hayır |
| DogumTarihi | datetime | Doğum tarihi | Hayır |
| Adres | nvarchar(500) | Adres bilgisi | Hayır |
| CreatedDate | datetime | Kayıt tarihi | Evet |

#### Randevu Tablosu
| Alan | Tür | Açıklama | Zorunlu |
|------|-----|----------|---------|
| Id | int | Primary Key, Identity | Evet |
| HastaId | int | Foreign Key → Hasta | Evet |
| RandevuTarihi | datetime | Randevu tarihi ve saati | Evet |
| DoktorAdi | nvarchar(200) | Doktor adı | Hayır |
| Poliklinik | nvarchar(200) | Poliklinik adı | Hayır |
| Notlar | nvarchar(1000) | Randevu notları | Hayır |
| Durum | int | Randevu durumu (Enum) | Evet |
| CreatedDate | datetime | Oluşturulma tarihi | Evet |

### İlişkiler:
- **Hasta (1) → Randevu (N)**: Bir hastanın birden fazla randevusu olabilir
- **Cascade Delete**: Yok (Restrict) - Hasta silinirse randevular silinmez

---

## 4. Ekran Görüntüleri

### 4.1 Ana Sayfa
- İstatistik kartları (Toplam Hasta, Toplam Randevu, Bugünkü Randevular, Bekleyen Randevular)
- Hızlı işlem butonları
- Sistem özellikleri listesi

### 4.2 Hasta Listesi
- Tablo formatında hasta listesi
- Arama özelliği
- Sıralama (Ad, Soyad)
- CRUD işlem butonları (Detay, Düzenle, Sil)

### 4.3 Randevu Listesi
- Tablo formatında randevu listesi
- Hasta bilgileri ile birlikte görüntüleme
- Arama özelliği
- Sıralama (Tarih, Hasta)
- Durum badge'leri (Bekliyor, Onaylandı, İptal Edildi, Tamamlandı)

### 4.4 Form Sayfaları
- Create: Yeni kayıt ekleme formu
- Edit: Mevcut kaydı düzenleme formu
- Details: Detaylı bilgi görüntüleme
- Delete: Silme onay sayfası

---

## 5. Backend Kod Yapısının Açıklaması

### 5.1 Model-View-Controller (MVC) Mimarisi

#### Models (Modeller)
- **Hasta.cs**: Hasta entity modeli, data annotations ile validasyon
- **Randevu.cs**: Randevu entity modeli, enum ile durum yönetimi
- **ApplicationDbContext.cs**: Entity Framework DbContext, veritabanı bağlantısı ve konfigürasyonu

#### Controllers (Kontrolcüler)
- **HomeController.cs**: Ana sayfa ve istatistikler
- **HastaController.cs**: Hasta CRUD işlemleri
  - `Index()`: Liste ve arama
  - `Create()`: Yeni hasta ekleme (GET/POST)
  - `Edit()`: Hasta düzenleme (GET/POST)
  - `Details()`: Hasta detayları
  - `Delete()`: Hasta silme (GET/POST)
- **RandevuController.cs**: Randevu CRUD işlemleri
  - `Index()`: Liste, arama ve sıralama
  - `Create()`: Yeni randevu oluşturma (GET/POST)
  - `Edit()`: Randevu düzenleme (GET/POST)
  - `Details()`: Randevu detayları
  - `Delete()`: Randevu silme (GET/POST)

#### Views (Görünümler)
- Razor syntax ile dinamik HTML oluşturma
- Layout sayfası ile tutarlı tasarım
- Partial views ile kod tekrarını önleme
- Tag helpers ile form ve link oluşturma

### 5.2 Entity Framework Core Kullanımı

#### DbContext Yapılandırması:
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Hasta> Hastalar { get; set; }
    public DbSet<Randevu> Randevular { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // İlişki tanımlamaları
        // Seed data
    }
}
```

#### Dependency Injection:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

### 5.3 CRUD İşlemleri

#### Create (Oluşturma):
```csharp
[HttpPost]
public async Task<IActionResult> Create([Bind(...)] Hasta hasta)
{
    if (ModelState.IsValid)
    {
        hasta.CreatedDate = DateTime.Now;
        _context.Add(hasta);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    return View(hasta);
}
```

#### Read (Okuma):
```csharp
public async Task<IActionResult> Index()
{
    var hastalar = await _context.Hastalar.ToListAsync();
    return View(hastalar);
}
```

#### Update (Güncelleme):
```csharp
[HttpPost]
public async Task<IActionResult> Edit(int id, [Bind(...)] Hasta hasta)
{
    if (id != hasta.Id) return NotFound();
    
    if (ModelState.IsValid)
    {
        _context.Update(hasta);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    return View(hasta);
}
```

#### Delete (Silme):
```csharp
[HttpPost, ActionName("Delete")]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var hasta = await _context.Hastalar.FindAsync(id);
    if (hasta != null)
    {
        _context.Hastalar.Remove(hasta);
        await _context.SaveChangesAsync();
    }
    return RedirectToAction(nameof(Index));
}
```

### 5.4 Endpoint'ler

#### Hasta Endpoint'leri:
- `GET /Hasta` → Hasta listesi
- `GET /Hasta/Create` → Yeni hasta formu
- `POST /Hasta/Create` → Hasta oluşturma
- `GET /Hasta/Edit/{id}` → Düzenleme formu
- `POST /Hasta/Edit/{id}` → Hasta güncelleme
- `GET /Hasta/Details/{id}` → Hasta detayları
- `GET /Hasta/Delete/{id}` → Silme onay sayfası
- `POST /Hasta/Delete/{id}` → Hasta silme

#### Randevu Endpoint'leri:
- `GET /Randevu` → Randevu listesi
- `GET /Randevu/Create` → Yeni randevu formu
- `POST /Randevu/Create` → Randevu oluşturma
- `GET /Randevu/Edit/{id}` → Düzenleme formu
- `POST /Randevu/Edit/{id}` → Randevu güncelleme
- `GET /Randevu/Details/{id}` → Randevu detayları
- `GET /Randevu/Delete/{id}` → Silme onay sayfası
- `POST /Randevu/Delete/{id}` → Randevu silme

### 5.5 Controller-Model-View İlişkisi

1. **Controller** → **Model**: Controller, DbContext üzerinden Model'e erişir
2. **Controller** → **View**: Controller, View'a model verisi gönderir
3. **View** → **Controller**: View, form submit ile Controller'a POST isteği gönderir
4. **Model** → **View**: Model, View'da display edilir (Tag Helpers ile)

---

## 6. Sonuç ve Değerlendirme

### 6.1 Proje Başarıları

✅ **Tam CRUD İşlemleri**: Tüm Create, Read, Update, Delete işlemleri başarıyla uygulandı.

✅ **Modern Teknolojiler**: ASP.NET Core MVC 8.0 ve Entity Framework Core 8.0 kullanıldı.

✅ **Responsive Tasarım**: Bootstrap 5 ile mobil uyumlu arayüz oluşturuldu.

✅ **Form Validasyonları**: Hem client-side hem server-side validasyonlar eklendi.

✅ **Kullanıcı Dostu Arayüz**: Temiz, anlaşılır ve modern bir tasarım yapıldı.

✅ **Arama ve Filtreleme**: Hasta ve randevu listelerinde arama özelliği eklendi.

✅ **Sıralama**: Tablolarda sıralama özelliği eklendi.

### 6.2 Öğrenilen Konular

1. **ASP.NET Core MVC Mimarisi**: Model-View-Controller pattern'inin uygulanması
2. **Entity Framework Core**: ORM kullanımı, DbContext, LINQ sorguları
3. **Razor Syntax**: View'larda dinamik içerik oluşturma
4. **Dependency Injection**: Servislerin enjekte edilmesi
5. **Form Handling**: GET/POST işlemleri, Model Binding, Validation
6. **Navigation Properties**: İlişkili verilerin çekilmesi
7. **Responsive Design**: Bootstrap ile mobil uyumlu tasarım

### 6.3 Geliştirilebilecek Özellikler

- 🔐 Kullanıcı kimlik doğrulama ve yetkilendirme
- 📊 Gelişmiş raporlama ve istatistikler
- 📧 E-posta bildirimleri
- 📅 Takvim görünümü
- 🔍 Gelişmiş filtreleme seçenekleri
- 📱 Mobil uygulama
- 🌐 Çoklu dil desteği
- 📄 PDF export özelliği

### 6.4 Deployment Süreci

Proje, Render.com, Railway veya Azure gibi platformlara deploy edilebilir. Detaylı deployment talimatları `DEPLOYMENT.md` dosyasında bulunmaktadır.

**Deployment Adımları:**
1. GitHub repository oluşturma
2. Projeyi push etme
3. Cloud platform seçimi (Render.com önerilir)
4. Database oluşturma (PostgreSQL veya SQL Server)
5. Environment variables ayarlama
6. Build ve deploy

### 6.5 Sonuç

Bu proje, ASP.NET Core MVC framework'ünün temel ve ileri seviye özelliklerini kapsamlı bir şekilde uygulama fırsatı sağlamıştır. Tüm CRUD işlemleri, form validasyonları, arama ve filtreleme özellikleri başarıyla uygulanmıştır. Proje, production'a hazır durumda ve canlıya alınabilir seviyededir.

---

**Proje Tarihi:** 2024  
**Geliştirici:** [İsim Soyisim]  
**Versiyon:** 1.0.0


