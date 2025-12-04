using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SporSalon_1.Models;

namespace SporSalon_1.Data
{
    public class ApplicationDbContext : IdentityDbContext<Uye>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SporSalonu> SporSalonlari { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- حل مشكلة التضارب (Cycles or Multiple Cascade Paths) ---

            // عند حذف مدرب، لا تحذف المواعيد المرتبطة به تلقائياً
            builder.Entity<Randevu>()
                .HasOne(r => r.Antrenor)
                .WithMany(a => a.Randevular)
                .HasForeignKey(r => r.AntrenorId)
                .OnDelete(DeleteBehavior.Restrict);

            // عند حذف خدمة، لا تحذف المواعيد المرتبطة بها تلقائياً
            builder.Entity<Randevu>()
                .HasOne(r => r.Hizmet)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // عند حذف صالة، لا تحذف المدربين والخدمات تلقائياً لتجنب المشاكل
            builder.Entity<Hizmet>()
               .HasOne(h => h.SporSalonu)
               .WithMany(s => s.Hizmetler)
               .HasForeignKey(h => h.SporSalonuId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Antrenor>()
               .HasOne(a => a.SporSalonu)
               .WithMany(s => s.Antrenorler)
               .HasForeignKey(a => a.SporSalonuId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}