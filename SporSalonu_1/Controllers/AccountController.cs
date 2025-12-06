using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SporSalon_1.Models;

namespace SporSalon_1.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Uye> _signInManager;
        private readonly UserManager<Uye> _userManager;

        public AccountController(SignInManager<Uye> signInManager, UserManager<Uye> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // 1. فتح صفحة تسجيل الدخول (GET)
        public IActionResult Login()
        {
            return View();
        }

        // 2. التحقق من البيانات وتسجيل الدخول (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // البحث عن المستخدم بالإيميل
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // محاولة تسجيل الدخول
                // false هنا تعني "Remember Me" غير مفعل
                // false الثانية تعني "Lockout" (قفل الحساب بعد محاولات فاشلة) غير مفعل
                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            // إذا فشل الدخول
            ViewBag.Error = "E-posta veya şifre hatalı!";
            return View();
        }

        // ==========================================
        // صفحة التسجيل الجديد (Register)
        // ==========================================
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Uye model, string password)
        {
            // التحقق من تكرار الإيميل
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ViewBag.Error = "Bu e-posta adresi zaten kayıtlı!";
                return View(model);
            }

            // تجهيز المستخدم الجديد
            var user = new Uye
            {
                UserName = model.Email,
                Email = model.Email,
                AdSoyad = model.AdSoyad,
                PhoneNumber = model.PhoneNumber,
                DogumTarihi = model.DogumTarihi,
                Boy = model.Boy,
                Kilo = model.Kilo,
                EmailConfirmed = true // تفعيل الحساب تلقائياً للتسهيل
            };

            // 1. إنشاء المستخدم مع كلمة المرور
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // 2. إعطاء رتبة "Member" تلقائياً
                await _userManager.AddToRoleAsync(user, "Member");

                // 3. تسجيل الدخول مباشرة بعد التسجيل
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }

            // في حال وجود أخطاء (مثل كلمة مرور ضعيفة)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // 3. تسجيل الخروج (Logout)
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // 4. صفحة "غير مصرح لك" (Access Denied)
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}