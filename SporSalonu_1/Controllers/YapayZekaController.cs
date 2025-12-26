using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SporSalonu_1.Controllers
{
    public class YapayZekaController : Controller
    {
        // هذه الدالة تعرض الصفحة التي يدخل فيها المستخدم بياناته
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TavsiyeAl(double boy, double kilo, int yas, string hedef)
        {
            // هنا نقوم بصياغة "البرومبت" الذي سيُرسل للذكاء الاصطناعي
            string prompt = $"{boy} cm boyunda, {kilo} kg ağırlığında ve {yas} yaşında olan، " +
                            $"hedefi {hedef} olan bir birey için kişiselleştirilmiş " +
                            $"bir spor ve beslenme programı hazırla.";

            // محاكاة الاتصال بـ OpenAI API
            // في الرابور سنشرح أن هذا الجزء يرسل البيانات لموديل GPT
            string yapayZekaYaniti = await FakeOpenAIService(prompt);

            ViewBag.Yanit = yapayZekaYaniti;
            return View("Index");
        }

        // دالة وهمية لمحاكاة استجابة الذكاء الاصطناعي
        private async Task<string> FakeOpenAIService(string prompt)
        {
            await Task.Delay(1000); // محاكاة وقت التفكير
            return "Vücut analiziniz yapıldı: Günlük 2500 kalori almalı ve haftada 4 gün antrenman yapmalısınız. " +
                   "Önerilen: Yüksek proteinli diyet ve kardiyo ağırlıklı program.";
        }
    }
}