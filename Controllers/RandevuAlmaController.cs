using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HastaRandevuTakip.Models;

namespace HastaRandevuTakip.Controllers
{
    public class RandevuAlmaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RandevuAlmaController> _logger;

        public RandevuAlmaController(ApplicationDbContext context, ILogger<RandevuAlmaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: RandevuAlma - Ana randevu alma sayfası
        public IActionResult Index()
        {
            try
            {
                var doktorlar = _context.Doktorlar
                    .Where(d => d.Aktif == true)
                    .OrderBy(d => d.UzmanlikAlani)
                    .ToList();

                return View(doktorlar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doktorlar listelenirken hata oluştu: {Message}", ex.Message);
                // Hata durumunda boş liste döndür
                return View(new List<Doktor>());
            }
        }

        // GET: RandevuAlma/RandevuFormu
        public IActionResult RandevuFormu(int? doktorId)
        {
            if (doktorId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var doktor = _context.Doktorlar.Find(doktorId);
            if (doktor == null || !doktor.Aktif)
            {
                TempData["ErrorMessage"] = "Seçilen doktor bulunamadı veya aktif değil.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Doktor = doktor;
            ViewBag.DoktorId = doktorId;
            
            return View();
        }

        // POST: RandevuAlma/RandevuFormu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuFormu([Bind("DoktorId,HastaId,RandevuTarihi,Rahatsizlik,Notlar")] Randevu randevu, 
            [Bind("Ad,Soyad,TCKimlikNo,Telefon,Email,DogumTarihi,Adres")] Hasta hasta)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // TC Kimlik No kontrolü
                    var mevcutHasta = await _context.Hastalar
                        .FirstOrDefaultAsync(h => h.TCKimlikNo == hasta.TCKimlikNo);

                    if (mevcutHasta != null)
                    {
                        // Mevcut hasta varsa onu kullan
                        randevu.HastaId = mevcutHasta.Id;
                    }
                    else
                    {
                        // Yeni hasta oluştur
                        hasta.CreatedDate = DateTime.Now;
                        _context.Hastalar.Add(hasta);
                        await _context.SaveChangesAsync();
                        randevu.HastaId = hasta.Id;
                    }

                    // Randevu bilgilerini tamamla
                    var doktor = await _context.Doktorlar.FindAsync(randevu.DoktorId);
                    if (doktor != null)
                    {
                        randevu.DoktorAdi = doktor.AdSoyad;
                        randevu.Poliklinik = doktor.UzmanlikAlani;
                    }

                    randevu.Durum = RandevuDurumu.Bekliyor;
                    randevu.CreatedDate = DateTime.Now;

                    _context.Randevular.Add(randevu);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Randevunuz başarıyla oluşturuldu! Randevu numaranız: " + randevu.Id;
                    return RedirectToAction(nameof(RandevuOnay), new { id = randevu.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Randevu oluşturulurken hata oluştu");
                    ModelState.AddModelError("", "Randevu oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            // Hata durumunda doktor bilgisini tekrar yükle
            var doktorReload = await _context.Doktorlar.FindAsync(randevu.DoktorId);
            if (doktorReload != null)
            {
                ViewBag.Doktor = doktorReload;
                ViewBag.DoktorId = randevu.DoktorId;
            }

            return View(randevu);
        }

        // GET: RandevuAlma/RandevuOnay
        public async Task<IActionResult> RandevuOnay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Doktor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null)
            {
                return NotFound();
            }

            return View(randevu);
        }

        // GET: RandevuAlma/DoktorDetay
        public async Task<IActionResult> DoktorDetay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doktor = await _context.Doktorlar.FindAsync(id);
            if (doktor == null || !doktor.Aktif)
            {
                return NotFound();
            }

            return View(doktor);
        }

        // GET: RandevuAlma/Randevularim - Randevu sorgulama sayfası
        public async Task<IActionResult> Randevularim(string? tcKimlikNo)
        {
            // Eğer TC Kimlik No query string'den geliyorsa (iptal sonrası veya GET isteği)
            if (!string.IsNullOrEmpty(tcKimlikNo))
            {
                var hasta = await _context.Hastalar
                    .FirstOrDefaultAsync(h => h.TCKimlikNo == tcKimlikNo);

                if (hasta != null)
                {
                    var randevular = await _context.Randevular
                        .Include(r => r.Hasta)
                        .Include(r => r.Doktor)
                        .Where(r => r.HastaId == hasta.Id)
                        .OrderByDescending(r => r.RandevuTarihi)
                        .ToListAsync();

                    ViewBag.Hasta = hasta;
                    ViewBag.TCKimlikNo = tcKimlikNo;
                    return View(randevular);
                }
                else
                {
                    TempData["ErrorMessage"] = "Bu TC Kimlik No ile kayıtlı hasta bulunamadı.";
                }
            }

            return View();
        }

        // POST: RandevuAlma/Randevularim - TC Kimlik No ile randevu sorgulama
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Randevularim")]
        public async Task<IActionResult> RandevularimPost(string tcKimlikNo)
        {
            if (string.IsNullOrEmpty(tcKimlikNo))
            {
                ModelState.AddModelError("tcKimlikNo", "TC Kimlik No giriniz.");
                return View();
            }

            // TC Kimlik No ile hasta bul
            var hasta = await _context.Hastalar
                .FirstOrDefaultAsync(h => h.TCKimlikNo == tcKimlikNo);

            if (hasta == null)
            {
                TempData["ErrorMessage"] = "Bu TC Kimlik No ile kayıtlı hasta bulunamadı.";
                return View();
            }

            // Hastanın randevularını getir
            var randevular = await _context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Doktor)
                .Where(r => r.HastaId == hasta.Id)
                .OrderByDescending(r => r.RandevuTarihi)
                .ToListAsync();

            ViewBag.Hasta = hasta;
            ViewBag.TCKimlikNo = tcKimlikNo;
            return View(randevular);
        }

        // POST: RandevuAlma/RandevuIptal - Randevu iptal etme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuIptal(int id, string tcKimlikNo)
        {
            var randevu = await _context.Randevular
                .Include(r => r.Hasta)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (randevu == null)
            {
                TempData["ErrorMessage"] = "Randevu bulunamadı.";
                return RedirectToAction(nameof(Randevularim));
            }

            // TC Kimlik No kontrolü
            if (randevu.Hasta?.TCKimlikNo != tcKimlikNo)
            {
                TempData["ErrorMessage"] = "Bu randevuyu iptal etme yetkiniz yok.";
                return RedirectToAction(nameof(Randevularim));
            }

            // Sadece bekleyen veya onaylanmış randevular iptal edilebilir
            if (randevu.Durum != RandevuDurumu.Bekliyor && randevu.Durum != RandevuDurumu.Onaylandi)
            {
                TempData["ErrorMessage"] = "Bu randevu iptal edilemez. (Zaten iptal edilmiş veya tamamlanmış)";
                return RedirectToAction(nameof(Randevularim));
            }

            randevu.Durum = RandevuDurumu.IptalEdildi;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Randevunuz başarıyla iptal edildi.";
            return RedirectToAction(nameof(Randevularim), new { tcKimlikNo = tcKimlikNo });
        }
    }
}

