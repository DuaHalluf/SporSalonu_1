namespace SporSalon_1.Models
{
    public class YapayZekaModel
    {
        // المدخلات
        public double Boy { get; set; } // الطول بالسنتيمتر
        public double Kilo { get; set; } // الوزن بالكيلو
        public string Cinsiyet { get; set; } // Erkek / Kadın
        public string Hedef { get; set; } // Kilo Vermek / Kas Yapmak

        // المخرجات (النتيجة التي سيولدها النظام)
        public string? SonucBaslik { get; set; }
        public string? Tavsiye { get; set; }
        public string? EgzersizPlani { get; set; }
    }
}