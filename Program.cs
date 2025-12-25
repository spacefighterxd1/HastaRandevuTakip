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
if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    try
    {
        // Npgsql'in kendi connection string builder'ını kullan
        var npgsqlBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        connectionString = npgsqlBuilder.ConnectionString;
    }
    catch
    {
        // Npgsql parse edemezse manuel parse dene
        try
        {
            var uri = new Uri(connectionString);
            var host = uri.Host;
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
            
            connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;";
        }
        catch
        {
            // Parse edilemezse olduğu gibi kullan
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

// Database Migration
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Veritabanı bağlantısı kontrol ediliyor...");
        
        // Önce bağlantıyı test et
        if (!context.Database.CanConnect())
        {
            logger.LogError("Veritabanına bağlanılamıyor!");
            throw new Exception("Veritabanı bağlantısı başarısız!");
        }
        logger.LogInformation("Veritabanı bağlantısı başarılı.");
        
        // Production'da EnsureCreated kullan (migration yerine)
        var created = context.Database.EnsureCreated();
        if (created)
        {
            logger.LogInformation("Veritabanı ve tablolar oluşturuldu.");
        }
        else
        {
            logger.LogInformation("Veritabanı zaten mevcut.");
        }
        
        // Tabloların varlığını doğrula
        try
        {
            var doktorCount = context.Doktorlar.Count();
            logger.LogInformation("Tablolar mevcut. Doktor sayısı: {0}", doktorCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Tablolar bulunamadı! Hata: {Message}", ex.Message);
            logger.LogInformation("Tabloları yeniden oluşturmayı deniyorum...");
            // Tablolar yoksa tekrar oluşturmayı dene
            try
            {
                context.Database.EnsureDeleted();
                var recreated = context.Database.EnsureCreated();
                if (recreated)
                {
                    logger.LogInformation("Tablolar başarıyla yeniden oluşturuldu.");
                }
                else
                {
                    logger.LogWarning("Tablolar yeniden oluşturulamadı (EnsureCreated false döndü).");
                }
            }
            catch (Exception ex2)
            {
                logger.LogError(ex2, "Tablolar yeniden oluşturulamadı: {Message}", ex2.Message);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Veritabanı oluşturulurken bir hata oluştu: {Message}", ex.Message);
        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
        // Production'da hata olsa bile uygulama çalışmaya devam etsin (logları kontrol edebilmek için)
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


