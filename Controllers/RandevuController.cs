using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HastaRandevuTakip.Models;

namespace HastaRandevuTakip.Controllers
{
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RandevuController> _logger;

        public RandevuController(ApplicationDbContext context, ILogger<RandevuController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Randevu
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TarihSortParm = sortOrder == "Tarih" ? "Tarih_desc" : "Tarih";
            ViewBag.HastaSortParm = sortOrder == "Hasta" ? "Hasta_desc" : "Hasta";

            var randevular = _context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Doktor)
                .AsQueryable();

            // Arama
            if (!string.IsNullOrEmpty(searchString))
            {
                randevular = randevular.Where(r =>
                    r.Hasta!.Ad.Contains(searchString) ||
                    r.Hasta!.Soyad.Contains(searchString) ||
                    r.DoktorAdi!.Contains(searchString) ||
                    r.Poliklinik!.Contains(searchString));
            }

            // Sıralama
            switch (sortOrder)
            {
                case "Tarih":
                    randevular = randevular.OrderBy(r => r.RandevuTarihi);
                    break;
                case "Tarih_desc":
                    randevular = randevular.OrderByDescending(r => r.RandevuTarihi);
                    break;
                case "Hasta":
                    randevular = randevular.OrderBy(r => r.Hasta!.Ad);
                    break;
                case "Hasta_desc":
                    randevular = randevular.OrderByDescending(r => r.Hasta!.Ad);
                    break;
                default:
                    randevular = randevular.OrderByDescending(r => r.RandevuTarihi);
                    break;
            }

            return View(await randevular.ToListAsync());
        }

        // GET: Randevu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Hasta)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null)
            {
                return NotFound();
            }

            return View(randevu);
        }

        // GET: Randevu/Create
        public IActionResult Create(int? hastaId = null)
        {
            ViewData["HastaId"] = new SelectList(_context.Hastalar, "Id", "AdSoyad", hastaId);
            ViewData["DoktorId"] = new SelectList(_context.Doktorlar.Where(d => d.Aktif), "Id", "AdSoyad");
            ViewData["DurumList"] = new SelectList(Enum.GetValues(typeof(RandevuDurumu))
                .Cast<RandevuDurumu>()
                .Select(e => new { Value = e, Text = e.ToString() }), "Value", "Text");
            
            // Eğer hastaId parametresi varsa, model'e set et
            if (hastaId.HasValue)
            {
                var randevu = new Randevu { HastaId = hastaId.Value };
                return View(randevu);
            }
            
            return View();
        }

        // POST: Randevu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HastaId,DoktorId,RandevuTarihi,DoktorAdi,Poliklinik,Rahatsizlik,Notlar,Durum")] Randevu randevu)
        {
            if (ModelState.IsValid)
            {
                randevu.CreatedDate = DateTime.UtcNow;
                // RandevuTarihi'ni UTC'ye çevir
                if (randevu.RandevuTarihi.Kind == DateTimeKind.Unspecified)
                {
                    randevu.RandevuTarihi = DateTime.SpecifyKind(randevu.RandevuTarihi, DateTimeKind.Utc);
                }
                else if (randevu.RandevuTarihi.Kind == DateTimeKind.Local)
                {
                    randevu.RandevuTarihi = randevu.RandevuTarihi.ToUniversalTime();
                }
                _context.Add(randevu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["HastaId"] = new SelectList(_context.Hastalar, "Id", "AdSoyad", randevu.HastaId);
            ViewData["DurumList"] = new SelectList(Enum.GetValues(typeof(RandevuDurumu))
                .Cast<RandevuDurumu>()
                .Select(e => new { Value = e, Text = e.ToString() }), "Value", "Text", randevu.Durum);
            return View(randevu);
        }

        // GET: Randevu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }
            ViewData["HastaId"] = new SelectList(_context.Hastalar, "Id", "AdSoyad", randevu.HastaId);
            ViewData["DurumList"] = new SelectList(Enum.GetValues(typeof(RandevuDurumu))
                .Cast<RandevuDurumu>()
                .Select(e => new { Value = e, Text = e.ToString() }), "Value", "Text", randevu.Durum);
            return View(randevu);
        }

        // POST: Randevu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HastaId,DoktorId,RandevuTarihi,DoktorAdi,Poliklinik,Rahatsizlik,Notlar,Durum,CreatedDate")] Randevu randevu)
        {
            if (id != randevu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // RandevuTarihi'ni UTC'ye çevir
                    if (randevu.RandevuTarihi.Kind == DateTimeKind.Unspecified)
                    {
                        randevu.RandevuTarihi = DateTime.SpecifyKind(randevu.RandevuTarihi, DateTimeKind.Utc);
                    }
                    else if (randevu.RandevuTarihi.Kind == DateTimeKind.Local)
                    {
                        randevu.RandevuTarihi = randevu.RandevuTarihi.ToUniversalTime();
                    }
                    // CreatedDate UTC olmalı
                    if (randevu.CreatedDate.Kind != DateTimeKind.Utc)
                    {
                        randevu.CreatedDate = randevu.CreatedDate.Kind == DateTimeKind.Local 
                            ? randevu.CreatedDate.ToUniversalTime() 
                            : DateTime.SpecifyKind(randevu.CreatedDate, DateTimeKind.Utc);
                    }
                    _context.Update(randevu);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Randevu başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RandevuExists(randevu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["HastaId"] = new SelectList(_context.Hastalar, "Id", "AdSoyad", randevu.HastaId);
            ViewData["DurumList"] = new SelectList(Enum.GetValues(typeof(RandevuDurumu))
                .Cast<RandevuDurumu>()
                .Select(e => new { Value = e, Text = e.ToString() }), "Value", "Text", randevu.Durum);
            return View(randevu);
        }

        // GET: Randevu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Hasta)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null)
            {
                return NotFound();
            }

            return View(randevu);
        }

        // POST: Randevu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.Id == id);
        }
    }
}

