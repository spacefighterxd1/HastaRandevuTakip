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
        
        // Production'da EnsureCreated kullan (migration yerine)
        context.Database.EnsureCreated();
        logger.LogInformation("Veritabanı hazır.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Veritabanı oluşturulurken bir hata oluştu: {Message}", ex.Message);
        // Production'da hata olsa bile uygulama çalışmaya devam etsin
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


