using Microsoft.AspNetCore.Mvc;
using Kutuphane.Data;
using Kutuphane.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace Kutuphane.Controllers
{
    [Authorize]
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
                    .ThenInclude(o => o.Sinif)
                .OrderByDescending(o => o.OduncAlmaTarihi)
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
                _logger.LogInformation("Ver metodu başladı. Gelen veri: {@OduncKitap}", oduncKitap);

                if (oduncKitap == null)
                {
                    _logger.LogWarning("OduncKitap nesnesi null");
                    return Json(new { success = false, message = "Geçersiz veri gönderildi." });
                }

                // ModelState kontrolü
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    _logger.LogWarning("ModelState geçersiz. Hatalar: {@Errors}", errors);
                    return Json(new { success = false, message = "Geçersiz veri: " + string.Join(", ", errors) });
                }

                // Kitabın aktif olup olmadığını kontrol et
                var kitap = await _context.Kitaplar.FindAsync(oduncKitap.KitapId);
                if (kitap == null)
                {
                    _logger.LogWarning("Kitap bulunamadı. KitapId: {KitapId}", oduncKitap.KitapId);
                    return Json(new { success = false, message = "Seçilen kitap sistemde bulunmamaktadır." });
                }

                if (!kitap.AktifMi)
                {
                    _logger.LogWarning("Kitap aktif değil. KitapId: {KitapId}", oduncKitap.KitapId);
                    return Json(new { success = false, message = "Seçilen kitap aktif değil." });
                }

                // Öğrencinin aktif olup olmadığını kontrol et
                var ogrenci = await _context.Ogrenciler.FindAsync(oduncKitap.OgrenciId);
                if (ogrenci == null)
                {
                    _logger.LogWarning("Öğrenci bulunamadı. OgrenciId: {OgrenciId}", oduncKitap.OgrenciId);
                    return Json(new { success = false, message = "Seçilen öğrenci sistemde bulunmamaktadır." });
                }

                if (!ogrenci.AktifMi)
                {
                    _logger.LogWarning("Öğrenci aktif değil. OgrenciId: {OgrenciId}", oduncKitap.OgrenciId);
                    return Json(new { success = false, message = "Seçilen öğrenci aktif değil." });
                }

                // Öğrencinin aktif ödünç kaydı var mı kontrol et
                var ogrenciOduncDurumu = await _context.OduncKitaplar
                    .AnyAsync(o => o.OgrenciId == oduncKitap.OgrenciId && !o.TeslimDurumu);

                if (ogrenciOduncDurumu)
                {
                    _logger.LogWarning("Öğrencinin elinde kitap var. OgrenciId: {OgrenciId}", oduncKitap.OgrenciId);
                    return Json(new { success = false, message = "Bu öğrencinin elinde henüz iade edilmemiş bir kitap bulunmaktadır. Yeni kitap ödünç alabilmesi için mevcut kitabı iade etmesi gerekmektedir." });
                }

                // Kitabın aktif ödünç kaydı var mı kontrol et
                var kitapOduncDurumu = await _context.OduncKitaplar
                    .AnyAsync(o => o.KitapId == oduncKitap.KitapId && !o.TeslimDurumu);

                if (kitapOduncDurumu)
                {
                    _logger.LogWarning("Kitap zaten ödünç verilmiş. KitapId: {KitapId}", oduncKitap.KitapId);
                    return Json(new { success = false, message = "Bu kitap zaten ödünç verilmiş." });
                }

                try
                {
                    // Kitabın ödünç verildi durumunu güncelle
                    kitap.OduncVerildiMi = true;
                    _context.Kitaplar.Update(kitap);

                    // Ödünç kaydını oluştur
                    oduncKitap.OduncAlmaTarihi = DateTime.Now;
                    oduncKitap.TeslimDurumu = false;
                    _context.OduncKitaplar.Add(oduncKitap);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Kitap başarıyla ödünç verildi. KitapId: {KitapId}, OgrenciId: {OgrenciId}", 
                        oduncKitap.KitapId, oduncKitap.OgrenciId);

                    return Json(new { success = true });
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Veritabanı güncelleme hatası. KitapId: {KitapId}, OgrenciId: {OgrenciId}, Hata: {Hata}", 
                        oduncKitap.KitapId, oduncKitap.OgrenciId, dbEx.InnerException?.Message);
                    return Json(new { success = false, message = "Veritabanı işlemi sırasında bir hata oluştu: " + dbEx.InnerException?.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap ödünç verme işleminde hata oluştu. KitapId: {KitapId}, OgrenciId: {OgrenciId}, Hata: {Hata}", 
                    oduncKitap?.KitapId, oduncKitap?.OgrenciId, ex.Message);
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
                    .ThenInclude(o => o.Sinif)
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
        public async Task<IActionResult> TeslimAlConfirmed(int id)
        {
            var oduncKitap = await _context.OduncKitaplar.FindAsync(id);
            if (oduncKitap == null)
            {
                return NotFound();
            }

            oduncKitap.TeslimDurumu = true;
            oduncKitap.TeslimTarihi = DateTime.Now;
            _context.Update(oduncKitap);
            await _context.SaveChangesAsync();

            TempData["Basarili"] = "Kitap başarıyla teslim alındı.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Kitap arama
        public async Task<IActionResult> KitapAra(string arama)
        {
            var kitaplar = await _context.Kitaplar
                .Where(k => k.KitapAdi.Contains(arama) && k.AktifMi)
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
                .Where(o => (o.OgrenciAdi.Contains(arama) || o.OgrenciSoyadi.Contains(arama)) && o.AktifMi)
                .Select(o => new
                {
                    id = o.Id,
                    adSoyad = o.OgrenciAdi + " " + o.OgrenciSoyadi,
                    sinif = o.Sinif.SinifAdi,
                    numara = o.OkulNumarasi
                })
                .OrderBy(o => o.adSoyad)
                .Take(10)
                .ToListAsync();

            return Json(ogrenciler);
        }

        // POST: Odunc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var oduncKitap = await _context.OduncKitaplar.FindAsync(id);
            if (oduncKitap == null)
            {
                return NotFound();
            }

            // Ödünç kaydını silmek yerine teslim durumunu güncelle
            oduncKitap.TeslimDurumu = true;
            oduncKitap.TeslimTarihi = DateTime.Now;
            _context.Update(oduncKitap);
            await _context.SaveChangesAsync();

            TempData["Basarili"] = "Ödünç kaydı başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
} 