namespace Kutuphane.Models.ViewModels
{
    public class GecikenKitapViewModel
    {
        public int OgrenciId { get; set; }
        public string OgrenciAdi { get; set; } = string.Empty;
        public string OgrenciSoyadi { get; set; } = string.Empty;
        public string SinifAdi { get; set; } = string.Empty;
        public int KitapId { get; set; }
        public string KitapAdi { get; set; } = string.Empty;
        public DateTime OduncAlmaTarihi { get; set; }
        public int GecikmeGunu { get; set; }
    }
}
