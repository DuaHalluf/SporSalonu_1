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
        // ==========================================================
        [HttpGet]
        public async Task<JsonResult> GetAntrenorlerByHizmet(int hizmetId)
        {
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);

            if (hizmet == null)
            {
                return Json(new List<object>());
            }

            // تنظيف اسم الخدمة
            string searchKeyword = hizmet.Ad.Trim();
            var turkishCulture = new CultureInfo("tr-TR");

            var tumAntrenorler = await _context.Antrenorler.ToListAsync();

            var uygunAntrenorler = tumAntrenorler
                .Where(a => !string.IsNullOrEmpty(a.UzmanlikAlani))
                .Where(a =>
                    // مقارنة ذكية تتجاهل حالة الأحرف والرموز التركية
                    turkishCulture.CompareInfo.IndexOf(a.UzmanlikAlani, searchKeyword, CompareOptions.IgnoreCase) >= 0
                    ||
                    a.UzmanlikAlani.ToLower().Contains(searchKeyword.ToLower())
                 )
                .Select(a => new {
                    id = a.Id,
                    ad = a.AdSoyad
                })
                .ToList();

            return Json(uygunAntrenorler);
        }

        // ==========================================================
        // 1. صفحة عرض المواعيد (Index)
        // ==========================================================
        public async Task<IActionResult> Index()
        {
            var randevularQuery = _context.Randevular
                .Include(r => r.Antrenor)
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
        public IActionResult Create(int? hizmetId) // استقبال رقم الخدمة الاختياري
        {
            // 1. إذا جاء المستخدم من زر "Randevu Al" الخاص بخدمة معينة
            if (hizmetId.HasValue)
            {
                var secilenHizmet = _context.Hizmetler.Find(hizmetId.Value);
                if (secilenHizmet != null)
                {
                    // نرسل الخدمة المختارة للواجهة لقفل الحقل
                    ViewBag.PreSelectedHizmet = secilenHizmet;

                    // تحديد القيمة في القائمة المنسدلة أيضاً (احتياط)
                    ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", hizmetId.Value);
                }
                else
                {
                    ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad");
                }
            }
            else
            {
                // الوضع الطبيعي: قائمة فارغة يختار منها المستخدم
                ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad");
            }

            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad");
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