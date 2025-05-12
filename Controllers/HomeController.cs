using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kutuphane.Models;
using Kutuphane.Data;
using Microsoft.EntityFrameworkCore;

namespace Kutuphane.Controllers;
public class HomeController : Controller
{
    private readonly KutuphaneDbContext _context;

    public HomeController(KutuphaneDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index()
    {
        // 7 gün içinde teslim edilmeyen kitaplar
        var teslimEdilmeyenKitaplar = await _context.OduncKitaplar
            .Include(o => o.Kitap)
            .Include(o => o.Ogrenci)
            .Where(o =>
                o.OduncAlmaTarihi.AddDays(7) < DateTime.Now && // 7 gün geçti
                o.TeslimDurumu == false)                       // hâlâ teslim edilmedi
            .Select(o => new KitapOduncİslemleri
            {
                OgrenciAdi = o.Ogrenci != null
                    ? o.Ogrenci.OgrenciAdi + " " + o.Ogrenci.OgrenciSoyadi
                    : "Bilinmiyor",

                KitapAdi = o.Kitap != null
                    ? o.Kitap.KitapAdi
                    : "Bilinmiyor",

                AlinmaTarihi = o.OduncAlmaTarihi,

                TeslimTarihi = o.OduncAlmaTarihi.AddDays(7), // Planlanan teslim süresi

                GercekTeslimTarihi = o.TeslimDurumu
                    ? o.TeslimTarihi
                    : null,

                OduncKitap = o
            })
            .ToListAsync();

        return View(teslimEdilmeyenKitaplar);
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}