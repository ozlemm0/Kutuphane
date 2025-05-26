using Kutuphane.Data;
using Kutuphane.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Kutuphane.Controllers
{
    public class RaporController : Controller
    {
        private readonly KutuphaneDbContext _context;

        public RaporController(KutuphaneDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? secilenOgrenciId)
        {
            var enCokOkuyanOgrenciler = _context.OduncKitaplar
                .Include(o => o.Ogrenci)
                .Where(o => o.OgrenciId != null && o.Ogrenci != null)
                .GroupBy(o => o.Ogrenci)
                .Select(g => new OgrenciOkumaViewModel
                {
                    OgrenciId = g.Key.Id,
                    AdSoyad = g.Key.OgrenciAdi + " " + g.Key.OgrenciSoyadi,
                    OkumaSayisi = g.Count()
                })
                .OrderByDescending(x => x.OkumaSayisi)
                .ToList();

            var enCokOkunanKitaplar = _context.OduncKitaplar
                .Include(o => o.Kitap)
                .Where(o => o.Kitap != null)
                .GroupBy(o => o.Kitap)
                .Select(g => new KitapOkunmaViewModel
                {
                    KitapId = g.Key.Id,
                    KitapAdi = g.Key.KitapAdi,
                    Yazar = g.Key.Yazar,
                    OkunmaSayisi = g.Count()
                })
                .OrderByDescending(x => x.OkunmaSayisi)
                .ToList();

            var oduncVerilenKitaplar = _context.OduncKitaplar
                .Include(o => o.Kitap)
                .Include(o => o.Ogrenci)
                .Where(o => o.TeslimDurumu == false && o.OgrenciId != null && o.Kitap != null && o.Ogrenci != null)
                .OrderByDescending(o => o.OduncAlmaTarihi)
                .Select(o => new OduncKitapViewModel
                {
                    Id = o.Id,
                    KitapAdi = o.Kitap.KitapAdi,
                    OgrenciAdSoyad = o.Ogrenci.OgrenciAdi + " " + o.Ogrenci.OgrenciSoyadi,
                    OduncAlmaTarihi = o.OduncAlmaTarihi,
                    TeslimTarihi = o.TeslimTarihi,
                    TeslimDurumu = o.TeslimDurumu
                })
                .ToList();

            var secilenOgrencininKitaplari = new List<OduncKitapViewModel>();
            string secilenOgrenciAdSoyad = string.Empty;

            if (secilenOgrenciId.HasValue)
            {
                secilenOgrencininKitaplari = _context.OduncKitaplar
                    .Include(o => o.Kitap)
                    .Include(o => o.Ogrenci)
                    .Where(o => o.OgrenciId == secilenOgrenciId.Value && o.Kitap != null && o.Ogrenci != null)
                    .Select(o => new OduncKitapViewModel
                    {
                        Id = o.Id,
                        KitapAdi = o.Kitap.KitapAdi,
                        OgrenciAdSoyad = o.Ogrenci.OgrenciAdi + " " + o.Ogrenci.OgrenciSoyadi,
                        OduncAlmaTarihi = o.OduncAlmaTarihi,
                        TeslimTarihi = o.TeslimTarihi,
                        TeslimDurumu = o.TeslimDurumu
                    })
                    .ToList();

                var ogrenci = _context.Ogrenciler.FirstOrDefault(x => x.Id == secilenOgrenciId.Value);
                if (ogrenci != null)
                {
                    secilenOgrenciAdSoyad = ogrenci.OgrenciAdi + " " + ogrenci.OgrenciSoyadi;
                }
            }

            var model = new RaporViewModel
            {
                EnCokOkuyanOgrenciler = enCokOkuyanOgrenciler,
                EnCokOkunanKitaplar = enCokOkunanKitaplar,
                OduncVerilenKitaplar = oduncVerilenKitaplar,
                SecilenOgrencininKitaplari = secilenOgrencininKitaplari,
                SecilenOgrenciId = secilenOgrenciId,
                SecilenOgrenciAdSoyad = secilenOgrenciAdSoyad
            };

            return View(model);
        }
    }
}
