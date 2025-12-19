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

namespace SporSalonu_1.Controllers
{
    // 🔒 الحماية العامة: كل الوظائف محجوبة إلا للأدمن بشكل افتراضي
    [Authorize(Roles = "Admin")]
    public class SporSalonusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SporSalonusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ✅ السماح للجميع (حتى غير المسجلين) برؤية القائمة
        // =========================================================
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.SporSalonlari.ToListAsync());
        }

        // =========================================================
        // ✅ السماح للجميع برؤية التفاصيل
        // =========================================================
        // GET: SporSalonus/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sporSalonu = await _context.SporSalonlari
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sporSalonu == null) return NotFound();

            // 👇 التعديل الجديد: جلب كل الخدمات المتاحة في النظام وإرسالها للصفحة
            ViewBag.TumHizmetler = await _context.Hizmetler.ToListAsync();

            return View(sporSalonu);
        }

        // =========================================================
        // 🔒 الإضافة (محمية تلقائياً بسبب Authorize في الأعلى)
        // =========================================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ad,Adres,CalismaSaatleri,Telefon")] SporSalonu sporSalonu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sporSalonu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sporSalonu);
        }

        // =========================================================
        // 🔒 التعديل (محمية تلقائياً)
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sporSalonu = await _context.SporSalonlari.FindAsync(id);
            if (sporSalonu == null)
            {
                return NotFound();
            }
            return View(sporSalonu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Adres,CalismaSaatleri,Telefon")] SporSalonu sporSalonu)
        {
            if (id != sporSalonu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sporSalonu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SporSalonuExists(sporSalonu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sporSalonu);
        }

        // =========================================================
        // 🔒 الحذف (محمية تلقائياً)
        // =========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sporSalonu = await _context.SporSalonlari
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sporSalonu == null)
            {
                return NotFound();
            }

            return View(sporSalonu);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. جلب الصالة مع كل تفاصيلها (المدربين، الخدمات، والمواعيد المرتبطة بهم)
            var sporSalonu = await _context.SporSalonlari
                .Include(s => s.Antrenorler).ThenInclude(a => a.Randevular)
                .Include(s => s.Hizmetler).ThenInclude(h => h.Randevular)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sporSalonu != null)
            {
                // 2. تنظيف المواعيد المرتبطة بمدربين هذه الصالة
                if (sporSalonu.Antrenorler != null)
                {
                    foreach (var antrenor in sporSalonu.Antrenorler)
                    {
                        if (antrenor.Randevular != null && antrenor.Randevular.Any())
                        {
                            _context.Randevular.RemoveRange(antrenor.Randevular);
                        }
                    }
                    // حذف المدربين
                    _context.Antrenorler.RemoveRange(sporSalonu.Antrenorler);
                }

                // 3. تنظيف المواعيد المرتبطة بخدمات هذه الصالة
                if (sporSalonu.Hizmetler != null)
                {
                    foreach (var hizmet in sporSalonu.Hizmetler)
                    {
                        if (hizmet.Randevular != null && hizmet.Randevular.Any())
                        {
                            _context.Randevular.RemoveRange(hizmet.Randevular);
                        }
                    }
                    // حذف الخدمات
                    _context.Hizmetler.RemoveRange(sporSalonu.Hizmetler);
                }

                // 4. الآن الصالة فارغة تماماً، يمكننا حذفها بأمان
                _context.SporSalonlari.Remove(sporSalonu);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SporSalonuExists(int id)
        {
            return _context.SporSalonlari.Any(e => e.Id == id);
        }
    }
}