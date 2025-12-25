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

// Database Setup - Standard ASP.NET Core + PostgreSQL + EF Core approach
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        logger.LogInformation("=== VERİTABANI KURULUMU BAŞLIYOR ===");
        
        // Test connection
        logger.LogInformation("Veritabanı bağlantısı test ediliyor...");
        if (context.Database.CanConnect())
        {
            logger.LogInformation("✓ Veritabanı bağlantısı başarılı.");
            
            // Check if tables exist by trying to query
            bool tablesExist = false;
            try
            {
                _ = context.Doktorlar.Count();
                tablesExist = true;
                logger.LogInformation("✓ Tablolar mevcut.");
            }
            catch
            {
                tablesExist = false;
                logger.LogInformation("Tablolar bulunamadı, oluşturulacak...");
            }
            
            // Create tables if they don't exist
            if (!tablesExist)
            {
                logger.LogInformation("Tablolar oluşturuluyor...");
                var created = context.Database.EnsureCreated();
                if (created)
                {
                    logger.LogInformation("✓ Tablolar başarıyla oluşturuldu.");
                }
                else
                {
                    logger.LogWarning("EnsureCreated() false döndü.");
                }
                
                // Verify tables were created
                try
                {
                    var count = context.Doktorlar.Count();
                    logger.LogInformation("✓ Tablolar doğrulandı. Doktor sayısı: {0}", count);
                }
                catch (Exception ex)
                {
                    logger.LogError("✗ Tablolar oluşturulamadı! Hata: {Message}", ex.Message);
                    // Try one more time with EnsureDeleted + EnsureCreated
                    try
                    {
                        logger.LogInformation("Tabloları silip yeniden oluşturmayı deniyorum...");
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                        var verifyCount = context.Doktorlar.Count();
                        logger.LogInformation("✓ Tablolar başarıyla yeniden oluşturuldu. Doktor sayısı: {0}", verifyCount);
                    }
                    catch (Exception ex2)
                    {
                        logger.LogError("✗ Tablolar yeniden oluşturulamadı! Hata: {Message}", ex2.Message);
                    }
                }
            }
            
            logger.LogInformation("=== VERİTABANI KURULUMU TAMAMLANDI ===");
        }
        else
        {
            logger.LogError("✗ Veritabanı bağlantısı başarısız!");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "✗✗✗ VERİTABANI KURULUMU BAŞARISIZ! ✗✗✗");
        logger.LogError("Hata: {Message}", ex.Message);
        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
        if (ex.InnerException != null)
        {
            logger.LogError("Inner exception: {InnerException}", ex.InnerException.Message);
        }
    }
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


