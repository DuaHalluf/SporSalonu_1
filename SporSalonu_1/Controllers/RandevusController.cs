using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporSalon_1.Data;
using SporSalon_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace SporSalonu_1.Controllers
{
    [Authorize] // مسموح للأعضاء المسجلين والأدمن
    public class RandevusController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Uye> _userManager;

        public RandevusController(ApplicationDbContext context, UserManager<Uye> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Randevus
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // جلب المواعيد مع بيانات المدرب، العضو، والخدمة
            var randevular = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Uye)
                .Include(r => r.Hizmet) // 🚨 إضافة الخدمة للعرض
                .AsQueryable();

            // الأدمن يرى الكل، العضو يرى مواعيده فقط
            if (!User.IsInRole("Admin"))
            {
                randevular = randevular.Where(r => r.UyeId == user.Id);
            }

            return View(await randevular.ToListAsync());
        }

        // GET: Randevus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Uye)
                .Include(r => r.Hizmet) // 🚨 إضافة الخدمة للتفاصيل
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            // حماية الخصوصية
            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (randevu.UyeId != currentUser.Id) return Forbid();
            }

            return View(randevu);
        }

        // GET: Randevus/Create
        public IActionResult Create()
        {
            // إرسال قائمة المدربين
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad");
            // 🚨 إرسال قائمة الخدمات (هذا الجديد والمهم)
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad");

            return View();
        }

        // POST: Randevus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tarih,Saat,AntrenorId,HizmetId")] Randevu randevu)
        {
            // تحديد العضو تلقائياً
            var currentUser = await _userManager.GetUserAsync(User);
            randevu.UyeId = currentUser.Id;
            randevu.OlusturulmaTarihi = DateTime.Now;

            // تجاهل الحقول المرتبطة (Navigation Properties)
            ModelState.Remove("UyeId");
            ModelState.Remove("Uye");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");

            if (ModelState.IsValid)
            {
                // 🚨 التحقق من التعارض: هل المدرب مشغول؟
                bool isBusy = await _context.Randevular.AnyAsync(r =>
                    r.AntrenorId == randevu.AntrenorId &&
                    r.Tarih.Date == randevu.Tarih.Date &&
                    r.Saat == randevu.Saat);

                if (isBusy)
                {
                    ModelState.AddModelError("", "Seçilen antrenör bu tarih ve saatte dolu.");

                    // إعادة تعبئة القوائم عند الخطأ
                    ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
                    ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
                    return View(randevu);
                }

                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // في حال فشل التحقق، نعيد تعبئة القوائم
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
            return View(randevu);
        }

        // GET: Randevus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (randevu.UyeId != currentUser.Id) return Forbid();
            }

            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
            // 🚨 إضافة قائمة الخدمات للتعديل
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);

            return View(randevu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tarih,Saat,UyeId,AntrenorId,HizmetId")] Randevu randevu)
        {
            if (id != randevu.Id) return NotFound();

            ModelState.Remove("Uye");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(randevu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RandevuExists(randevu.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
            return View(randevu);
        }

        // GET: Randevus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Uye)
                .Include(r => r.Hizmet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (randevu == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (randevu.UyeId != currentUser.Id) return Forbid();
            }

            return View(randevu);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null) _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.Id == id);
        }
    }
}
