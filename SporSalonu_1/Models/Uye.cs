using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalon_1.Models
{
    // يرث من IdentityUser وبالتالي يحتوي تلقائياً على: Email, PhoneNumber, UserName, PasswordHash
    public class Uye : IdentityUser
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        [StringLength(100)]
        public string AdSoyad { get; set; } = string.Empty;

        [Display(Name = "Boy (cm)")]
        [Range(0, 300, ErrorMessage = "Geçerli bir boy giriniz.")]
        public double? Boy { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(0, 500, ErrorMessage = "Geçerli bir kilo giriniz.")]
        public double? Kilo { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        public string? ResimUrl { get; set; }

        // تهيئة القائمة لتجنب مشاكل Null عند الإضافة
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}