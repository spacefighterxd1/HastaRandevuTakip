using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HastaRandevuTakip.Models;

namespace HastaRandevuTakip.Controllers
{
    public class HastaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HastaController> _logger;

        public HastaController(ApplicationDbContext context, ILogger<HastaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Hasta
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.AdSortParm = sortOrder == "Ad" ? "Ad_desc" : "Ad";
            ViewBag.SoyadSortParm = sortOrder == "Soyad" ? "Soyad_desc" : "Soyad";

            var hastalar = _context.Hastalar.AsQueryable();

            // Arama
            if (!string.IsNullOrEmpty(searchString))
            {
                hastalar = hastalar.Where(h =>
                    h.Ad.Contains(searchString) ||
                    h.Soyad.Contains(searchString) ||
                    h.TCKimlikNo.Contains(searchString) ||
                    h.Telefon.Contains(searchString));
            }

            // Sıralama
            switch (sortOrder)
            {
                case "Ad":
                    hastalar = hastalar.OrderBy(h => h.Ad);
                    break;
                case "Ad_desc":
                    hastalar = hastalar.OrderByDescending(h => h.Ad);
                    break;
                case "Soyad":
                    hastalar = hastalar.OrderBy(h => h.Soyad);
                    break;
                case "Soyad_desc":
                    hastalar = hastalar.OrderByDescending(h => h.Soyad);
                    break;
                default:
                    hastalar = hastalar.OrderBy(h => h.Ad);
                    break;
            }

            return View(await hastalar.ToListAsync());
        }

        // GET: Hasta/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hasta = await _context.Hastalar
                .Include(h => h.Randevular)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hasta == null)
            {
                return NotFound();
            }

            return View(hasta);
        }

        // GET: Hasta/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Hasta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ad,Soyad,TCKimlikNo,Telefon,Email,DogumTarihi,Adres")] Hasta hasta)
        {
            if (ModelState.IsValid)
            {
                // TC Kimlik No kontrolü
                if (await _context.Hastalar.AnyAsync(h => h.TCKimlikNo == hasta.TCKimlikNo))
                {
                    ModelState.AddModelError("TCKimlikNo", "Bu TC Kimlik No ile kayıtlı hasta zaten mevcut.");
                    return View(hasta);
                }

                hasta.CreatedDate = DateTime.Now;
                _context.Add(hasta);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hasta başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(hasta);
        }

        // GET: Hasta/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hasta = await _context.Hastalar.FindAsync(id);
            if (hasta == null)
            {
                return NotFound();
            }
            return View(hasta);
        }

        // POST: Hasta/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Soyad,TCKimlikNo,Telefon,Email,DogumTarihi,Adres,CreatedDate")] Hasta hasta)
        {
            if (id != hasta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // TC Kimlik No kontrolü (kendisi hariç)
                    if (await _context.Hastalar.AnyAsync(h => h.TCKimlikNo == hasta.TCKimlikNo && h.Id != hasta.Id))
                    {
                        ModelState.AddModelError("TCKimlikNo", "Bu TC Kimlik No ile kayıtlı başka bir hasta mevcut.");
                        return View(hasta);
                    }

                    _context.Update(hasta);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Hasta bilgileri başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HastaExists(hasta.Id))
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
            return View(hasta);
        }

        // GET: Hasta/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hasta = await _context.Hastalar
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hasta == null)
            {
                return NotFound();
            }

            // Randevu kontrolü
            var randevuSayisi = await _context.Randevular.CountAsync(r => r.HastaId == id);
            ViewBag.RandevuSayisi = randevuSayisi;

            return View(hasta);
        }

        // POST: Hasta/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasta = await _context.Hastalar.FindAsync(id);
            if (hasta != null)
            {
                // Önce randevuları kontrol et
                var randevular = await _context.Randevular.Where(r => r.HastaId == id).ToListAsync();
                if (randevular.Any())
                {
                    TempData["ErrorMessage"] = "Bu hastaya ait randevular bulunduğu için hasta silinemez. Önce randevuları siliniz.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Hastalar.Remove(hasta);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hasta başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool HastaExists(int id)
        {
            return _context.Hastalar.Any(e => e.Id == id);
        }
    }
}


