using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SporSalon_1.Data;
using SporSalon_1.Models;

var builder = WebApplication.CreateBuilder(args);

// تأكد أن الكلمة بين القوسين هي "DefaultConnection" بالضبط
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// إضافة خدمة الهوية (Identity)
builder.Services.AddIdentity<Uye, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
