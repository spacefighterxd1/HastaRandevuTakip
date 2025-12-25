using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaRandevuTakip.Models
{
    public class Hasta
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC Kimlik No zorunludur")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
        [Display(Name = "TC Kimlik No")]
        public string TCKimlikNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon alanı zorunludur")]
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir")]
        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string Telefon { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "E-posta en fazla 200 karakter olabilir")]
        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string? Email { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Property
        public virtual ICollection<Randevu>? Randevular { get; set; }

        [NotMapped]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad => $"{Ad} {Soyad}";
    }
}


