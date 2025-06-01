using Microsoft.AspNetCore.Mvc;
using Kutuphane.Data;
using Kutuphane.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Kutuphane.Controllers
{
    [Authorize]
    public class OduncKitapController : Controller
    {
        private readonly KutuphaneDbContext _context;

        public OduncKitapController(KutuphaneDbContext context)
        {
            _context = context;
        }

        // GET: OduncKitap
        public async Task<IActionResult> Index()
        {
            var oduncKitaplar = await _context.OduncKitaplar
                .Include(o => o.Kitap)
                .Include(o => o.Ogrenci)
                .OrderByDescending(o => o.OduncAlmaTarihi)
                .ToListAsync();
            return View(oduncKitaplar);
        }

        // GET: OduncKitap/Ekle
        public async Task<IActionResult> Ekle()
        {
            // Sadece aktif öğrencileri getir
            ViewBag.Ogrenciler = new SelectList(
                await _context.Ogrenciler
                    .Where(o => o.AktifMi)
                    .OrderBy(o => o.OgrenciAdi)
                    .ThenBy(o => o.OgrenciSoyadi)
                    .ToListAsync(),
                "Id",
                "TamAd"
            );

            // Sadece aktif ve ödünç verilmemiş kitapları getir
            ViewBag.Kitaplar = new SelectList(
                await _context.Kitaplar
                    .Where(k => k.AktifMi && !k.OduncKitaplar.Any(o => !o.TeslimDurumu))
                    .OrderBy(k => k.KitapAdi)
                    .ToListAsync(),
                "Id",
                "KitapAdi"
            );

            return View();
        }

        // POST: OduncKitap/Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(OduncKitap oduncKitap)
        {
            if (ModelState.IsValid)
            {
                oduncKitap.OduncAlmaTarihi = DateTime.Now;
                oduncKitap.TeslimDurumu = false;
                _context.OduncKitaplar.Add(oduncKitap);
                await _context.SaveChangesAsync();
                TempData["Basarili"] = "Kitap başarıyla ödünç verildi.";
                return RedirectToAction(nameof(Index));
            }

            // Sadece aktif öğrencileri getir
            ViewBag.Ogrenciler = new SelectList(
                await _context.Ogrenciler
                    .Where(o => o.AktifMi)
                    .OrderBy(o => o.OgrenciAdi)
                    .ThenBy(o => o.OgrenciSoyadi)
                    .ToListAsync(),
                "Id",
                "TamAd"
            );

            // Sadece aktif ve ödünç verilmemiş kitapları getir
            ViewBag.Kitaplar = new SelectList(
                await _context.Kitaplar
                    .Where(k => k.AktifMi && !k.OduncKitaplar.Any(o => !o.TeslimDurumu))
                    .OrderBy(k => k.KitapAdi)
                    .ToListAsync(),
                "Id",
                "KitapAdi"
            );

            return View(oduncKitap);
        }

        // GET: OduncKitap/Guncelle/5
        public async Task<IActionResult> Guncelle(int? id)
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

            // Sadece aktif öğrencileri getir
            ViewBag.Ogrenciler = new SelectList(
                await _context.Ogrenciler
                    .Where(o => o.AktifMi)
                    .OrderBy(o => o.OgrenciAdi)
                    .ThenBy(o => o.OgrenciSoyadi)
                    .ToListAsync(),
                "Id",
                "TamAd",
                oduncKitap.OgrenciId
            );

            // Sadece aktif ve ödünç verilmemiş kitapları getir
            ViewBag.Kitaplar = new SelectList(
                await _context.Kitaplar
                    .Where(k => k.AktifMi && (!k.OduncKitaplar.Any(o => !o.TeslimDurumu) || k.Id == oduncKitap.KitapId))
                    .OrderBy(k => k.KitapAdi)
                    .ToListAsync(),
                "Id",
                "KitapAdi",
                oduncKitap.KitapId
            );

            return View(oduncKitap);
        }

        // POST: OduncKitap/Guncelle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guncelle(int id, OduncKitap oduncKitap)
        {
            if (id != oduncKitap.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(oduncKitap);
                    await _context.SaveChangesAsync();
                    TempData["Basarili"] = "Ödünç kitap kaydı başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OduncKitapExists(oduncKitap.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Sadece aktif öğrencileri getir
            ViewBag.Ogrenciler = new SelectList(
                await _context.Ogrenciler
                    .Where(o => o.AktifMi)
                    .OrderBy(o => o.OgrenciAdi)
                    .ThenBy(o => o.OgrenciSoyadi)
                    .ToListAsync(),
                "Id",
                "TamAd",
                oduncKitap.OgrenciId
            );

            // Sadece aktif ve ödünç verilmemiş kitapları getir
            ViewBag.Kitaplar = new SelectList(
                await _context.Kitaplar
                    .Where(k => k.AktifMi && (!k.OduncKitaplar.Any(o => !o.TeslimDurumu) || k.Id == oduncKitap.KitapId))
                    .OrderBy(k => k.KitapAdi)
                    .ToListAsync(),
                "Id",
                "KitapAdi",
                oduncKitap.KitapId
            );

            return View(oduncKitap);
        }

        // GET: OduncKitap/TeslimAl/5
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

        // POST: OduncKitap/TeslimAl/5
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
            await _context.SaveChangesAsync();
            TempData["Basarili"] = "Kitap başarıyla teslim alındı.";
            return RedirectToAction(nameof(Index));
        }

        private bool OduncKitapExists(int id)
        {
            return _context.OduncKitaplar.Any(e => e.Id == id);
        }
    }
} 