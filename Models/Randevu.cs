using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaRandevuTakip.Models
{
    public class Randevu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Hasta seçimi zorunludur")]
        [Display(Name = "Hasta")]
        public int HastaId { get; set; }

        [Required(ErrorMessage = "Doktor seçimi zorunludur")]
        [Display(Name = "Doktor")]
        public int DoktorId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur")]
        [Display(Name = "Randevu Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime RandevuTarihi { get; set; }

        [StringLength(200, ErrorMessage = "Doktor adı en fazla 200 karakter olabilir")]
        [Display(Name = "Doktor Adı")]
        public string? DoktorAdi { get; set; }

        [StringLength(200, ErrorMessage = "Poliklinik en fazla 200 karakter olabilir")]
        [Display(Name = "Poliklinik")]
        public string? Poliklinik { get; set; }

        [Required(ErrorMessage = "Rahatsızlık açıklaması zorunludur")]
        [StringLength(1000, ErrorMessage = "Rahatsızlık açıklaması en fazla 1000 karakter olabilir")]
        [Display(Name = "Rahatsızlık Açıklaması")]
        [DataType(DataType.MultilineText)]
        public string Rahatsizlik { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        [Display(Name = "Notlar")]
        [DataType(DataType.MultilineText)]
        public string? Notlar { get; set; }

        [Display(Name = "Durum")]
        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Bekliyor;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("HastaId")]
        public virtual Hasta? Hasta { get; set; }

        [ForeignKey("DoktorId")]
        public virtual Doktor? Doktor { get; set; }
    }

    public enum RandevuDurumu
    {
        [Display(Name = "Bekliyor")]
        Bekliyor = 0,
        [Display(Name = "Onaylandı")]
        Onaylandi = 1,
        [Display(Name = "İptal Edildi")]
        IptalEdildi = 2,
        [Display(Name = "Tamamlandı")]
        Tamamlandi = 3
    }
}


