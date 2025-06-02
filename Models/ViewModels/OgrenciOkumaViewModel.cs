using System;

namespace Kutuphane.Models.ViewModels
{
    public class OgrenciKitapListesiViewModel
    {
        // ... existing code ...
        public int KitapId { get; set; }
        public string KitapAdi { get; set; } = string.Empty;
        public string Yazar { get; set; } = string.Empty;
        public DateTime OduncAlmaTarihi { get; set; }
        public DateTime? TeslimTarihi { get; set; }
        public bool TeslimDurumu { get; set; }
        // Add other relevant properties if needed, like due date, author, etc.
    }

    public class OgrenciOkumaViewModel
    {
        public int OgrenciId { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string SinifAdi { get; set; } = string.Empty;
        public int OkumaSayisi { get; set; }
    }
} 