using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalon_1.Models
{
    public class Hizmet
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [Display(Name = "Hizmet Adı")]
        public string Ad { get; set; }

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Süre bilgisi zorunludur.")]
        [Display(Name = "Süre (Dakika)")]
        public int Sure { get; set; } // المدة بالدقائق

        [Required(ErrorMessage = "Ücret bilgisi zorunludur.")]
        [Display(Name = "Ücret (TL)")]
        [Column(TypeName = "decimal(18,2)")] // <--- هذا هو السطر الجديد للإصلاح
        public decimal Ucret { get; set; }

        // --- ربط الخدمة بصالة معينة ---
        [Display(Name = "Spor Salonu")]
        public int SporSalonuId { get; set; }

        [ForeignKey("SporSalonuId")]
        public SporSalonu? SporSalonu { get; set; }

        public ICollection<Randevu>? Randevular { get; set; }
    }
}