using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kutuphane.Models;
using Kutuphane.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Kutuphane.Controllers;
[Authorize]
public class SinifController : Controller
{
    private readonly KutuphaneDbContext _context;

    public SinifController(KutuphaneDbContext context)
    {
        _context = context;
    }

    // GET: Sinif
    public async Task<IActionResult> Index()
    {
        var siniflar = await _context.Siniflar
            .Where(s => s.AktifMi)
            .OrderBy(s => s.SinifAdi)
            .ToListAsync();
        return View(siniflar);
    }

    // GET: Sinif/Create
    public IActionResult Ekle()
    {
        return View();
    }

    // POST: Sinif/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(Sinif sinif)
    {
        if (ModelState.IsValid)
        {
            sinif.AktifMi = true;
            await _context.Siniflar.AddAsync(sinif);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(sinif);
    }

    // GET: Sinif/Edit/5
    public async Task<IActionResult> Guncelle(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        var sinif = await _context.Siniflar.FindAsync(id);
        if (sinif == null || !sinif.AktifMi)
        {
            return NotFound();
        }
        return View(sinif);
    }

    // POST: Sinif/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guncelle(Sinif sinif)
    {
        if (ModelState.IsValid)
        {
            try
            {
                sinif.AktifMi = true;
                _context.Siniflar.Update(sinif);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SinifExists(sinif.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        return View(sinif);
    }

    // GET: Sinif/Delete/5
    public async Task<IActionResult> Sil(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        var sinif = await _context.Siniflar
            .Include(s => s.Ogrenciler)
            .FirstOrDefaultAsync(s => s.Id == id && s.AktifMi);

        if (sinif == null)
        {
            return NotFound();
        }

        return View(sinif);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sinif = await _context.Siniflar
            .Include(s => s.Ogrenciler)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sinif == null)
        {
            return NotFound();
        }

        // Sınıfta aktif öğrenciler var mı kontrol et
        var aktifOgrenciVarMi = sinif.Ogrenciler.Any(o => o.AktifMi);
        if (aktifOgrenciVarMi)
        {
            TempData["Hata"] = "Bu sınıfta aktif öğrenciler var. Önce öğrencileri başka bir sınıfa taşıyın.";
            return RedirectToAction(nameof(Index));
        }

        // Sınıfı silmek yerine aktif durumunu false yap
        sinif.AktifMi = false;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SinifExists(int id)
    {
        return _context.Siniflar.Any(e => e.Id == id);
    }
}