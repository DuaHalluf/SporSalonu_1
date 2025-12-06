using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalon_1.Models
{
    public class Randevu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen bir tarih seçiniz.")]
        [Display(Name = "Randevu Tarihi")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Lütfen bir saat seçiniz.")]
        [Display(Name = "Randevu Saati")]
        [DataType(DataType.Time)]
        public TimeSpan Saat { get; set; }

        [Display(Name = "Onay Durumu")]
        public bool OnaylandiMi { get; set; } = false;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        // --- العلاقات ---

        [Display(Name = "Üye")]
        public string? UyeId { get; set; }

        [ForeignKey("UyeId")]
        public virtual Uye? Uye { get; set; }

        [Required(ErrorMessage = "Lütfen bir antrenör seçiniz.")]
        [Display(Name = "Antrenör")]
        public int AntrenorId { get; set; }

        [ForeignKey("AntrenorId")]
        public virtual Antrenor? Antrenor { get; set; }

        [Required(ErrorMessage = "Lütfen bir hizmet türü seçiniz.")]
        [Display(Name = "Hizmet Türü")]
        public int HizmetId { get; set; }

        [ForeignKey("HizmetId")]
        public virtual Hizmet? Hizmet { get; set; }
    }
}