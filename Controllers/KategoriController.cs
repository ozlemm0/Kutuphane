using Microsoft.AspNetCore.Mvc;
using Kutuphane.Data;
using Kutuphane.Models;
using Microsoft.EntityFrameworkCore;

namespace Kutuphane.Controllers
{
    public class KategoriController : Controller
    {
        private readonly KutuphaneDbContext _context;

        public KategoriController(KutuphaneDbContext context)
        {
            _context = context;
        }

        // GET: Kategori
        public async Task<IActionResult> Index()
        {
            return View(await _context.Kategoriler.ToListAsync());
        }

        // GET: Kategori/Ekle
        public IActionResult Ekle()
        {
            return View();
        }

        // POST: Kategori/Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Kategori kategori)
        {
            if (ModelState.IsValid)
            {
                _context.Kategoriler.Add(kategori);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kategori);
        }

        // GET: Kategori/Guncelle/5
        public async Task<IActionResult> Guncelle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategori = await _context.Kategoriler.FindAsync(id);
            if (kategori == null)
            {
                return NotFound();
            }
            return View(kategori);
        }

        // POST: Kategori/Guncelle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guncelle(int id, Kategori kategori)
        {
            if (id != kategori.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kategori);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KategoriExists(kategori.Id))
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
            return View(kategori);
        }

        // GET: Kategori/Sil/5
        public async Task<IActionResult> Sil(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategori = await _context.Kategoriler
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kategori == null)
            {
                return NotFound();
            }

            return View(kategori);
        }

        // POST: Kategori/Sil/5
        [HttpPost, ActionName("Sil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SilOnayla(int id)
        {
            var kategori = await _context.Kategoriler.FindAsync(id);
            if (kategori != null)
            {
                _context.Kategoriler.Remove(kategori);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool KategoriExists(int id)
        {
            return _context.Kategoriler.Any(e => e.Id == id);
        }
    }
} 