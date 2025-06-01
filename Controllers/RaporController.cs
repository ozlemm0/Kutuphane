using Kutuphane.Data;
using Kutuphane.Models;
using Kutuphane.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Kutuphane.Controllers
{
    [Authorize]
    public class RaporController : Controller
    {
        private readonly KutuphaneDbContext _context;
        private readonly ILogger<RaporController> _logger;

        public RaporController(KutuphaneDbContext context, ILogger<RaporController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? secilenOgrenciId, int? secilenKitapId)
        {
            try
            {
                // Öğrenci listesini oluştur
                var ogrenciler = await _context.Ogrenciler
                    .OrderBy(o => o.OgrenciAdi)
                    .ThenBy(o => o.OgrenciSoyadi)
                    .Select(o => new
                    {
                        Id = o.Id,
                        OgrenciAdiSoyadi = o.OgrenciAdi + " " + o.OgrenciSoyadi
                    })
                    .ToListAsync();

                ViewBag.Ogrenciler = new SelectList(ogrenciler, "Id", "OgrenciAdiSoyadi", secilenOgrenciId);

                // Kitap listesini oluştur
                var kitaplar = await _context.Kitaplar
                    .OrderBy(k => k.KitapAdi)
                    .Select(k => new
                    {
                        Id = k.Id,
                        KitapAdi = k.KitapAdi + " - " + k.Yazar
                    })
                    .ToListAsync();

                ViewBag.Kitaplar = new SelectList(kitaplar, "Id", "KitapAdi", secilenKitapId);

                var enCokOkuyanOgrenciler = await _context.OduncKitaplar
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
                    .ToListAsync();

                var enCokOkunanKitaplar = await _context.OduncKitaplar
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
                    .ToListAsync();

                var model = new RaporViewModel
                {
                    EnCokOkuyanOgrenciler = enCokOkuyanOgrenciler,
                    EnCokOkunanKitaplar = enCokOkunanKitaplar,
                    SecilenOgrenciId = secilenOgrenciId,
                    SecilenKitapId = secilenKitapId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rapor ana sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Rapor yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> EnCokOkuyanOgrenciler(int? sinifId = null, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            try
            {
                var query = _context.OduncKitaplar
                    .Include(o => o.Ogrenci)
                        .ThenInclude(o => o.Sinif)
                    .Where(o => o.TeslimDurumu);

                if (sinifId.HasValue && sinifId.Value > 0)
                {
                    query = query.Where(o => o.Ogrenci.SinifId == sinifId.Value);
                }

                if (baslangicTarihi.HasValue)
                {
                    query = query.Where(o => o.OduncAlmaTarihi >= baslangicTarihi.Value);
                }

                if (bitisTarihi.HasValue)
                {
                    query = query.Where(o => o.OduncAlmaTarihi <= bitisTarihi.Value);
                }

                var ogrenciOkumaSayilari = await query
                    .GroupBy(o => new { o.OgrenciId, o.Ogrenci.OgrenciAdi, o.Ogrenci.OgrenciSoyadi, o.Ogrenci.Sinif.SinifAdi })
                    .Select(g => new
                    {
                        OgrenciId = g.Key.OgrenciId,
                        OgrenciAdi = g.Key.OgrenciAdi ?? "Bilinmiyor",
                        OgrenciSoyadi = g.Key.OgrenciSoyadi ?? "Bilinmiyor",
                        SinifAdi = g.Key.SinifAdi ?? "Bilinmiyor",
                        OkumaSayisi = g.Count()
                    })
                    .OrderByDescending(x => x.OkumaSayisi)
                    .Take(10)
                    .ToListAsync();

                ViewBag.Siniflar = await _context.Siniflar.ToListAsync();
                ViewBag.SecilenSinifId = sinifId;
                ViewBag.BaslangicTarihi = baslangicTarihi;
                ViewBag.BitisTarihi = bitisTarihi;

                return View(ogrenciOkumaSayilari);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "En çok okuyan öğrenciler raporu oluşturulurken hata oluştu");
                TempData["Error"] = "Rapor oluşturulurken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> EnCokOkunanKitaplar(int? kategoriId = null, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            try
            {
                var query = _context.OduncKitaplar
                    .Include(o => o.Kitap)
                        .ThenInclude(k => k.Kategori)
                    .Where(o => o.TeslimDurumu);

                if (kategoriId.HasValue && kategoriId.Value > 0)
                {
                    query = query.Where(o => o.Kitap.KategoriId == kategoriId.Value);
                }

                if (baslangicTarihi.HasValue)
                {
                    query = query.Where(o => o.OduncAlmaTarihi >= baslangicTarihi.Value);
                }

                if (bitisTarihi.HasValue)
                {
                    query = query.Where(o => o.OduncAlmaTarihi <= bitisTarihi.Value);
                }

                var kitapOkumaSayilari = await query
                    .GroupBy(o => new { o.KitapId, o.Kitap.KitapAdi, o.Kitap.Kategori.KategoriAdi })
                    .Select(g => new
                    {
                        KitapId = g.Key.KitapId,
                        KitapAdi = g.Key.KitapAdi ?? "Bilinmiyor",
                        KategoriAdi = g.Key.KategoriAdi ?? "Bilinmiyor",
                        OkunmaSayisi = g.Count()
                    })
                    .OrderByDescending(x => x.OkunmaSayisi)
                    .Take(10)
                    .ToListAsync();

                ViewBag.Kategoriler = await _context.Kategoriler.ToListAsync();
                ViewBag.SecilenKategoriId = kategoriId;
                ViewBag.BaslangicTarihi = baslangicTarihi;
                ViewBag.BitisTarihi = bitisTarihi;

                return View(kitapOkumaSayilari);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "En çok okunan kitaplar raporu oluşturulurken hata oluştu");
                TempData["Error"] = "Rapor oluşturulurken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> GecikenKitaplar()
        {
            try
            {
                var gecikenKitaplar = await _context.OduncKitaplar
                    .Include(o => o.Kitap)
                    .Include(o => o.Ogrenci)
                        .ThenInclude(o => o.Sinif)
                    .Where(o => !o.TeslimDurumu && o.OduncAlmaTarihi.AddDays(14) < DateTime.Now)
                    .Select(o => new
                    {
                        o.Id,
                        KitapAdi = o.Kitap.KitapAdi ?? "Bilinmiyor",
                        OgrenciAdi = o.Ogrenci.OgrenciAdi ?? "Bilinmiyor",
                        OgrenciSoyadi = o.Ogrenci.OgrenciSoyadi ?? "Bilinmiyor",
                        SinifAdi = o.Ogrenci.Sinif.SinifAdi ?? "Bilinmiyor",
                        o.OduncAlmaTarihi,
                        GecikmeGunu = (DateTime.Now - o.OduncAlmaTarihi).Days - 14
                    })
                    .OrderByDescending(x => x.GecikmeGunu)
                    .ToListAsync();

                return View(gecikenKitaplar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geciken kitaplar raporu oluşturulurken hata oluştu");
                TempData["Error"] = "Rapor oluşturulurken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> OgrenciKitapListesi(int? sinifId = null)
        {
            try
            {
                var query = _context.OduncKitaplar
                    .Include(o => o.Kitap)
                    .Include(o => o.Ogrenci)
                        .ThenInclude(o => o.Sinif)
                    .Where(o => !o.TeslimDurumu);

                if (sinifId.HasValue && sinifId.Value > 0)
                {
                    query = query.Where(o => o.Ogrenci.SinifId == sinifId.Value);
                }

                var ogrenciKitaplari = await query
                    .Select(o => new
                    {
                        o.Id,
                        KitapAdi = o.Kitap.KitapAdi ?? "Bilinmiyor",
                        OgrenciAdi = o.Ogrenci.OgrenciAdi ?? "Bilinmiyor",
                        OgrenciSoyadi = o.Ogrenci.OgrenciSoyadi ?? "Bilinmiyor",
                        SinifAdi = o.Ogrenci.Sinif.SinifAdi ?? "Bilinmiyor",
                        o.OduncAlmaTarihi
                    })
                    .OrderBy(x => x.SinifAdi)
                    .ThenBy(x => x.OgrenciAdi)
                    .ToListAsync();

                ViewBag.Siniflar = await _context.Siniflar.ToListAsync();
                ViewBag.SecilenSinifId = sinifId;

                return View(ogrenciKitaplari);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Öğrenci kitap listesi raporu oluşturulurken hata oluştu");
                TempData["Error"] = "Rapor oluşturulurken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> OduncVerilenKitaplar(int? sinifId)
        {
            try
            {
                // Dropdown için sınıfları gönderiyoruz
                ViewBag.Siniflar = new SelectList(await _context.Siniflar.ToListAsync(), "Id", "SinifAdi");

                // Teslim edilmemiş kitapları çek (öğrencide olanlar)
                IQueryable<OduncKitap> query = _context.OduncKitaplar
                    .Where(o => !o.TeslimDurumu)
                    .Include(o => o.Ogrenci)
                        .ThenInclude(o => o.Sinif)
                    .Include(o => o.Kitap);

                // Sınıf filtresi varsa uygula
                if (sinifId.HasValue && sinifId.Value > 0)
                {
                    query = query.Where(o => o.Ogrenci != null && o.Ogrenci.SinifId == sinifId.Value);
                }

                var oduncKitaplar = await query.ToListAsync();

                // ViewModel'e dönüştür
                var model = oduncKitaplar.Select(o => new GecikenKitapViewModel
                {
                    OgrenciId = o.OgrenciId,
                    OgrenciAdi = o.Ogrenci?.OgrenciAdi ?? "Bilinmiyor",
                    OgrenciSoyadi = o.Ogrenci?.OgrenciSoyadi ?? "Bilinmiyor",
                    SinifAdi = o.Ogrenci?.Sinif?.SinifAdi ?? "Bilinmiyor",
                    KitapId = o.KitapId,
                    KitapAdi = o.Kitap?.KitapAdi ?? "Bilinmiyor",
                    OduncAlmaTarihi = o.OduncAlmaTarihi,
                    GecikmeGunu = 0
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödünç verilen kitaplar raporu oluşturulurken hata oluştu");
                TempData["Error"] = "Rapor oluşturulurken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> OgrencininKitaplari(int ogrenciId)
        {
            try
            {
                if (ogrenciId <= 0)
                {
                    TempData["Hata"] = "Geçersiz öğrenci ID'si.";
                    return RedirectToAction(nameof(Index));
                }

                var ogrenci = await _context.Ogrenciler
                    .Include(o => o.Sinif)
                    .FirstOrDefaultAsync(o => o.Id == ogrenciId);

                if (ogrenci == null)
                {
                    TempData["Hata"] = $"ID: {ogrenciId} olan öğrenci bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                var kitaplar = await _context.OduncKitaplar
                    .Include(o => o.Kitap)
                    .Where(o => o.OgrenciId == ogrenciId)
                    .OrderByDescending(o => o.OduncAlmaTarihi)
                    .ToListAsync();

                ViewBag.OgrenciAdiSoyadi = $"{ogrenci.OgrenciAdi} {ogrenci.OgrenciSoyadi}";
                ViewBag.SinifAdi = ogrenci.Sinif?.SinifAdi ?? "Sınıf Belirtilmemiş";
                ViewBag.ToplamKitap = kitaplar.Count;
                ViewBag.TeslimEdilmisKitap = kitaplar.Count(k => k.TeslimDurumu);
                ViewBag.TeslimEdilmemisKitap = kitaplar.Count(k => !k.TeslimDurumu);

                return View(kitaplar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Öğrencinin kitapları raporu oluşturulurken hata oluştu");
                TempData["Error"] = "Rapor oluşturulurken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> KitabiOkuyanOgrenciler(int? kitapId)
        {
            try
            {
                if (!kitapId.HasValue || kitapId.Value <= 0)
                {
                    TempData["Hata"] = "Geçersiz kitap ID'si.";
                    return RedirectToAction(nameof(Index));
                }

                var kitap = await _context.Kitaplar
                    .FirstOrDefaultAsync(k => k.Id == kitapId.Value);

                if (kitap == null)
                {
                    TempData["Hata"] = $"ID: {kitapId} olan kitap bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                var ogrenciler = await _context.OduncKitaplar
                    .Include(o => o.Ogrenci)
                        .ThenInclude(o => o.Sinif)
                    .Where(o => o.KitapId == kitapId.Value && o.Ogrenci != null && o.Ogrenci.AktifMi)
                    .OrderByDescending(o => o.OduncAlmaTarihi)
                    .ToListAsync();

                ViewBag.KitapAdi = kitap.KitapAdi;
                ViewBag.Yazar = kitap.Yazar;
                ViewBag.ToplamOgrenci = ogrenciler.Count;
                ViewBag.TeslimEdilmis = ogrenciler.Count(o => o.TeslimDurumu);
                ViewBag.TeslimEdilmemis = ogrenciler.Count(o => !o.TeslimDurumu);

                return View(ogrenciler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitabı okuyan öğrenciler raporu oluşturulurken hata oluştu");
                TempData["Error"] = "Rapor oluşturulurken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}