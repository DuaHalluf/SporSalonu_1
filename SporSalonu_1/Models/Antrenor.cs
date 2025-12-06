using System;
using System.Collections.Generic;
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
        // 🚨 تم التصحيح: تعيين قيمة أولية فارغة
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [Display(Name = "Uzmanlık Alanı")]
        // 🚨 تم التصحيح: تعيين قيمة أولية فارغة
        public string UzmanlikAlani { get; set; } = string.Empty; // مثال: Pilates Eğitmeni

        [Display(Name = "Fotoğraf")]
        public string? ResimUrl { get; set; } // هذا صحيح بالفعل ومسموح له بـ Null

        [Required]
        [Display(Name = "Çalışma Saatleri")]
        // 🚨 تم التصحيح: تعيين قيمة أولية فارغة
        public string CalismaSaatleri { get; set; } = string.Empty; // مثال: 09:00 - 17:00

        // --- ربط المدرب بصالة معينة ---
        [Display(Name = "Spor Salonu")]
        public int SporSalonuId { get; set; }

        [ForeignKey("SporSalonuId")]
        public SporSalonu? SporSalonu { get; set; }

        public ICollection<Randevu>? Randevular { get; set; }
    }
}
