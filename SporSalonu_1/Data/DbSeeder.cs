using Microsoft.AspNetCore.Identity;
using SporSalon_1.Models; // تأكد أن الـ Namespace صحيح
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

            // 1. إنشاء الأدوار إذا لم تكن موجودة
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("Member"));

            // 2. بيانات الأدمن
            string adminEmail = "G221210564@sakarya.edu.tr";

            // البحث عن المستخدم
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            // إذا لم يكن موجوداً، نقوم بإنشائه
            if (adminUser == null)
            {
                var newAdmin = new Uye
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    AdSoyad = "System Admin",
                    EmailConfirmed = true,
                    Boy = 0,
                    Kilo = 0
                };

                // كلمة السر كما هي مطلوبة في المشروع
                var result = await userManager.CreateAsync(newAdmin, "sau");

                if (result.Succeeded)
                {
                    adminUser = newAdmin; // تحديث المتغير لاستخدامه في الخطوة التالية
                }
            }

            // 3. 🚨 هذا هو الإصلاح: التحقق من الصلاحية ومنحها حتى لو المستخدم موجود مسبقاً
            if (adminUser != null)
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}