using Kutuphane.Data;
using Kutuphane.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kutuphane.Controllers
{
    [Authorize]
    public class RaporController : Controller
    {
        private readonly KutuphaneDbContext _context;

        public RaporController(KutuphaneDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? secilenOgrenciId, int? secilenKitapId)
        {
            // Öğrenci listesini oluştur
            var ogrenciler = _context.Ogrenciler
                .OrderBy(o => o.OgrenciAdi)
                .ThenBy(o => o.OgrenciSoyadi)
                .Select(o => new
                {
                    Id = o.Id,
                    OgrenciAdiSoyadi = o.OgrenciAdi + " " + o.OgrenciSoyadi
                })
                .ToList();

            ViewBag.Ogrenciler = new SelectList(ogrenciler, "Id", "OgrenciAdiSoyadi", secilenOgrenciId);

            // Kitap listesini oluştur
            var kitaplar = _context.Kitaplar
                .OrderBy(k => k.KitapAdi)
                .Select(k => new
                {
                    Id = k.Id,
                    KitapAdi = k.KitapAdi + " - " + k.Yazar
                })
                .ToList();

            ViewBag.Kitaplar = new SelectList(kitaplar, "Id", "KitapAdi", secilenKitapId);

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

            var model = new RaporViewModel
            {
                EnCokOkuyanOgrenciler = enCokOkuyanOgrenciler,
                EnCokOkunanKitaplar = enCokOkunanKitaplar,
                SecilenOgrenciId = secilenOgrenciId,
                SecilenKitapId = secilenKitapId
            };

            return View(model);
        }

        public IActionResult EnCokOkuyanOgrenciler(int? sinifId, DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            var query = _context.OduncKitaplar
                .Include(o => o.Ogrenci)
                    .ThenInclude(o => o.Sinif)
                .Where(o => o.OgrenciId != null && o.Ogrenci != null);

            // Sınıf filtresi
            if (sinifId.HasValue)
            {
                query = query.Where(o => o.Ogrenci.SinifId == sinifId);
            }

            // Tarih filtresi
            if (baslangicTarihi.HasValue)
            {
                query = query.Where(o => o.OduncAlmaTarihi >= baslangicTarihi.Value);
            }
            if (bitisTarihi.HasValue)
            {
                query = query.Where(o => o.OduncAlmaTarihi <= bitisTarihi.Value);
            }

            var enCokOkuyanOgrenciler = query
                .GroupBy(o => o.OgrenciId)
                .Select(g => new OgrenciOkumaViewModel
                {
                    OgrenciId = g.Key.Value,
                    AdSoyad = g.First().Ogrenci.OgrenciAdi + " " + g.First().Ogrenci.OgrenciSoyadi,
                    SinifAdi = g.First().Ogrenci.Sinif != null ? g.First().Ogrenci.Sinif.SinifAdi : "Sınıf Belirtilmemiş",
                    OkumaSayisi = g.Count()
                })
                .OrderByDescending(x => x.OkumaSayisi)
                .ToList();

            ViewBag.Siniflar = new SelectList(_context.Siniflar.ToList(), "Id", "SinifAdi", sinifId);
            ViewBag.BaslangicTarihi = baslangicTarihi;
            ViewBag.BitisTarihi = bitisTarihi;

            return View(enCokOkuyanOgrenciler);
        }

        public IActionResult EnCokOkunanKitaplar(int? sinifId, DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            var query = _context.OduncKitaplar
                .Include(o => o.Kitap)
                .Include(o => o.Ogrenci)
                    .ThenInclude(o => o.Sinif)
                .Where(o => o.Kitap != null);

            // Sınıf filtresi
            if (sinifId.HasValue)
            {
                query = query.Where(o => o.Ogrenci.SinifId == sinifId);
            }

            // Tarih filtresi
            if (baslangicTarihi.HasValue)
            {
                query = query.Where(o => o.OduncAlmaTarihi >= baslangicTarihi.Value);
            }
            if (bitisTarihi.HasValue)
            {
                query = query.Where(o => o.OduncAlmaTarihi <= bitisTarihi.Value);
            }

            var enCokOkunanKitaplar = query
                .GroupBy(o => o.KitapId)
                .Select(g => new KitapOkunmaViewModel
                {
                    KitapId = g.Key,
                    KitapAdi = g.First().Kitap.KitapAdi,
                    Yazar = g.First().Kitap.Yazar,
                    OkunmaSayisi = g.Count()
                })
                .OrderByDescending(x => x.OkunmaSayisi)
                .ToList();

            ViewBag.Siniflar = new SelectList(_context.Siniflar.ToList(), "Id", "SinifAdi", sinifId);
            ViewBag.BaslangicTarihi = baslangicTarihi;
            ViewBag.BitisTarihi = bitisTarihi;

            return View(enCokOkunanKitaplar);
        }
        public IActionResult GecikenKitaplar(int? sinifId)
        {
            // Sınıfları dropdown için ViewBag ile gönder
            ViewBag.Siniflar = new SelectList(_context.Siniflar, "Id", "SinifAdi");

            // Teslim edilmemiş ödünç alınan kitapları veritabanından al, ilişkili verileri include et
            var oduncKitaplar = _context.OduncKitaplar
                .Where(o => o.TeslimDurumu == false) // teslim edilmemiş kitaplar
                .Include(o => o.Ogrenci)
                    .ThenInclude(o => o.Sinif)
                .Include(o => o.Kitap)
                .ToList(); // Belleğe alıyoruz, SQLite desteklemez çünkü

            // Gecikmiş kitapları filtrele ve viewmodel'e dönüştür
            var gecikenKitaplar = oduncKitaplar
                .Where(o => (DateTime.Now - o.OduncAlmaTarihi).TotalDays > 14) // 14 gün örnek ödünç süresi, istersen değiştir
                .Select(o => new GecikenKitapViewModel
                {
                    OgrenciId = o.OgrenciId ?? 0,
                    OgrenciAdi = o.Ogrenci?.OgrenciAdi ?? "Bilinmiyor",
                    OgrenciSoyadi = o.Ogrenci?.OgrenciSoyadi ?? "Bilinmiyor",
                    SinifAdi = o.Ogrenci?.Sinif?.SinifAdi ?? "Bilinmiyor",
                    KitapId = o.KitapId,
                    KitapAdi = o.Kitap?.KitapAdi ?? "Bilinmiyor",
                    OduncAlmaTarihi = o.OduncAlmaTarihi,
                    GecikmeGunu = (int)((DateTime.Now - o.OduncAlmaTarihi).TotalDays - 14) // 14 gün ödünç süresi varsayıldı
                });

            // Sınıf filtresi varsa uygula
            if (sinifId.HasValue)
            {
                gecikenKitaplar = gecikenKitaplar
                    .Where(k => k.SinifAdi != null && 
                                _context.Siniflar.Any(s => s.Id == sinifId.Value && s.SinifAdi == k.SinifAdi));
            }

            return View(gecikenKitaplar.ToList());
        }

        public IActionResult OduncVerilenKitaplar(int? sinifId)
        {
            // Dropdown için sınıfları gönderiyoruz
            ViewBag.Siniflar = new SelectList(_context.Siniflar, "Id", "SinifAdi");

            // Teslim edilmemiş kitapları çek (öğrencide olanlar)
            var oduncKitaplar = _context.OduncKitaplar
                .Where(o => o.TeslimDurumu == false)
                .Include(o => o.Ogrenci)
                    .ThenInclude(o => o.Sinif)
                .Include(o => o.Kitap)
                .AsEnumerable(); // SQLite sınırlaması varsa ToList() da olur

            // Sınıf filtresi varsa uygula
            if (sinifId.HasValue)
            {
                oduncKitaplar = oduncKitaplar.Where(o => o.Ogrenci?.SinifId == sinifId.Value);
            }

            // ViewModel'e dönüştür
            var model = oduncKitaplar.Select(o => new GecikenKitapViewModel
            {
                OgrenciId = o.OgrenciId ?? 0,
                OgrenciAdi = o.Ogrenci?.OgrenciAdi ?? "Bilinmiyor",
                OgrenciSoyadi = o.Ogrenci?.OgrenciSoyadi ?? "Bilinmiyor",
                SinifAdi = o.Ogrenci?.Sinif?.SinifAdi ?? "Bilinmiyor",
                KitapId = o.KitapId,
                KitapAdi = o.Kitap?.KitapAdi ?? "Bilinmiyor",
                OduncAlmaTarihi = o.OduncAlmaTarihi,
                GecikmeGunu = 0 // Burada gecikme hesaplamaya gerek yok
            }).ToList();

            return View(model);
        }
        public IActionResult OgrencininKitaplari(int ogrenciId)
        {
            if (ogrenciId <= 0)
            {
                TempData["Hata"] = "Geçersiz öğrenci ID'si.";
                return RedirectToAction(nameof(Index));
            }

            var ogrenci = _context.Ogrenciler
                .Include(o => o.Sinif)
                .FirstOrDefault(o => o.Id == ogrenciId);

            if (ogrenci == null)
            {
                TempData["Hata"] = $"ID: {ogrenciId} olan öğrenci bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var kitaplar = _context.OduncKitaplar
                .Include(o => o.Kitap)
                .Where(o => o.OgrenciId == ogrenciId)
                .OrderByDescending(o => o.OduncAlmaTarihi)
                .ToList();

            ViewBag.OgrenciAdiSoyadi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
            ViewBag.SinifAdi = ogrenci.Sinif?.SinifAdi ?? "Sınıf Belirtilmemiş";
            ViewBag.ToplamKitap = kitaplar.Count;
            ViewBag.TeslimEdilmisKitap = kitaplar.Count(k => k.TeslimDurumu);
            ViewBag.TeslimEdilmemisKitap = kitaplar.Count(k => !k.TeslimDurumu);

            return View(kitaplar);
        }

        public IActionResult KitabiOkuyanOgrenciler(int? kitapId)
        {
            if (!kitapId.HasValue || kitapId <= 0)
            {
                TempData["Hata"] = "Geçersiz kitap ID'si.";
                return RedirectToAction(nameof(Index));
            }

            var kitap = _context.Kitaplar
                .FirstOrDefault(k => k.Id == kitapId);

            if (kitap == null)
            {
                TempData["Hata"] = $"ID: {kitapId} olan kitap bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var ogrenciler = _context.OduncKitaplar
                .Include(o => o.Ogrenci)
                    .ThenInclude(o => o.Sinif)
                .Where(o => o.KitapId == kitapId)
                .OrderByDescending(o => o.OduncAlmaTarihi)
                .ToList();

            ViewBag.KitapAdi = kitap.KitapAdi;
            ViewBag.Yazar = kitap.Yazar;
            ViewBag.ToplamOgrenci = ogrenciler.Count;
            ViewBag.TeslimEdilmis = ogrenciler.Count(o => o.TeslimDurumu);
            ViewBag.TeslimEdilmemis = ogrenciler.Count(o => !o.TeslimDurumu);

            return View(ogrenciler);
        }
    }
}