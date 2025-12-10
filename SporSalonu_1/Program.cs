using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SporSalon_1.Data;
using SporSalon_1.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. إعداد الاتصال بقاعدة البيانات
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. إعداد الهوية (Identity) مع تخفيف شروط كلمة المرور لتقبل "sau"
builder.Services.AddIdentity<Uye, IdentityRole>(options =>
{
    // تخفيف القيود لتناسب كلمة السر البسيطة
    options.Password.RequireDigit = false;           // لا يشترط أرقام
    options.Password.RequireLowercase = false;       // لا يشترط حروف صغيرة
    options.Password.RequireUppercase = false;       // لا يشترط حروف كبيرة
    options.Password.RequireNonAlphanumeric = false; // لا يشترط رموز
    options.Password.RequiredLength = 3;             // الطول المسموح 3 أحرف

    // إعدادات الدخول
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ✅ إضافة: ضبط مسارات الدخول والوصول المرفوض بشكل صريح
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";             // مسار صفحة الدخول
    options.AccessDeniedPath = "/Account/AccessDenied"; // مسار صفحة رفض الوصول (التي أنشأناها)
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. تشغيل زارع البيانات (Seeder) لإنشاء الأدمن تلقائياً
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // استدعاء دالة الـ Seeding (تأكد أنك عدلت ملف DbSeeder.cs كما اتفقنا سابقاً)
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "حدث خطأ أثناء تهيئة قاعدة البيانات (Seeding Error).");
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
app.UseAuthentication(); // معرفة من هو المستخدم
app.UseAuthorization();  // معرفة صلاحيات المستخدم

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();