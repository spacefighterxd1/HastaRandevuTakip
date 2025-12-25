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
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        logger.LogInformation("=== VERİTABANI KURULUMU BAŞLIYOR ===");
        
        // Önce bağlantıyı test et
        logger.LogInformation("Veritabanı bağlantısı test ediliyor...");
        if (!context.Database.CanConnect())
        {
            logger.LogError("VERİTABANI BAĞLANTISI BAŞARISIZ!");
        }
        else
        {
            logger.LogInformation("✓ Veritabanı bağlantısı başarılı.");
            
            // Tabloları oluştur
            logger.LogInformation("Tablolar oluşturuluyor...");
            var created = context.Database.EnsureCreated();
            if (created)
            {
                logger.LogInformation("✓ Veritabanı ve tablolar oluşturuldu.");
            }
            else
            {
                logger.LogInformation("ℹ Veritabanı zaten mevcut.");
            }
            
            // Tabloların varlığını doğrula
            logger.LogInformation("Tabloların varlığı kontrol ediliyor...");
            try
            {
                var doktorCount = context.Doktorlar.Count();
                logger.LogInformation("✓ Tablolar mevcut. Doktor sayısı: {0}", doktorCount);
            }
            catch (Exception ex)
            {
                logger.LogError("✗ Tablolar bulunamadı! Hata: {Message}", ex.Message);
                logger.LogInformation("Tabloları silip yeniden oluşturuyorum...");
                
                // Tabloları sil ve yeniden oluştur
                try
                {
                    context.Database.EnsureDeleted();
                    var recreated = context.Database.EnsureCreated();
                    if (recreated)
                    {
                        logger.LogInformation("✓ Tablolar başarıyla yeniden oluşturuldu.");
                    }
                    else
                    {
                        logger.LogError("✗ Tablolar yeniden oluşturulamadı!");
                    }
                }
                catch (Exception ex2)
                {
                    logger.LogError("Tablolar yeniden oluşturulamadı: {Message}", ex2.Message);
                }
            }
            
            logger.LogInformation("=== VERİTABANI KURULUMU TAMAMLANDI ===");
        }
    }
}
catch (Exception ex)
{
    try
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "✗✗✗ VERİTABANI KURULUMU BAŞARISIZ! ✗✗✗");
        logger.LogError("Hata: {Message}", ex.Message);
        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
    }
    catch { }
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


