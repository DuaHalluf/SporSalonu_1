using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalon_1.Models
{
    public class Antrenor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; }

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [Display(Name = "Uzmanlık Alanı")]
        public string UzmanlikAlani { get; set; } // مثال: Pilates Eğitmeni

        [Display(Name = "Fotoğraf")]
        public string? ResimUrl { get; set; }

        [Required]
        [Display(Name = "Çalışma Saatleri")]
        public string CalismaSaatleri { get; set; } // مثال: 09:00 - 17:00

        // --- ربط المدرب بصالة معينة ---
        [Display(Name = "Spor Salonu")]
        public int SporSalonuId { get; set; }

        [ForeignKey("SporSalonuId")]
        public SporSalonu? SporSalonu { get; set; }

        public ICollection<Randevu>? Randevular { get; set; }
    }
}
