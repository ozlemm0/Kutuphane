using System;
using System.Collections.Generic;

namespace Kutuphane.Models.ViewModels
{
    public class RaporViewModel
    {
        public List<OgrenciOkumaViewModel> EnCokOkuyanOgrenciler { get; set; } = new();
        public List<KitapOkunmaViewModel> EnCokOkunanKitaplar { get; set; } = new();
        public List<OduncKitapViewModel> OduncVerilenKitaplar { get; set; } = new();
        public List<OduncKitapViewModel> SecilenOgrencininKitaplari { get; set; } = new();
        public int? SecilenOgrenciId { get; set; }
        public int? SecilenKitapId { get; set; }
        public string SecilenOgrenciAdSoyad { get; set; } = string.Empty;
    }

    public class KitapOkunmaViewModel
    {
        public int KitapId { get; set; }
        public string KitapAdi { get; set; } = string.Empty;
        public string Yazar { get; set; } = string.Empty;
        public int OkunmaSayisi { get; set; }
    }

    public class OduncKitapViewModel
    {
        public int Id { get; set; }
        public string KitapAdi { get; set; } = string.Empty;
        public string OgrenciAdSoyad { get; set; } = string.Empty;
        public DateTime OduncAlmaTarihi { get; set; }
        public DateTime? TeslimTarihi { get; set; }
        public bool TeslimDurumu { get; set; }
    }
} 