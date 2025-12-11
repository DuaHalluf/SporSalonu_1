using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SporSalon_1.Data;
using SporSalon_1.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SporSalon_1.Controllers
{
    [Authorize] // 🔒 إجبار تسجيل الدخول
    public class RandevusController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Uye> _userManager;

        public RandevusController(ApplicationDbContext context, UserManager<Uye> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================================
        //  دالة AJAX لجلب المدربين حسب الخدمة (ديناميكية)

        // 4. ترتيب القائمة
        [HttpGet]
        public async Task<JsonResult> GetAntrenorlerByHizmet(int hizmetId, int? sporSalonuId) // sporSalonuId قد تكون null أو 0
        {
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);
            if (hizmet == null) return Json(new List<object>());

            string searchKeyword = hizmet.Ad.Trim();
            var turkishCulture = new CultureInfo("tr-TR");

            // 1. نبدأ بجميع المدربين في النظام
            var allAntrenorler = await _context.Antrenorler.ToListAsync();

            // 2. 🚨 التصفية الأولى (الحاسمة): التخصص (Expertise Filter)
            var uzmanAntrenorler = allAntrenorler
                .Where(a => !string.IsNullOrEmpty(a.UzmanlikAlani))
                .Where(a =>
                    turkishCulture.CompareInfo.IndexOf(a.UzmanlikAlani, searchKeyword, CompareOptions.IgnoreCase) >= 0 ||
                    a.UzmanlikAlani.ToLower().Contains(searchKeyword.ToLower())
                 ).ToList();

            // 3. 🚨 التصفية الثانية: الصالة (Location Filter)
            if (sporSalonuId.HasValue && sporSalonuId.Value > 0)
            {
                // إذا كان الحجز موجهاً من صالة معينة، نأخذ فقط المدربين المختصين الذين يعملون في هذه الصالة
                uzmanAntrenorler = uzmanAntrenorler.Where(a => a.SporSalonuId == sporSalonuId.Value).ToList();
            }
            // إذا لم يكن هناك تحديد للصالة، فإننا نأخذ جميع المختصين من جميع الفروع (وهذا صحيح).

            // 4. تجهيز القائمة النهائية للعرض
            var sonucListesi = uzmanAntrenorler.Select(a => new
            {
                id = a.Id,
                // الجميع الآن هم مختصون، لذا نعرضهم كخبراء
                ad = a.AdSoyad + " (🌟 Uzman)"
            }).ToList();

            // 5. إذا كانت القائمة فارغة، نعيد قائمة فارغة للعرض (وهذا يعني: لا يوجد مدرب مختص في هذه الخدمة/الصالة)
            if (sonucListesi.Count == 0 && sporSalonuId.HasValue && sporSalonuId.Value > 0)
            {
                // هنا نعرض رسالة مناسبة للمستخدم في الواجهة
                return Json(new List<object> { new { id = 0, ad = "-- Bu salonda uzman bulunamadı --" } });
            }

            return Json(sonucListesi);
        }
        // ==========================================================
        // 1. صفحة عرض المواعيد (Index)
        // ==========================================================
        public async Task<IActionResult> Index()
        {
            var randevularQuery = _context.Randevular
                .Include(r => r.Antrenor)
                .ThenInclude(a => a.SporSalonu)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye);

            // 👑 أدمن: يرى كل الحجوزات
            if (User.IsInRole("Admin"))
            {
                return View(await randevularQuery.OrderByDescending(r => r.Tarih).ToListAsync());
            }
            // 👤 عضو: يرى حجوزاته فقط
            else
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                return View(await randevularQuery
                    .Where(r => r.UyeId == user.Id)
                    .OrderByDescending(r => r.Tarih)
                    .ToListAsync());
            }
        }

        // صفحة النجاح (بعد الحجز)
        public IActionResult Success()
        {
            return View();
        }

        // ==========================================================
        // 2. صفحة الحجز الجديدة - GET (تم التعديل هنا ✅)
        // ==========================================================
        // GET: Randevus/Create
        // نستقبل الخدمة (hizmetId) والمدرب (antrenorId) كخيارات
        // GET: Randevus/Create
        // GET: Randevus/Create
        public IActionResult Create(int? hizmetId, int? antrenorId, int? sporSalonuId) // تم تعديلها لاستقبال sporSalonuId
        {
            // تخزين رقم الصالة ليرسله الجافاسكربت لاحقاً في الـ AJAX
            ViewBag.FilterSporSalonuId = sporSalonuId ?? 0;

            // 1. تحديد قائمة المدربين الأولية
            IQueryable<Antrenor> antrenorQuery = _context.Antrenorler.AsQueryable();

            // 🚨 الشرط الأهم: إذا كان الحجز موجهاً من صالة، صَفِّ قائمة المدربين الأولية
            if (sporSalonuId.HasValue && sporSalonuId.Value > 0)
            {
                antrenorQuery = antrenorQuery.Where(a => a.SporSalonuId == sporSalonuId.Value);
            }

            // إرسال القائمة الأولية للصفحة
            ViewData["AntrenorId"] = new SelectList(antrenorQuery.ToList(), "Id", "AdSoyad", antrenorId);


            // 2. منطق الخدمات (كما هو)
            if (hizmetId.HasValue)
            {
                var secilenHizmet = _context.Hizmetler.Find(hizmetId.Value);
                if (secilenHizmet != null)
                {
                    ViewBag.PreSelectedHizmet = secilenHizmet;
                    // يجب تعيين القيمة المختارة للخدمة لتظهر في الحقل المخفي
                    ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", hizmetId.Value);
                }
                else
                {
                    ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad");
                }
            }
            else
            {
                ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad");
            }

            return View();
        }
        // ==========================================================
        // 3. عملية الحفظ - POST
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu randevu)
        {
            // 1. ربط المستخدم الحالي
            var user = await _userManager.GetUserAsync(User);
            randevu.UyeId = user.Id;

            // 2. التحقق من التخصص (Validation)
            var secilenAntrenor = await _context.Antrenorler.FindAsync(randevu.AntrenorId);
            var secilenHizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);

            if (secilenAntrenor != null && secilenHizmet != null)
            {
                string uzmanlik = secilenAntrenor.UzmanlikAlani.Trim();
                string hizmetAd = secilenHizmet.Ad.Trim();
                var turkishCulture = new CultureInfo("tr-TR");

                bool isExpert = turkishCulture.CompareInfo.IndexOf(uzmanlik, hizmetAd, CompareOptions.IgnoreCase) >= 0;

                if (!isExpert)
                {
                    var tumAntrenorler = await _context.Antrenorler.ToListAsync();
                    var uzmanAntrenorler = tumAntrenorler
                        .Where(a => !string.IsNullOrEmpty(a.UzmanlikAlani) &&
                                    turkishCulture.CompareInfo.IndexOf(a.UzmanlikAlani, hizmetAd, CompareOptions.IgnoreCase) >= 0)
                        .Select(a => a.AdSoyad)
                        .ToList();

                    if (uzmanAntrenorler.Any())
                    {
                        string onerilenler = string.Join(", ", uzmanAntrenorler);
                        TempData["Hata"] = $"Hata: Seçilen antrenör ({secilenAntrenor.AdSoyad}) '{secilenHizmet.Ad}' hizmetinde uzman değil!\n" +
                                          $"Lütfen şu uzmanlardan birini seçiniz: {onerilenler}";
                    }
                    else
                    {
                        TempData["Hata"] = $"Hata: Seçilen antrenör ({secilenAntrenor.AdSoyad}) bu hizmette uzman değil.";
                    }

                    // إعادة تعبئة القوائم عند الخطأ
                    ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
                    ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);

                    // نحتفظ بحالة "القفل" إذا كانت موجودة
                    if (randevu.HizmetId != 0)
                    {
                        var preHizmet = _context.Hizmetler.Find(randevu.HizmetId);
                        if (preHizmet != null) ViewBag.PreSelectedHizmet = preHizmet;
                    }

                    return View(randevu);
                }
            }

            // 3. التحقق من تعارض الوقت (Conflict Check)
            bool isBusy = _context.Randevular.Any(x =>
                x.AntrenorId == randevu.AntrenorId &&
                x.Tarih.Date == randevu.Tarih.Date &&
                x.Saat == randevu.Saat);

            if (isBusy)
            {
                TempData["Hata"] = "Seçilen antrenör bu saatte dolu! Lütfen başka bir saat seçiniz.";
                ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
                ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);

                if (randevu.HizmetId != 0)
                {
                    var preHizmet = _context.Hizmetler.Find(randevu.HizmetId);
                    if (preHizmet != null) ViewBag.PreSelectedHizmet = preHizmet;
                }
                return View(randevu);
            }

            // 4. تجاهل التحقق من الخصائص المرتبطة (Navigation Properties)
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");
            ModelState.Remove("Uye");

            // 5. الحفظ النهائي
            if (ModelState.IsValid)
            {
                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Success));
            }

            // في حال فشل الـ Model Validation
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
            return View(randevu);
        }

        // ==========================================================
        // 4. التفاصيل
        // ==========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        // ==========================================================
        // 5. الحذف - GET
        // ==========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        // ==========================================================
        // 6. تأكيد الحذف - POST
        // ==========================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}