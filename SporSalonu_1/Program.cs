using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SporSalon_1.Data;
using SporSalon_1.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. إعداد الاتصال بقاعدة البيانات
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. إعداد الهوية (Identity) مع تخفيف شروط كلمة المرور (هذا الجزء المعدل) 🚨
builder.Services.AddIdentity<Uye, IdentityRole>(options =>
{
    // تخفيف القيود لتناسب كلمة السر "sau"
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;

    // عدم طلب تأكيد الإيميل للدخول
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. تشغيل زارع البيانات (Seeder) لإنشاء الأدمن (هذا الجزء المضاف) 🚨
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // استدعاء الكلاس الذي أنشأناه لإنشاء الأدمن
        await SporSalon_1.Data.DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// إعدادات الـ Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// تفعيل نظام الأمان
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();