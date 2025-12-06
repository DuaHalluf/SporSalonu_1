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
using Microsoft.AspNetCore.Authorization;

namespace SporSalon_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UyesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        // 🚨 انتبه: يجب أن يكون هذا الجزء موجوداً مرة واحدة فقط
        public UyesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // 1. Index
        public async Task<IActionResult> Index()
        {
            return View(await _context.Uyeler.ToListAsync());
        }

        // 2. Details
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var uye = await _context.Uyeler.FirstOrDefaultAsync(m => m.Id == id);
            if (uye == null) return NotFound();
            return View(uye);
        }

        // 3. Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Uye uye, IFormFile file)
        {
            if (_context.Uyeler.Any(u => u.Email == uye.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
            }

            ModelState.Remove("ResimUrl");
            ModelState.Remove("UserName");
            ModelState.Remove("Randevular");

            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string pathFolder = Path.Combine(wwwRootPath, "images");
                    if (!Directory.Exists(pathFolder)) Directory.CreateDirectory(pathFolder);

                    string fullPath = Path.Combine(pathFolder, fileName);
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    uye.ResimUrl = "/images/" + fileName;
                }

                uye.UserName = uye.Email;
                uye.NormalizedUserName = uye.Email?.ToUpper();
                uye.NormalizedEmail = uye.Email?.ToUpper();
                uye.EmailConfirmed = true;
                uye.SecurityStamp = Guid.NewGuid().ToString();

                _context.Add(uye);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(uye);
        }

        // 4. Edit
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var uye = await _context.Uyeler.FindAsync(id);
            if (uye == null) return NotFound();
            return View(uye);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Uye uye, IFormFile? file)
        {
            if (id != uye.Id) return NotFound();
            var existingUye = await _context.Uyeler.FindAsync(id);
            if (existingUye == null) return NotFound();

            existingUye.AdSoyad = uye.AdSoyad;
            existingUye.PhoneNumber = uye.PhoneNumber;
            existingUye.Boy = uye.Boy;
            existingUye.Kilo = uye.Kilo;
            existingUye.DogumTarihi = uye.DogumTarihi;

            if (file != null)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string path = Path.Combine(wwwRootPath, "images", fileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                existingUye.ResimUrl = "/images/" + fileName;
            }

            try
            {
                _context.Update(existingUye);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Uyeler.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // 5. Delete
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var uye = await _context.Uyeler.FirstOrDefaultAsync(m => m.Id == id);
            if (uye == null) return NotFound();
            return View(uye);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var uye = await _context.Uyeler.FindAsync(id);
            if (uye != null)
            {
                if (!string.IsNullOrEmpty(uye.ResimUrl))
                {
                    var imagePath = Path.Combine(_hostEnvironment.WebRootPath, uye.ResimUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
                }
                _context.Uyeler.Remove(uye);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}