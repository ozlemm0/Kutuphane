using Microsoft.AspNetCore.Mvc;
using Kutuphane.Data;
using Kutuphane.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace Kutuphane.Controllers
{
    public class OduncController : Controller
    {
        private readonly KutuphaneDbContext _context;
        private readonly ILogger<OduncController> _logger;

        public OduncController(KutuphaneDbContext context, ILogger<OduncController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Odunc
        public async Task<IActionResult> Index()
        {
            var oduncKitaplar = await _context.OduncKitaplar
                .Include(o => o.Kitap)
                .Include(o => o.Ogrenci)
                .ToListAsync();
            return View(oduncKitaplar);
        }

        // GET: Odunc/Ver
        public IActionResult Ver()
        {
            return View();
        }

        // POST: Odunc/Ver
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ver([FromBody] OduncKitap oduncKitap)
        {
            try
            {
                _logger.LogInformation("Ver metodu çağrıldı. KitapId: {KitapId}, OgrenciId: {OgrenciId}", 
                    oduncKitap.KitapId, oduncKitap.OgrenciId);

                if (oduncKitap.KitapId == 0 || oduncKitap.OgrenciId == 0)
                {
                    return Json(new { success = false, message = "Lütfen kitap ve öğrenci seçiniz." });
                }

                // Kitabın ödünç verilip verilmediğini kontrol et
                var kitapOduncDurumu = await _context.OduncKitaplar
                    .AnyAsync(o => o.KitapId == oduncKitap.KitapId && !o.TeslimDurumu);

                if (kitapOduncDurumu)
                {
                    _logger.LogWarning("Kitap zaten ödünç verilmiş. KitapId: {KitapId}", oduncKitap.KitapId);
                    return Json(new { success = false, message = "Bu kitap zaten ödünç verilmiş." });
                }

                oduncKitap.OduncAlmaTarihi = DateTime.Now;
                oduncKitap.TeslimDurumu = false;

                _context.OduncKitaplar.Add(oduncKitap);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Kitap başarıyla ödünç verildi. KitapId: {KitapId}, OgrenciId: {OgrenciId}", 
                    oduncKitap.KitapId, oduncKitap.OgrenciId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap ödünç verme işleminde hata oluştu. KitapId: {KitapId}, OgrenciId: {OgrenciId}", 
                    oduncKitap.KitapId, oduncKitap.OgrenciId);
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // GET: Odunc/TeslimAl/5
        public async Task<IActionResult> TeslimAl(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oduncKitap = await _context.OduncKitaplar
                .Include(o => o.Kitap)
                .Include(o => o.Ogrenci)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (oduncKitap == null)
            {
                return NotFound();
            }

            return View(oduncKitap);
        }

        // POST: Odunc/TeslimAl/5
        [HttpPost, ActionName("TeslimAl")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeslimAlOnayla(int id)
        {
            var oduncKitap = await _context.OduncKitaplar.FindAsync(id);
            if (oduncKitap != null)
            {
                oduncKitap.TeslimDurumu = true;
                oduncKitap.TeslimTarihi = DateTime.Now;
                _context.Update(oduncKitap);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Kitap arama
        public async Task<IActionResult> KitapAra(string arama)
        {
            var kitaplar = await _context.Kitaplar
                .Where(k => k.KitapAdi.Contains(arama))
                .Select(k => new
                {
                    id = k.Id,
                    kitapAdi = k.KitapAdi,
                    oduncDurumu = _context.OduncKitaplar.Any(o => o.KitapId == k.Id && !o.TeslimDurumu)
                })
                .Take(10)
                .ToListAsync();

            return Json(kitaplar);
        }

        // AJAX: Öğrenci arama
        public async Task<IActionResult> OgrenciAra(string arama)
        {
            var ogrenciler = await _context.Ogrenciler
                .Where(o => o.OgrenciAdi.Contains(arama) || o.OgrenciSoyadi.Contains(arama))
                .Select(o => new
                {
                    id = o.Id,
                    adSoyad = o.OgrenciAdi + " " + o.OgrenciSoyadi
                })
                .Take(10)
                .ToListAsync();

            return Json(ogrenciler);
        }
    }
} 