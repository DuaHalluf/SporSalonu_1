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
    // 🚨 أزلنا القيد العام عن الكلاس لكي يستطيع الأعضاء رؤية صفحة العرض
    public class HizmetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HizmetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hizmets
        // ✅ هذه الصفحة متاحة للجميع (ليرى الأعضاء الخدمات)
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Hizmetler.Include(h => h.SporSalonu);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Hizmets/Details/5
        // ✅ هذه الصفحة متاحة للجميع (ليرى الأعضاء تفاصيل الخدمة قبل الحجز)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler
                .Include(h => h.SporSalonu)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hizmet == null)
            {
                return NotFound();
            }

            return View(hizmet);
        }

        // 🔒 --- منطقة الأدمن (Create, Edit, Delete) --- 🔒

        // GET: Hizmets/Create
        [Authorize(Roles = "Admin")] // 🚫 للأدمن فقط
        public IActionResult Create()
        {
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad");
            return View();
        }

        // POST: Hizmets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] //  للأدمن فقط
        public async Task<IActionResult> Create([Bind("Id,Ad,Aciklama,Sure,Ucret,SporSalonuId")] Hizmet hizmet)
        {
            // Remove navigation property validation error if it exists
            ModelState.Remove("SporSalonu");

            if (ModelState.IsValid)
            {
                _context.Add(hizmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad", hizmet.SporSalonuId);
            return View(hizmet);
        }

        // GET: Hizmets/Edit/5
        [Authorize(Roles = "Admin")] // 🚫 للأدمن فقط
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null)
            {
                return NotFound();
            }
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad", hizmet.SporSalonuId);
            return View(hizmet);
        }

        // POST: Hizmets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // 🚫 للأدمن فقط
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Aciklama,Sure,Ucret,SporSalonuId")] Hizmet hizmet)
        {
            if (id != hizmet.Id)
            {
                return NotFound();
            }

            // Remove navigation property validation error if it exists
            ModelState.Remove("SporSalonu");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hizmet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HizmetExists(hizmet.Id))
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
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad", hizmet.SporSalonuId);
            return View(hizmet);
        }

        // GET: Hizmets/Delete/5
        [Authorize(Roles = "Admin")] // 🚫 للأدمن فقط
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler
                .Include(h => h.SporSalonu)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hizmet == null)
            {
                return NotFound();
            }

            return View(hizmet);
        }

        // POST: Hizmets/Delete/5
        // POST: Hizmets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // تأكد من وجود هذا السطر إذا كنت تريد حمايتها
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. جلب الخدمة مع المواعيد المرتبطة بها
            var hizmet = await _context.Hizmetler
                .Include(h => h.Randevular) // 🚨 ضروري جداً: استدعاء جدول المواعيد
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hizmet != null)
            {
                // 2. إذا كانت هناك مواعيد لهذه الخدمة، نحذفها أولاً
                if (hizmet.Randevular != null && hizmet.Randevular.Any())
                {
                    _context.Randevular.RemoveRange(hizmet.Randevular);
                }

                // 3. الآن نحذف الخدمة بأمان
                _context.Hizmetler.Remove(hizmet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool HizmetExists(int id)
        {
            return _context.Hizmetler.Any(e => e.Id == id);
        }
    }
}