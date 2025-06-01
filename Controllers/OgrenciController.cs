using Microsoft.AspNetCore.Mvc;
using Kutuphane.Data; // Adjust namespace based on your project structure
using Kutuphane.Models; // Adjust namespace based on your models
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Kutuphane.Controllers
{
    [Authorize]
    public class OgrenciController : Controller
    {
        private readonly KutuphaneDbContext _context;

        public OgrenciController(KutuphaneDbContext context)
        {
            _context = context;
        }

        // GET: Ogrenci
        public async Task<IActionResult> Index()
        {
            var ogrenciler = await _context.Ogrenciler
                .Include(o => o.Sinif)
                .Where(o => o.AktifMi)
                .OrderBy(o => o.OgrenciAdi)
                .ThenBy(o => o.OgrenciSoyadi)
                .ToListAsync();
            return View(ogrenciler);
        }

        // GET: Ogrenci/Create
        public async Task<IActionResult> Ekle()
        {
            ViewBag.Siniflar = new SelectList(await _context.Siniflar.Where(s => s.AktifMi).ToListAsync(), "Id", "SinifAdi");
            return View();
        }

        // POST: Ogrenci/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Ogrenci ogrenci)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ogrenci.AktifMi = true;
                    ogrenci.EklenmeTarihi = DateTime.Now;
                    _context.Ogrenciler.Add(ogrenci);
                    await _context.SaveChangesAsync();
                    TempData["Basarili"] = "Öğrenci başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Öğrenci eklenirken bir hata oluştu: " + ex.Message);
            }

            ViewBag.Siniflar = new SelectList(await _context.Siniflar.Where(s => s.AktifMi).ToListAsync(), "Id", "SinifAdi");
            return View(ogrenci);
        }

        // GET: Ogrenci/Edit/5
        public async Task<IActionResult> Guncelle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogrenci = await _context.Ogrenciler.FindAsync(id);
            if (ogrenci == null || !ogrenci.AktifMi)
            {
                return NotFound();
            }

            ViewBag.Siniflar = new SelectList(await _context.Siniflar.Where(s => s.AktifMi).ToListAsync(), "Id", "SinifAdi");
            return View(ogrenci);
        }

        // POST: Ogrenci/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guncelle(int id, Ogrenci ogrenci)
        {
            if (id != ogrenci.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ogrenci.AktifMi = true;
                    _context.Update(ogrenci);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OgrenciExists(ogrenci.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewBag.Siniflar = new SelectList(await _context.Siniflar.Where(s => s.AktifMi).ToListAsync(), "Id", "SinifAdi");
            return View(ogrenci);
        }

        // GET: Ogrenci/Delete/5
        public async Task<IActionResult> Sil(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogrenci = await _context.Ogrenciler
                .Include(o => o.Sinif)
                .Include(o => o.OduncKitaplar)
                .FirstOrDefaultAsync(m => m.Id == id && m.AktifMi);
            
            if (ogrenci == null)
            {
                return NotFound();
            }

            return View(ogrenci);
        }

        // POST: Ogrenci/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ogrenci = await _context.Ogrenciler
                .Include(o => o.OduncKitaplar)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ogrenci == null)
            {
                return NotFound();
            }

            // Öğrencinin teslim edilmemiş kitapları var mı kontrol et
            var teslimEdilmemisKitap = ogrenci.OduncKitaplar.Any(o => !o.TeslimDurumu);
            if (teslimEdilmemisKitap)
            {
                TempData["Hata"] = "Bu öğrencinin teslim etmediği kitaplar var. Önce kitapları teslim alın.";
                return RedirectToAction(nameof(Index));
            }

            // Öğrenciyi silmek yerine aktif durumunu false yap
            ogrenci.AktifMi = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OgrenciExists(int id)
        {
            return _context.Ogrenciler.Any(e => e.Id == id);
        }
    }
}