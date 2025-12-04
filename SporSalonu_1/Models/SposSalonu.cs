using SporSalonu_1.Models;
using System.ComponentModel.DataAnnotations;

namespace SporSalon_1.Models
{
    public class SporSalonu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Salon adı zorunludur.")]
        [Display(Name = "Salon Adı")]
        public string Ad { get; set; } // مثال: Merkez Şube

        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Required(ErrorMessage = "Çalışma saatleri zorunludur.")]
        [Display(Name = "Çalışma Saatleri")]
        public string CalismaSaatleri { get; set; } // مثال: 09:00 - 22:00

        [Display(Name = "Telefon")]
        [Phone]
        public string? Telefon { get; set; }

        // --- العلاقات ---
        public ICollection<Hizmet>? Hizmetler { get; set; }
        public ICollection<Antrenor>? Antrenorler { get; set; }
    }
}
