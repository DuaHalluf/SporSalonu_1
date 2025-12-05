using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SporSalon_1.Models
{
    public class Uye : IdentityUser
    {
        
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string? AdSoyad { get; set; }

        [Display(Name = "Boy (cm)")]
        public double? Boy { get; set; } // للطول

        [Display(Name = "Kilo (kg)")]
        public double? Kilo { get; set; } // للوزن

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        public ICollection<Randevu>? Randevular { get; set; }
    }
}
