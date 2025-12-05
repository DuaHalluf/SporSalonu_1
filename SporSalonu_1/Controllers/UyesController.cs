using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalon_1.Data;
using SporSalon_1.Models;

namespace SporSalonu_1.Controllers
{
    public class UyesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UyesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------
        // 1. صفحة عرض القائمة (Index) - موجودة عندك سابقاً
        // ---------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            // جلب قائمة الأعضاء من قاعدة البيانات (Uyeler)
            return View(await _context.Uyeler.ToListAsync());
        }

        // ---------------------------------------------------------
        // 2. (الإضافة الجديدة) فتح صفحة التعبئة (GET)
        // ---------------------------------------------------------
        public IActionResult Create()
        {
            return View();
        }

        // ---------------------------------------------------------
        // 3. (الإضافة الجديدة) حفظ البيانات عند الضغط على زر الحفظ (POST)
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Uye uye)
        {
            // بما أن العضو هو IdentityUser، يجب ملء بعض الحقول الإجبارية يدوياً لتجنب الأخطاء
            // سنجعل اسم المستخدم (UserName) هو نفسه الإيميل
            uye.UserName = uye.Email;
            uye.NormalizedUserName = uye.Email?.ToUpper();
            uye.NormalizedEmail = uye.Email?.ToUpper();
            uye.EmailConfirmed = true; // تفعيل الإيميل وهمياً

            // إضافة العضو للجدول وحفظ التغييرات
            _context.Add(uye);
            await _context.SaveChangesAsync();

            // العودة لصفحة القائمة الرئيسية بعد النجاح
            return RedirectToAction(nameof(Index));
        }
    }
}