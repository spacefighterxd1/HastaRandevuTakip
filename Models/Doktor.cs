using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaRandevuTakip.Models
{
    public class Doktor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Doktor adı zorunludur")]
        [StringLength(100, ErrorMessage = "Doktor adı en fazla 100 karakter olabilir")]
        [Display(Name = "Doktor Adı")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doktor soyadı zorunludur")]
        [StringLength(100, ErrorMessage = "Doktor soyadı en fazla 100 karakter olabilir")]
        [Display(Name = "Doktor Soyadı")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur")]
        [StringLength(200, ErrorMessage = "Uzmanlık alanı en fazla 200 karakter olabilir")]
        [Display(Name = "Uzmanlık Alanı")]
        public string UzmanlikAlani { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [StringLength(200, ErrorMessage = "Fotoğraf URL en fazla 200 karakter olabilir")]
        [Display(Name = "Fotoğraf")]
        public string? FotoUrl { get; set; }

        [Display(Name = "Aktif")]
        public bool Aktif { get; set; } = true;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual ICollection<Randevu>? Randevular { get; set; }

        [NotMapped]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad => $"{Ad} {Soyad}";
    }
}

