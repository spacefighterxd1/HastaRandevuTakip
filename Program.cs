using Microsoft.EntityFrameworkCore;
using HastaRandevuTakip.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// PostgreSQL URI formatını standard connection string formatına çevir
// Npgsql URI formatını direkt kabul eder, ama güvenlik için standard formata çevirelim
if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    try
    {
        // Önce Npgsql'in kendi connection string builder'ını dene
        var npgsqlBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        connectionString = npgsqlBuilder.ConnectionString;
    }
    catch (Exception ex)
    {
        // Npgsql parse edemezse manuel parse dene
        try
        {
            var uri = new Uri(connectionString);
            var host = uri.Host;
            // Port yoksa varsayılan 5432 kullan
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/').Split('?')[0];
            var userInfo = uri.UserInfo;
            
            string user = "";
            string password = "";
            
            if (!string.IsNullOrEmpty(userInfo))
            {
                var userInfoParts = userInfo.Split(':');
                user = Uri.UnescapeDataString(userInfoParts[0]);
                if (userInfoParts.Length > 1)
                {
                    password = Uri.UnescapeDataString(string.Join(":", userInfoParts.Skip(1)));
                }
            }
            
            // Standard PostgreSQL connection string formatı
            connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;";
        }
        catch
        {
            // Parse edilemezse URI formatını olduğu gibi kullan (Npgsql kabul edebilir)
        }
    }
}

// PostgreSQL veya SQL Server desteği (connection string'e göre otomatik algılar)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // PostgreSQL connection string kontrolü
    if (connectionString.Contains("Host=") || 
        connectionString.Contains("postgresql://") || 
        connectionString.Contains("postgres://") ||
        connectionString.Contains("User Id=") ||
        connectionString.Contains("Username="))
    {
        // PostgreSQL connection string
        options.UseNpgsql(connectionString);
    }
    else
    {
        // SQL Server connection string
        options.UseSqlServer(connectionString);
    }
});

var app = builder.Build();

// Database Migration - Uygulama başlamadan önce tabloları oluştur
Console.WriteLine("=== VERİTABANI KURULUMU BAŞLIYOR ===");
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        ILogger<Program> logger;
        try
        {
            logger = services.GetRequiredService<ILogger<Program>>();
        }
        catch
        {
            // Logger alınamazsa Console kullan
            Console.WriteLine("Logger alınamadı, Console kullanılıyor.");
            logger = null;
        }
        
        ApplicationDbContext context;
        try
        {
            context = services.GetRequiredService<ApplicationDbContext>();
        }
        catch (Exception ex)
        {
            var errorMsg = $"ApplicationDbContext alınamadı: {ex.Message}";
            Console.WriteLine(errorMsg);
            logger?.LogError(errorMsg);
            throw;
        }
        
        logger?.LogInformation("=== VERİTABANI KURULUMU BAŞLIYOR ===");
        Console.WriteLine("Veritabanı bağlantısı test ediliyor...");
        
        // Önce bağlantıyı test et
        bool canConnect = false;
        try
        {
            canConnect = context.Database.CanConnect();
        }
        catch (Exception ex)
        {
            var errorMsg = $"CanConnect() hatası: {ex.Message}";
            Console.WriteLine(errorMsg);
            logger?.LogError(errorMsg);
            throw;
        }
        
        if (!canConnect)
        {
            var errorMsg = "VERİTABANI BAĞLANTISI BAŞARISIZ!";
            Console.WriteLine(errorMsg);
            logger?.LogError(errorMsg);
            throw new Exception("Veritabanı bağlantısı başarısız!");
        }
        
        Console.WriteLine("✓ Veritabanı bağlantısı başarılı.");
        logger?.LogInformation("✓ Veritabanı bağlantısı başarılı.");
        
        // Tabloları oluştur
        Console.WriteLine("Tablolar oluşturuluyor...");
        logger?.LogInformation("Tablolar oluşturuluyor...");
        
        bool created = false;
        try
        {
            created = context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            var errorMsg = $"EnsureCreated() hatası: {ex.Message}";
            Console.WriteLine(errorMsg);
            logger?.LogError(errorMsg);
            throw;
        }
        
        if (created)
        {
            Console.WriteLine("✓ Veritabanı ve tablolar oluşturuldu.");
            logger?.LogInformation("✓ Veritabanı ve tablolar oluşturuldu.");
        }
        else
        {
            Console.WriteLine("ℹ Veritabanı zaten mevcut.");
            logger?.LogInformation("ℹ Veritabanı zaten mevcut.");
        }
        
        // Tabloların varlığını doğrula
        Console.WriteLine("Tabloların varlığı kontrol ediliyor...");
        logger?.LogInformation("Tabloların varlığı kontrol ediliyor...");
        
        try
        {
            var doktorCount = context.Doktorlar.Count();
            Console.WriteLine($"✓ Tablolar mevcut. Doktor sayısı: {doktorCount}");
            logger?.LogInformation("✓ Tablolar mevcut. Doktor sayısı: {0}", doktorCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Tablolar bulunamadı! Hata: {ex.Message}");
            logger?.LogError("✗ Tablolar bulunamadı! Hata: {Message}", ex.Message);
            Console.WriteLine("Tabloları silip yeniden oluşturuyorum...");
            logger?.LogInformation("Tabloları silip yeniden oluşturuyorum...");
            
            // Tabloları sil ve yeniden oluştur
            try
            {
                context.Database.EnsureDeleted();
                var recreated = context.Database.EnsureCreated();
                if (recreated)
                {
                    Console.WriteLine("✓ Tablolar başarıyla yeniden oluşturuldu.");
                    logger?.LogInformation("✓ Tablolar başarıyla yeniden oluşturuldu.");
                }
                else
                {
                    var errorMsg = "✗ Tablolar yeniden oluşturulamadı!";
                    Console.WriteLine(errorMsg);
                    logger?.LogError(errorMsg);
                    throw new Exception("Tablolar oluşturulamadı!");
                }
            }
            catch (Exception ex2)
            {
                var errorMsg = $"Tablolar yeniden oluşturulamadı: {ex2.Message}";
                Console.WriteLine(errorMsg);
                logger?.LogError(errorMsg);
                throw;
            }
        }
        
        Console.WriteLine("=== VERİTABANI KURULUMU TAMAMLANDI ===");
        logger?.LogInformation("=== VERİTABANI KURULUMU TAMAMLANDI ===");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗✗✗ VERİTABANI KURULUMU BAŞARISIZ! ✗✗✗");
    Console.WriteLine($"Hata: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    
    try
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "✗✗✗ VERİTABANI KURULUMU BAŞARISIZ! ✗✗✗");
        logger.LogError("Hata: {Message}", ex.Message);
        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
    }
    catch
    {
        // Logger alınamazsa sadece Console'a yaz
    }
    // Uygulama başlamaya devam etsin (logları görebilmek için)
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=RandevuAlma}/{action=Index}/{id?}");

app.Run();


