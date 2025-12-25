using Microsoft.AspNetCore.Mvc;
using HastaRandevuTakip.Models;

namespace HastaRandevuTakip.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // İstatistikler
            var toplamHasta = _context.Hastalar.Count();
            var toplamRandevu = _context.Randevular.Count();
            var bugunRandevu = _context.Randevular
                .Where(r => r.RandevuTarihi.Date == DateTime.Today)
                .Count();
            var bekleyenRandevu = _context.Randevular
                .Where(r => r.Durum == RandevuDurumu.Bekliyor)
                .Count();

            ViewBag.ToplamHasta = toplamHasta;
            ViewBag.ToplamRandevu = toplamRandevu;
            ViewBag.BugunRandevu = bugunRandevu;
            ViewBag.BekleyenRandevu = bekleyenRandevu;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}


