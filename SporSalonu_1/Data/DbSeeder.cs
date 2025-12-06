using Microsoft.AspNetCore.Identity;
using SporSalon_1.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SporSalon_1.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // استدعاء مديري المستخدمين والأدوار
            var userManager = service.GetService<UserManager<Uye>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. إنشاء الأدوار (Roles) إذا لم تكن موجودة
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("Member"));

            // 2. إنشاء الأدمن إذا لم يكن موجوداً
            // 🚨 تأكد من تغيير الرقم الجامعي هنا إذا لزم الأمر
            string adminEmail = "G221210564@sakarya.edu.tr";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new Uye
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    AdSoyad = "System Admin",
                    EmailConfirmed = true,
                    Boy = 0,   // قيم افتراضية
                    Kilo = 0
                };

                // إنشاء الأدمن بكلمة السر "sau"
                var result = await userManager.CreateAsync(newAdmin, "sau");

                if (result.Succeeded)
                {
                    // إعطاء صلاحية الأدمن لهذا المستخدم
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}