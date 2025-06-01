using Microsoft.AspNetCore.Mvc;
using Kutuphane.Data;
using Kutuphane.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Kutuphane.Controllers
{
    [Authorize]
    public class KitapController : Controller
    {
        private readonly KutuphaneDbContext _context;

        public KitapController(KutuphaneDbContext context)
        {
            _context = context;
        }

        // GET: Kitap
        public async Task<IActionResult> Index()
        {
            var kitaplar = await _context.Kitaplar
                                 .Include(k => k.Kategori)
                                 .ToListAsync();
            return View(kitaplar);
        }

        // GET: Kitap/Ekle
        public async Task<IActionResult> Ekle()
        {
            ViewBag.Kategoriler = new SelectList(await _context.Kategoriler.ToListAsync(), "Id", "KategoriAdi");
            return View();
        }

        // POST: Kitap/Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Kitap kitap)
        {
            if (ModelState.IsValid)
            {
                _context.Kitaplar.Add(kitap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Kategoriler = new SelectList(await _context.Kategoriler.ToListAsync(), "Id", "KategoriAdi");
            return View(kitap);
        }

        // GET: Kitap/Guncelle/5
        public async Task<IActionResult> Guncelle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kitap = await _context.Kitaplar.FindAsync(id);
            if (kitap == null)
            {
                return NotFound();
            }

            ViewBag.Kategoriler = new SelectList(await _context.Kategoriler.Where(k => k.AktifMi).ToListAsync(), "Id", "KategoriAdi");
            return View(kitap);
        }

        // POST: Kitap/Guncelle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guncelle(int id, Kitap kitap)
        {
            if (id != kitap.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    kitap.AktifMi = true;
                    _context.Update(kitap);
                    await _context.SaveChangesAsync();
                    TempData["Basarili"] = "Kitap başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KitapExists(kitap.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewBag.Kategoriler = new SelectList(await _context.Kategoriler.Where(k => k.AktifMi).ToListAsync(), "Id", "KategoriAdi");
            return View(kitap);
        }

        // GET: Kitap/Sil/5
        public async Task<IActionResult> Sil(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kitap = await _context.Kitaplar
                .Include(k => k.Kategori)
                .Include(k => k.OduncKitaplar)
                .FirstOrDefaultAsync(m => m.Id == id && m.AktifMi);
            
            if (kitap == null)
            {
                return NotFound();
            }

            return View(kitap);
        }

        // POST: Kitap/Sil/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kitap = await _context.Kitaplar
                .Include(k => k.OduncKitaplar)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (kitap == null)
            {
                return NotFound();
            }

            // Kitabın teslim edilmemiş ödünç kayıtları var mı kontrol et
            var teslimEdilmemisKayit = kitap.OduncKitaplar.Any(o => !o.TeslimDurumu);
            if (teslimEdilmemisKayit)
            {
                TempData["Hata"] = "Bu kitabın teslim edilmemiş ödünç kayıtları var. Önce kitapları teslim alın.";
                return RedirectToAction(nameof(Index));
            }

            // Kitabı silmek yerine aktif durumunu false yap
            kitap.AktifMi = false;
            await _context.SaveChangesAsync();
            TempData["Basarili"] = "Kitap başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        private bool KitapExists(int id)
        {
            return _context.Kitaplar.Any(e => e.Id == id);
        }
    }
}