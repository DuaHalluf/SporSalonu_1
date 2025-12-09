using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalon_1.Data;
using SporSalon_1.Models;
using System.Threading.Tasks;
using System.Linq;

namespace SporSalon_1.Controllers
{
    [Authorize] // 🔒 إجبار المستخدم على تسجيل الدخول للوصول لهذا الكنترولر
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Uye> _userManager;

        public RandevuController(ApplicationDbContext context, UserManager<Uye> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. صفحة عرض المواعيد (Randevularım)
        public async Task<IActionResult> Index()
        {
            // جلب المستخدم الحالي المسجل للدخول
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // جلب المواعيد الخاصة بهذا المستخدم فقط + بيانات المدرب
            var randevular = await _context.Randevular
                                           .Include(r => r.Antrenor) // لكي يظهر اسم المدرب
                                           .Include(r => r.Hizmet)   // لكي يظهر اسم الخدمة (سباحة، كمال أجسام...)
                                           .Where(r => r.UyeId == user.Id) // التصفية حسب المستخدم
                                           .OrderByDescending(r => r.Tarih) // الأحدث أولاً
                                           .ToListAsync();

            return View(randevular);
        }

        // 2. صفحة الحجز الجديدة (Form) - GET
        public IActionResult Create()
        {
            // إرسال قوائم المدربين والخدمات للقائمة المنسدلة
            ViewBag.Antrenorler = _context.Antrenorler.ToList();
            ViewBag.Hizmetler = _context.Hizmetler.ToList();
            return View();
        }

        // 3. عملية الحفظ - POST
        [HttpPost]
        public async Task<IActionResult> Create(Randevu randevu)
        {
            var user = await _userManager.GetUserAsync(User);
            randevu.UyeId = user.Id; // ربط الموعد بالمستخدم الحالي

            // 🛑 منطق منع التعارض (Conflict Check)
            // هل هذا المدرب مشغول في نفس التاريخ ونفس الساعة؟
            bool isBusy = _context.Randevular.Any(x =>
                x.AntrenorId == randevu.AntrenorId &&
                x.Tarih.Date == randevu.Tarih.Date &&
                x.Saat == randevu.Saat);

            if (isBusy)
            {
                // إذا كان مشغولاً، نرسل خطأ
                TempData["Hata"] = "Seçilen antrenör bu saatte dolu! Lütfen başka bir saat seçiniz.";

                // إعادة تعبئة القوائم لتظهر عند العودة للفورم
                ViewBag.Antrenorler = _context.Antrenorler.ToList();
                ViewBag.Hizmetler = _context.Hizmetler.ToList();
                return View(randevu);
            }

            // ✅ إذا كان متاحاً، نحفظ الموعد
            _context.Randevular.Add(randevu);
            await _context.SaveChangesAsync();

            TempData["Basari"] = "Randevunuz başarıyla oluşturuldu!";
            return RedirectToAction("Index");
        }

        // 4. حذف الموعد (اختياري)
        public async Task<IActionResult> Delete(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
