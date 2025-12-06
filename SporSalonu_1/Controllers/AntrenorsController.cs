using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporSalon_1.Data;
using SporSalon_1.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization; // 1. 🚨 مكتبة الحماية

namespace SporSalonu_1.Controllers
{
    // 2. 🚨 حماية الكنترولر: الأدمن فقط يستطيع الوصول هنا
    [Authorize(Roles = "Admin")]
    public class AntrenorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AntrenorsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Antrenors
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Antrenorler.Include(a => a.SporSalonu);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Antrenors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // GET: Antrenors/Create
        public IActionResult Create()
        {
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad");
            return View();
        }

        // POST: Antrenors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AdSoyad,UzmanlikAlani,CalismaSaatleri,SporSalonuId")] Antrenor antrenor, IFormFile file)
        {
            // إزالة الحقول التي لا تأتي من الفورم لتجنب مشاكل الـ Validation
            ModelState.Remove("ResimUrl");
            ModelState.Remove("SporSalonu");

            if (ModelState.IsValid)
            {
                // كود رفع الصورة
                if (file != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    // التأكد من وجود المجلد
                    string pathFolder = Path.Combine(wwwRootPath, "images");
                    if (!Directory.Exists(pathFolder)) Directory.CreateDirectory(pathFolder);

                    string fullPath = Path.Combine(pathFolder, fileName);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    antrenor.ResimUrl = "/images/" + fileName;
                }

                _context.Add(antrenor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad", antrenor.SporSalonuId);
            return View(antrenor);
        }

        // GET: Antrenors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad", antrenor.SporSalonuId);
            return View(antrenor);
        }

        // POST: Antrenors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AdSoyad,UzmanlikAlani,ResimUrl,CalismaSaatleri,SporSalonuId")] Antrenor antrenor, IFormFile file)
        {
            if (id != antrenor.Id) return NotFound();

            // إزالة الأخطاء للحقول غير الضرورية أو التي سنعالجها يدوياً
            ModelState.Remove("ResimUrl");
            ModelState.Remove("SporSalonu");

            if (ModelState.IsValid)
            {
                try
                {
                    // جلب البيانات القديمة (بدون تتبع) لنعرف رابط الصورة القديم
                    var existingAntrenor = await _context.Antrenorler.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

                    if (existingAntrenor == null) return NotFound();

                    // معالجة الصورة
                    if (file != null)
                    {
                        // 1. حذف الصورة القديمة إذا وجدت
                        if (!string.IsNullOrEmpty(existingAntrenor.ResimUrl))
                        {
                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingAntrenor.ResimUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // 2. رفع الصورة الجديدة
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string path = Path.Combine(wwwRootPath, "images", fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        antrenor.ResimUrl = "/images/" + fileName;
                    }
                    else
                    {
                        // إذا لم يرفع صورة جديدة، نحتفظ بالقديمة
                        antrenor.ResimUrl = existingAntrenor.ResimUrl;
                    }

                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorExists(antrenor.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad", antrenor.SporSalonuId);
            return View(antrenor);
        }

        // GET: Antrenors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // POST: Antrenors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                // حذف الصورة من السيرفر عند حذف المدرب
                if (!string.IsNullOrEmpty(antrenor.ResimUrl))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, antrenor.ResimUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Antrenorler.Remove(antrenor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.Id == id);
        }
    }
}