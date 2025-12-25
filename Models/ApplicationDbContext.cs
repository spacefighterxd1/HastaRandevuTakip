using Microsoft.EntityFrameworkCore;

namespace HastaRandevuTakip.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Hasta> Hastalar { get; set; }
        public DbSet<Doktor> Doktorlar { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Hasta - Randevu ilişkisi
            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Hasta)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HastaId)
                .OnDelete(DeleteBehavior.Restrict); // Hasta silinirse randevular silinmesin

            // Doktor - Randevu ilişkisi
            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Doktor)
                .WithMany(d => d.Randevular)
                .HasForeignKey(r => r.DoktorId)
                .OnDelete(DeleteBehavior.Restrict); // Doktor silinirse randevular silinmesin

            // Seed Data - Doktorlar
            modelBuilder.Entity<Doktor>().HasData(
                new Doktor
                {
                    Id = 1,
                    Ad = "Mehmet",
                    Soyad = "Kaya",
                    UzmanlikAlani = "Kardiyoloji",
                    Aciklama = "Kalp ve damar hastalıkları uzmanı. 15 yıllık deneyim.",
                    FotoUrl = "https://via.placeholder.com/150",
                    Aktif = true,
                    CreatedDate = new DateTime(2024, 1, 1)
                },
                new Doktor
                {
                    Id = 2,
                    Ad = "Ayşe",
                    Soyad = "Yıldız",
                    UzmanlikAlani = "Nöroloji",
                    Aciklama = "Beyin ve sinir sistemi hastalıkları uzmanı. 12 yıllık deneyim.",
                    FotoUrl = "https://via.placeholder.com/150",
                    Aktif = true,
                    CreatedDate = new DateTime(2024, 1, 1)
                },
                new Doktor
                {
                    Id = 3,
                    Ad = "Ali",
                    Soyad = "Demir",
                    UzmanlikAlani = "Ortopedi",
                    Aciklama = "Kemik, eklem ve kas hastalıkları uzmanı. 10 yıllık deneyim.",
                    FotoUrl = "https://via.placeholder.com/150",
                    Aktif = true,
                    CreatedDate = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}


