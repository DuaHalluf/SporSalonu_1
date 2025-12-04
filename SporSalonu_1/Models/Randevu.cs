using SporSalonu_1.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalon_1.Models
{
    public class Randevu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Randevu Tarihi")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Required]
        [Display(Name = "Randevu Saati")]
        [DataType(DataType.Time)]
        public TimeSpan Saat { get; set; }

        [Display(Name = "Onay Durumu")]
        public bool OnaylandiMi { get; set; } = false; // افتراضياً غير موافق عليه

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        // --- العلاقات (Foreign Keys) ---

        // 1. العضو
        public string UyeId { get; set; }
        [ForeignKey("UyeId")]
        public Uye Uye { get; set; }

        // 2. المدرب
        [Display(Name = "Antrenör")]
        public int AntrenorId { get; set; }
        [ForeignKey("AntrenorId")]
        public Antrenor Antrenor { get; set; }

        // 3. الخدمة
        [Display(Name = "Hizmet")]
        public int HizmetId { get; set; }
        [ForeignKey("HizmetId")]
        public Hizmet Hizmet { get; set; }
    }
}