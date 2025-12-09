using Microsoft.AspNetCore.Identity;
using SporSalon_1.Models; // تأكد من اسم النيم سبيس الصحيح لمشروعك
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SporSalon_1.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var userManager = service.GetService<UserManager<Uye>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. إنشاء الأدوار
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Member"))
                await roleManager.CreateAsync(new IdentityRole("Member"));

            // 2. بيانات الأدمن المطلوبة
            // 👇 الإيميل الذي طلبته
            string adminEmail = "G221210564@sakarya.edu.tr";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new Uye
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    AdSoyad = "Admin G22", // اسم توضيحي
                    EmailConfirmed = true,
                    Boy = 0,
                    Kilo = 0
                };

                // 3. إنشاء المستخدم بكلمة "sau"
                // ⚠️ (سيعمل هذا فقط إذا طبقت الخطوة 1 في Program.cs)
                var result = await userManager.CreateAsync(newAdmin, "sau");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}