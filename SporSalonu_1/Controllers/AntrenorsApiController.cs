using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalon_1.Data;   // تأكد أن الاسم يطابق مسار الـ Data عندك
using SporSalon_1.Models; // تأكد أن الاسم يطابق مسار الـ Models عندك

namespace SporSalonu_1.Controllers
{
    // 1. تعريف الـ API
    [Route("api/[controller]")]
    [ApiController]
    public class AntrenorsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AntrenorsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 2. دالة لجلب جميع المدربين (GET: api/AntrenorsApi)
        // تحقق شرط: "Tüm antrenörleri listeleme"
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAntrenorler()
        {
            // استخدام LINQ لتحديد الحقول المطلوبة فقط (Select)
            // هذا يمنع مشاكل التكرار في JSON ويحقق شرط استخدام LINQ
            var antrenorler = await _context.Antrenorler
                .Select(a => new
                {
                    id = a.Id,
                    adSoyad = a.AdSoyad,
                    uzmanlik = a.UzmanlikAlani,
                    calismaSaatleri = a.CalismaSaatleri
                })
                .ToListAsync();

            return Ok(antrenorler);
        }

        // 3. دالة لجلب مدرب محدد بالـ ID (GET: api/AntrenorsApi/5)
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAntrenor(int id)
        {
            var antrenor = await _context.Antrenorler
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    id = a.Id,
                    adSoyad = a.AdSoyad,
                    uzmanlik = a.UzmanlikAlani
                })
                .FirstOrDefaultAsync();

            if (antrenor == null)
            {
                return NotFound();
            }

            return Ok(antrenor);
        }
    }
}