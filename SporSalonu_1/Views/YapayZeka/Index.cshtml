using Microsoft.AspNetCore.Mvc;
using SporSalon_1.Models;

namespace SporSalonu_1.Controllers
{
    public class YapayZekaController : Controller
    {
        // 1. عرض الصفحة الفارغة
        [HttpGet]
        public IActionResult Index()
        {
            return View(new YapayZekaModel());
        }

        // 2. استقبال البيانات وتحليلها (المحاكاة)
        [HttpPost]
        public IActionResult Hesapla(YapayZekaModel model)
        {
            if (model.Boy > 0 && model.Kilo > 0)
            {
                // حساب مؤشر كتلة الجسم (BMI)
                double boyMetre = model.Boy / 100.0;
                double vki = model.Kilo / (boyMetre * boyMetre);

                // --- خوارزمية الذكاء الاصطناعي (تحليل البيانات) ---

                string durum = "";
                string tavsiye = "";
                string plan = "";

                // 1. تحليل الوزن
                if (vki < 18.5)
                {
                    durum = "Zayıf (Düşük Kilolu)";
                    tavsiye = "Günlük kalori ihtiyacınızın 500 kalori üzerine çıkmalısınız. Karbonhidrat ve protein dengesini artırın.";
                    plan = "Haftada 3 gün Full Body (Tüm Vücut) antrenmanı + Düşük yoğunluklu kardiyo.";
                }
                else if (vki >= 18.5 && vki < 25)
                {
                    durum = "Normal (İdeal Kilo)";
                    tavsiye = "Mevcut kilonuzu koruyarak kas kütlesini artırmaya odaklanın. Dengeli beslenme şart.";
                    plan = "Haftada 4 gün Bölgesel (Split) antrenman + Haftada 1 gün HIIT kardiyo.";
                }
                else if (vki >= 25 && vki < 30)
                {
                    durum = "Fazla Kilolu";
                    tavsiye = "Şeker ve hamur işini kesin. Akşam 19:00'dan sonra yemeyin. Su tüketimini artırın.";
                    plan = "Haftada 3 gün Ağırlık antrenmanı + Her antrenman sonrası 20 dk tempolu yürüyüş.";
                }
                else
                {
                    durum = "Obezite Sınırı";
                    tavsiye = "Acilen diyete başlamalısınız. İşlenmiş gıdalardan uzak durun ve sebze ağırlıklı beslenin.";
                    plan = "Haftada 5 gün sabah aç karnına 45 dk yürüyüş + Hafif ağırlıklarla direnç egzersizleri.";
                }

                // 2. تعديل النتيجة بناءً على هدف المستخدم
                if (model.Hedef == "Kas")
                {
                    tavsiye += " Kas gelişimi için protein alımını (Kilo x 2g) seviyesine çıkarın.";
                }
                else if (model.Hedef == "Zayiflama")
                {
                    tavsiye += " Kilo vermek için kalori açığı oluşturmaya odaklanın.";
                }
                else if (model.Hedef == "KiloAl")
                {
                    tavsiye += " Sağlıklı kilo almak için karbonhidrat miktarını artırın ve öğün atlamayın. (Günde 5-6 öğün).";
                    plan += " + Ağırlık artırmaya yönelik (Hipertrofi) antrenmanları ekleyin.";
                }

                // إعداد النتيجة للعرض
                model.SonucBaslik = $"Vücut Kitle İndeksiniz: {vki:F1} ({durum})";
                model.Tavsiye = tavsiye;
                model.EgzersizPlani = plan;
            }

            return View("Index", model);
        }
    }
}