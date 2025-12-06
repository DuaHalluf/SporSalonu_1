using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SporSalon_1.Data; // 1. ????? ??????
using SporSalon_1.Models;
using SporSalonu_1.Models;
using System.Diagnostics;
using System.Linq; // 2. ???????? ????????

namespace SporSalon_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // 3. ????? ????? ????????

        // 4. ??? ????? ???????? ?? ????????????
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // 5. ??? ??? ???????? ????? ???? ?? ??????????
            if (User.IsInRole("Admin"))
            {
                ViewBag.UyeSayisi = _context.Uyeler.Count(); // ??????? ?????? ?? ???? ??? ????
                ViewBag.AntrenorSayisi = _context.Antrenorler.Count();
                ViewBag.RandevuSayisi = _context.Randevular.Count();
                ViewBag.SporSalonuSayisi = _context.SporSalonlari.Count();

                // ??? ?????? ?????
                ViewBag.BugunRandevu = _context.Randevular.Count(r => r.Tarih.Date == DateTime.Today);
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
